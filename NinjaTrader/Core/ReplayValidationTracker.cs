using System;
using System.Globalization;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ReplayValidationTracker
    {
        public const string ProgressPrefix = "REPLAY_VALIDATION_PROGRESS=";
        public const string SummaryPrefix = "REPLAY_VALIDATION_SUMMARY=";

        public ReplayValidationTracker()
        {
            MinimumClosedOutcomesForReview = 50;
            MinimumBarsForReview = 500;
        }

        public ReplayValidationSession Session { get; private set; }
        public int JournaledCandidates { get; private set; }
        public int ConfirmedCandidates { get; private set; }
        public int WeakConfirmations { get; private set; }
        public int NoConfirmations { get; private set; }
        public int MinimumClosedOutcomesForReview { get; set; }
        public int MinimumBarsForReview { get; set; }

        public void StartSession(
            string sessionLabel,
            string instrument,
            string strategyName,
            DateTime startedAt,
            string dataMode,
            string barType,
            string timeframe,
            string tradingHoursTemplate,
            bool evaluationOnlyMode,
            bool executionDisabled,
            bool usesApproximateOrderFlow,
            bool usesApproximateVolumeProfile,
            int startingBar)
        {
            string label = string.IsNullOrEmpty(sessionLabel)
                ? startedAt.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)
                : sessionLabel;

            Session = new ReplayValidationSession
            {
                SessionId = strategyName + "-" + instrument + "-" + label,
                Instrument = instrument,
                StrategyName = strategyName,
                StartedAt = startedAt,
                DataMode = dataMode,
                BarType = barType,
                Timeframe = timeframe,
                TradingHoursTemplate = tradingHoursTemplate,
                EvaluationOnlyMode = evaluationOnlyMode,
                ExecutionDisabled = executionDisabled,
                UsesApproximateOrderFlow = usesApproximateOrderFlow,
                UsesApproximateVolumeProfile = usesApproximateVolumeProfile,
                StartingBar = startingBar,
                EndingBar = startingBar,
                TotalBarsProcessed = 0,
                Notes = "NT-4D replay validation started; observation-only and non-executable."
            };
        }

        public void RecordBar(
            int currentBar,
            DateTime timestamp,
            bool journaledCandidate,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot)
        {
            EnsureSession(timestamp, string.Empty, string.Empty);
            Session.EndingBar = currentBar;
            Session.TotalBarsProcessed = Math.Max(0, currentBar - Session.StartingBar + 1);

            if (journaledCandidate)
            {
                JournaledCandidates++;
            }

            if (candidateSnapshot == null || !candidateSnapshot.HasCandidate || confirmationSnapshot == null)
            {
                return;
            }

            if (confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.ConfirmationObserved)
            {
                ConfirmedCandidates++;
            }
            else if (confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.WeakConfirmation)
            {
                WeakConfirmations++;
            }
            else if (confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.NoConfirmation)
            {
                NoConfirmations++;
            }
        }

        public ReplayValidationSummary BuildSummary(
            HypotheticalPerformanceSummary performanceSummary,
            int openOutcomes,
            DateTime endedAt,
            int minimumClosedOutcomesForReview,
            int minimumBarsForReview)
        {
            EnsureSession(endedAt, string.Empty, string.Empty);
            Session.EndedAt = endedAt;
            MinimumClosedOutcomesForReview = Math.Max(1, minimumClosedOutcomesForReview);
            MinimumBarsForReview = Math.Max(1, minimumBarsForReview);

            HypotheticalPerformanceSummary safeSummary = performanceSummary ?? new HypotheticalPerformanceSummary();
            ReplayValidationSummary summary = new ReplayValidationSummary
            {
                SessionId = Session.SessionId,
                Instrument = Session.Instrument,
                StartedAt = Session.StartedAt,
                EndedAt = endedAt,
                TotalBarsProcessed = Session.TotalBarsProcessed,
                JournaledCandidates = JournaledCandidates,
                ConfirmedCandidates = ConfirmedCandidates,
                WeakConfirmations = WeakConfirmations,
                NoConfirmations = NoConfirmations,
                OpenOutcomes = openOutcomes,
                ClosedOutcomes = safeSummary.TotalClosedOutcomes,
                TargetHits = safeSummary.TargetHits,
                StopHits = safeSummary.StopHits,
                Timeouts = safeSummary.Timeouts,
                WinRate = safeSummary.WinRate,
                AverageR = safeSummary.AverageR,
                TotalR = safeSummary.TotalR,
                BestSetupType = safeSummary.BestSetupType,
                WorstSetupType = safeSummary.WorstSetupType,
                SummaryReason = "NT-4D replay validation is observation-only, non-executable, and does not prove profitability."
            };

            summary.IsReviewable = summary.TotalBarsProcessed >= MinimumBarsForReview
                && summary.ClosedOutcomes >= MinimumClosedOutcomesForReview;
            summary.ReviewWarnings = BuildWarnings(summary);
            return summary;
        }

        public bool IsReviewable(
            HypotheticalPerformanceSummary performanceSummary,
            int minimumClosedOutcomesForReview,
            int minimumBarsForReview)
        {
            HypotheticalPerformanceSummary safeSummary = performanceSummary ?? new HypotheticalPerformanceSummary();
            return Session != null
                && Session.TotalBarsProcessed >= Math.Max(1, minimumBarsForReview)
                && safeSummary.TotalClosedOutcomes >= Math.Max(1, minimumClosedOutcomesForReview);
        }

        public void PrintProgress(
            HypotheticalPerformanceSummary performanceSummary,
            int openOutcomes,
            DateTime timestamp,
            int minimumClosedOutcomesForReview,
            int minimumBarsForReview,
            Action<string> print)
        {
            ReplayValidationSummary summary = BuildSummary(
                performanceSummary,
                openOutcomes,
                timestamp,
                minimumClosedOutcomesForReview,
                minimumBarsForReview);

            SafePrint(print, ProgressPrefix
                + "SessionId=" + summary.SessionId
                + " Instrument=" + summary.Instrument
                + " Bars=" + summary.TotalBarsProcessed
                + " JournaledCandidates=" + summary.JournaledCandidates
                + " Confirmed=" + summary.ConfirmedCandidates
                + " Weak=" + summary.WeakConfirmations
                + " NoConfirmation=" + summary.NoConfirmations
                + " ClosedOutcomes=" + summary.ClosedOutcomes
                + " WinRate=" + Format(summary.WinRate)
                + " AvgR=" + Format(summary.AverageR)
                + " Reviewable=" + summary.IsReviewable);
        }

        public void PrintFinalSummary(
            HypotheticalPerformanceSummary performanceSummary,
            int openOutcomes,
            DateTime timestamp,
            int minimumClosedOutcomesForReview,
            int minimumBarsForReview,
            Action<string> print)
        {
            ReplayValidationSummary summary = BuildSummary(
                performanceSummary,
                openOutcomes,
                timestamp,
                minimumClosedOutcomesForReview,
                minimumBarsForReview);

            SafePrint(print, SummaryPrefix
                + "SessionId=" + summary.SessionId
                + " Instrument=" + summary.Instrument
                + " Bars=" + summary.TotalBarsProcessed
                + " Candidates=" + summary.JournaledCandidates
                + " Confirmed=" + summary.ConfirmedCandidates
                + " Weak=" + summary.WeakConfirmations
                + " NoConfirmation=" + summary.NoConfirmations
                + " ClosedOutcomes=" + summary.ClosedOutcomes
                + " TargetHits=" + summary.TargetHits
                + " StopHits=" + summary.StopHits
                + " Timeouts=" + summary.Timeouts
                + " WinRate=" + Format(summary.WinRate)
                + " AvgR=" + Format(summary.AverageR)
                + " TotalR=" + Format(summary.TotalR)
                + " BestSetup=" + summary.BestSetupType
                + " WorstSetup=" + summary.WorstSetupType
                + " Reviewable=" + summary.IsReviewable
                + " Warnings=" + summary.ReviewWarnings);
        }

        private void EnsureSession(DateTime timestamp, string instrument, string strategyName)
        {
            if (Session != null)
            {
                return;
            }

            StartSession(
                string.Empty,
                instrument,
                strategyName,
                timestamp,
                "Unknown",
                "Unknown",
                "Unknown",
                "Unknown",
                true,
                true,
                true,
                true,
                0);
        }

        private string BuildWarnings(ReplayValidationSummary summary)
        {
            string warnings = string.Empty;
            if (summary.TotalBarsProcessed < MinimumBarsForReview)
            {
                warnings = AppendWarning(warnings, "MinimumBarsForReview not met");
            }

            if (summary.ClosedOutcomes < MinimumClosedOutcomesForReview)
            {
                warnings = AppendWarning(warnings, "MinimumClosedOutcomesForReview not met");
            }

            return string.IsNullOrEmpty(warnings) ? "None" : warnings;
        }

        private static string AppendWarning(string current, string warning)
        {
            return string.IsNullOrEmpty(current) ? warning : current + "|" + warning;
        }

        private static string Format(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
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
