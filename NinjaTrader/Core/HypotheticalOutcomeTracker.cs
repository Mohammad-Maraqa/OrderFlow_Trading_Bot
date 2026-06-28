using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HypotheticalOutcomeTracker
    {
        public const string OpenedPrefix = "HYPOTHETICAL_OUTCOME_OPENED=";
        public const string ClosedPrefix = "HYPOTHETICAL_OUTCOME_CLOSED=";
        private readonly Dictionary<string, HypotheticalSignalOutcome> active =
            new Dictionary<string, HypotheticalSignalOutcome>();

        public int OpenOutcomeCount
        {
            get { return active.Count; }
        }

        public int ClosedOutcomeCount { get; private set; }

        public string LastOutcome { get; private set; }

        public bool TryOpenFromObservation(
            SignalObservationRecord record,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            int maxBarsToTrack,
            bool trackWeakConfirmations,
            bool printOutcomeEvents,
            string journalFileName,
            Action<string> print)
        {
            if (record == null || candidateSnapshot == null || confirmationSnapshot == null)
            {
                return false;
            }

            if (!candidateSnapshot.HasCandidate)
            {
                return false;
            }

            if (!IsTrackableConfirmation(confirmationSnapshot, trackWeakConfirmations))
            {
                return false;
            }

            if (!HasValidLongRisk(candidateSnapshot))
            {
                SafePrint(print, "Hypothetical outcome not opened: invalid long-only candidate risk.");
                return false;
            }

            if (active.ContainsKey(record.RecordId))
            {
                return false;
            }

            HypotheticalSignalOutcome outcome = new HypotheticalSignalOutcome
            {
                RecordId = record.RecordId,
                Instrument = record.Instrument,
                SignalTime = record.Timestamp,
                SignalBar = record.CurrentBar,
                CandidateSetupType = record.CandidateSetupType,
                ConfirmationState = record.ConfirmationState,
                ConfirmationType = record.ConfirmationType,
                EntryPrice = candidateSnapshot.CandidateEntryPrice,
                StopPrice = candidateSnapshot.CandidateStopPrice,
                TargetPrice = candidateSnapshot.CandidateTargetPrice,
                RewardRisk = candidateSnapshot.CandidateRewardRisk,
                BarsOpen = 0,
                MaxBarsToTrack = Math.Max(1, maxBarsToTrack),
                MaxFavorableExcursion = 0,
                MaxAdverseExcursion = 0,
                MaxFavorableR = 0,
                MaxAdverseR = 0,
                OutcomeState = HypotheticalOutcomeState.Open.ToString(),
                OutcomeReason = "NT-4B hypothetical outcome tracking opened; observation-only and non-executable.",
                IsClosed = false,
                ExecutionDisabled = true,
                EvaluationOnlyMode = true
            };

            active[outcome.RecordId] = outcome;
            WriteEvent("OUTCOME_OPENED", outcome, journalFileName, OpenedPrefix, print);

            if (printOutcomeEvents)
            {
                SafePrint(print, "Hypothetical outcome opened: RecordId=" + outcome.RecordId
                    + " Candidate=" + outcome.CandidateSetupType
                    + " ConfirmationState=" + outcome.ConfirmationState);
            }

            return true;
        }

        public List<HypotheticalSignalOutcome> UpdateOpenOutcomes(
            double highPrice,
            double lowPrice,
            double closePrice,
            int currentBar,
            DateTime timestamp,
            bool ConservativeSameBarResolution,
            bool printOutcomeEvents,
            string journalFileName,
            Action<string> print)
        {
            List<string> closedKeys = new List<string>();
            List<HypotheticalSignalOutcome> closedOutcomes = new List<HypotheticalSignalOutcome>();
            foreach (KeyValuePair<string, HypotheticalSignalOutcome> item in active)
            {
                HypotheticalSignalOutcome outcome = item.Value;
                UpdateExcursions(outcome, highPrice, lowPrice);
                outcome.BarsOpen = Math.Max(0, currentBar - outcome.SignalBar);

                bool targetTouched = highPrice >= outcome.TargetPrice;
                bool stopTouched = lowPrice <= outcome.StopPrice;

                if (ConservativeSameBarResolution && targetTouched && stopTouched)
                {
                    CloseOutcome(
                        outcome,
                        HypotheticalOutcomeState.StopHit,
                        currentBar,
                        timestamp,
                        outcome.StopPrice,
                        "Same-bar target and stop touched; ConservativeSameBarResolution assumes stop first.");
                }
                else if (stopTouched)
                {
                    CloseOutcome(
                        outcome,
                        HypotheticalOutcomeState.StopHit,
                        currentBar,
                        timestamp,
                        outcome.StopPrice,
                        "Hypothetical long stop touched.");
                }
                else if (targetTouched)
                {
                    CloseOutcome(
                        outcome,
                        HypotheticalOutcomeState.TargetHit,
                        currentBar,
                        timestamp,
                        outcome.TargetPrice,
                        "Hypothetical long target touched.");
                }
                else if (outcome.BarsOpen >= outcome.MaxBarsToTrack)
                {
                    CloseOutcome(
                        outcome,
                        HypotheticalOutcomeState.Timeout,
                        currentBar,
                        timestamp,
                        closePrice,
                        "Hypothetical tracking timed out before target or stop.");
                }

                if (outcome.IsClosed)
                {
                    closedKeys.Add(item.Key);
                    closedOutcomes.Add(outcome);
                    ClosedOutcomeCount++;
                    LastOutcome = outcome.OutcomeState;
                    WriteEvent("OUTCOME_CLOSED", outcome, journalFileName, ClosedPrefix, print);

                    if (printOutcomeEvents)
                    {
                        SafePrint(print, "Hypothetical outcome closed: RecordId=" + outcome.RecordId
                            + " OutcomeState=" + outcome.OutcomeState
                            + " BarsOpen=" + outcome.BarsOpen
                            + " MaxFavorableR=" + Format(outcome.MaxFavorableR)
                            + " MaxAdverseR=" + Format(outcome.MaxAdverseR));
                    }
                }
            }

            foreach (string key in closedKeys)
            {
                active.Remove(key);
            }

            return closedOutcomes;
        }

        private static bool IsTrackableConfirmation(
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            bool trackWeakConfirmations)
        {
            if (confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.ConfirmationObserved)
            {
                return true;
            }

            return trackWeakConfirmations
                && confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.WeakConfirmation;
        }

        private static bool HasValidLongRisk(LongSetupCandidateSnapshot candidateSnapshot)
        {
            return candidateSnapshot.CandidateEntryPrice > 0
                && candidateSnapshot.CandidateStopPrice > 0
                && candidateSnapshot.CandidateTargetPrice > 0
                && candidateSnapshot.CandidateStopPrice < candidateSnapshot.CandidateEntryPrice
                && candidateSnapshot.CandidateTargetPrice > candidateSnapshot.CandidateEntryPrice
                && candidateSnapshot.CandidateRewardRisk > 0;
        }

        private static void UpdateExcursions(
            HypotheticalSignalOutcome outcome,
            double highPrice,
            double lowPrice)
        {
            double risk = Math.Max(0.00000001, outcome.EntryPrice - outcome.StopPrice);
            double favorable = Math.Max(0, highPrice - outcome.EntryPrice);
            double adverse = Math.Max(0, outcome.EntryPrice - lowPrice);

            outcome.MaxFavorableExcursion = Math.Max(outcome.MaxFavorableExcursion, favorable);
            outcome.MaxAdverseExcursion = Math.Max(outcome.MaxAdverseExcursion, adverse);
            outcome.MaxFavorableR = outcome.MaxFavorableExcursion / risk;
            outcome.MaxAdverseR = outcome.MaxAdverseExcursion / risk;
        }

        private static void CloseOutcome(
            HypotheticalSignalOutcome outcome,
            HypotheticalOutcomeState state,
            int currentBar,
            DateTime timestamp,
            double price,
            string reason)
        {
            outcome.OutcomeState = state.ToString();
            outcome.OutcomeTime = timestamp;
            outcome.OutcomeBar = currentBar;
            outcome.OutcomePrice = price;
            outcome.OutcomeReason = reason;
            outcome.IsClosed = true;
            outcome.ExecutionDisabled = true;
            outcome.EvaluationOnlyMode = true;
        }

        private static void WriteEvent(
            string eventType,
            HypotheticalSignalOutcome outcome,
            string journalFileName,
            string fallbackPrefix,
            Action<string> print)
        {
            string jsonLine = ToJsonLine(eventType, outcome);
            try
            {
                string path = ResolvePath(journalFileName);
                File.AppendAllText(path, jsonLine + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception error)
            {
                SafePrint(print, "Hypothetical outcome journal warning: file write failed: "
                    + error.Message);
                SafePrint(print, fallbackPrefix + jsonLine);
            }
        }

        private static string ToJsonLine(string eventType, HypotheticalSignalOutcome outcome)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            Append(builder, "event_type", eventType).Append(",");
            Append(builder, "record_id", outcome.RecordId).Append(",");
            Append(builder, "instrument", outcome.Instrument).Append(",");
            Append(builder, "signal_time", outcome.SignalTime.ToString("O", CultureInfo.InvariantCulture)).Append(",");
            Append(builder, "signal_bar", outcome.SignalBar).Append(",");
            Append(builder, "candidate_setup_type", outcome.CandidateSetupType).Append(",");
            Append(builder, "confirmation_state", outcome.ConfirmationState).Append(",");
            Append(builder, "confirmation_type", outcome.ConfirmationType).Append(",");
            Append(builder, "entry_price", outcome.EntryPrice).Append(",");
            Append(builder, "stop_price", outcome.StopPrice).Append(",");
            Append(builder, "target_price", outcome.TargetPrice).Append(",");
            Append(builder, "reward_risk", outcome.RewardRisk).Append(",");
            Append(builder, "bars_open", outcome.BarsOpen).Append(",");
            Append(builder, "max_bars_to_track", outcome.MaxBarsToTrack).Append(",");
            Append(builder, "max_favorable_excursion", outcome.MaxFavorableExcursion).Append(",");
            Append(builder, "max_adverse_excursion", outcome.MaxAdverseExcursion).Append(",");
            Append(builder, "max_favorable_r", outcome.MaxFavorableR).Append(",");
            Append(builder, "max_adverse_r", outcome.MaxAdverseR).Append(",");
            Append(builder, "outcome_state", outcome.OutcomeState).Append(",");
            Append(builder, "outcome_time", outcome.OutcomeTime.HasValue
                ? outcome.OutcomeTime.Value.ToString("O", CultureInfo.InvariantCulture)
                : string.Empty).Append(",");
            Append(builder, "outcome_bar", outcome.OutcomeBar.HasValue ? outcome.OutcomeBar.Value : -1).Append(",");
            Append(builder, "outcome_price", outcome.OutcomePrice.HasValue ? outcome.OutcomePrice.Value : 0).Append(",");
            Append(builder, "outcome_reason", outcome.OutcomeReason).Append(",");
            Append(builder, "is_closed", outcome.IsClosed).Append(",");
            Append(builder, "execution_disabled", outcome.ExecutionDisabled).Append(",");
            Append(builder, "evaluation_only_mode", outcome.EvaluationOnlyMode);
            builder.Append("}");
            return builder.ToString();
        }

        private static string ResolvePath(string journalFileName)
        {
            string safeFileName = string.IsNullOrEmpty(journalFileName)
                ? SignalObservationJournalWriter.DefaultFileName
                : journalFileName;

            if (Path.IsPathRooted(safeFileName))
            {
                return safeFileName;
            }

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(folder, safeFileName);
        }

        private static StringBuilder Append(StringBuilder builder, string key, string value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append("\"").Append(Escape(value)).Append("\"");
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, double value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(Format(value));
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, int value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
            return builder;
        }

        private static StringBuilder Append(StringBuilder builder, string key, bool value)
        {
            builder.Append("\"").Append(Escape(key)).Append("\":");
            builder.Append(value ? "true" : "false");
            return builder;
        }

        private static string Format(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static void SafePrint(Action<string> print, string message)
        {
            if (print != null)
            {
                print(message);
            }
        }
    }
}
