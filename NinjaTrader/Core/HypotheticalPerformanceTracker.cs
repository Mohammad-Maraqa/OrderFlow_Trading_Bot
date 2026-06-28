using System;
using System.Collections.Generic;
using System.Globalization;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HypotheticalPerformanceTracker
    {
        public const string SummaryPrefix = "PERFORMANCE_SUMMARY=";
        public const string SetupStatsPrefix = "SETUP_STATS=";
        private readonly Dictionary<string, SetupPerformanceStats> bySetup =
            new Dictionary<string, SetupPerformanceStats>();

        public HypotheticalPerformanceTracker()
        {
            Summary = new HypotheticalPerformanceSummary();
            DefaultTargetRewardR = 2.0;
            TimeoutResultR = 0.0;
            InvalidatedResultR = 0.0;
            BestR = double.MinValue;
            WorstR = double.MaxValue;
            LastPrintedSummaryClosedOutcomes = 0;
        }

        public HypotheticalPerformanceSummary Summary { get; private set; }
        public double DefaultTargetRewardR { get; set; }
        public double TimeoutResultR { get; set; }
        public double InvalidatedResultR { get; set; }
        private double BestR { get; set; }
        private double WorstR { get; set; }
        private int LastPrintedSummaryClosedOutcomes { get; set; }

        public void RecordClosedOutcomes(
            IList<HypotheticalSignalOutcome> outcomes,
            int openOutcomes,
            double timeoutResultR,
            double invalidatedResultR,
            double defaultTargetRewardR)
        {
            if (outcomes == null)
            {
                return;
            }

            for (int index = 0; index < outcomes.Count; index++)
            {
                RecordClosedOutcome(
                    outcomes[index],
                    openOutcomes,
                    timeoutResultR,
                    invalidatedResultR,
                    defaultTargetRewardR);
            }
        }

        public void RecordClosedOutcome(
            HypotheticalSignalOutcome outcome,
            int openOutcomes,
            double timeoutResultR,
            double invalidatedResultR,
            double defaultTargetRewardR)
        {
            if (outcome == null || !outcome.IsClosed)
            {
                return;
            }

            TimeoutResultR = timeoutResultR;
            InvalidatedResultR = invalidatedResultR;
            DefaultTargetRewardR = defaultTargetRewardR <= 0 ? 2.0 : defaultTargetRewardR;

            double resultR = CalculateResultR(outcome);
            Summary.TotalClosedOutcomes++;
            Summary.OpenOutcomes = openOutcomes;
            Summary.TotalR += resultR;
            Summary.AverageMaxFavorableR = RunningAverage(
                Summary.AverageMaxFavorableR,
                outcome.MaxFavorableR,
                Summary.TotalClosedOutcomes);
            Summary.AverageMaxAdverseR = RunningAverage(
                Summary.AverageMaxAdverseR,
                outcome.MaxAdverseR,
                Summary.TotalClosedOutcomes);

            if (outcome.OutcomeState == HypotheticalOutcomeState.TargetHit.ToString())
            {
                Summary.TargetHits++;
            }
            else if (outcome.OutcomeState == HypotheticalOutcomeState.StopHit.ToString())
            {
                Summary.StopHits++;
            }
            else if (outcome.OutcomeState == HypotheticalOutcomeState.Timeout.ToString())
            {
                Summary.Timeouts++;
            }
            else if (outcome.OutcomeState == HypotheticalOutcomeState.Invalidated.ToString())
            {
                Summary.Invalidated++;
            }

            BestR = Math.Max(BestR, resultR);
            WorstR = Math.Min(WorstR, resultR);
            Summary.BestR = BestR == double.MinValue ? 0 : BestR;
            Summary.WorstR = WorstR == double.MaxValue ? 0 : WorstR;
            Summary.AverageR = Summary.TotalClosedOutcomes == 0
                ? 0
                : Summary.TotalR / Summary.TotalClosedOutcomes;
            Summary.WinRate = Rate(Summary.TargetHits, Summary.TotalClosedOutcomes);
            Summary.StopRate = Rate(Summary.StopHits, Summary.TotalClosedOutcomes);
            Summary.TimeoutRate = Rate(Summary.Timeouts, Summary.TotalClosedOutcomes);
            Summary.LastUpdated = outcome.OutcomeTime.HasValue
                ? outcome.OutcomeTime.Value
                : DateTime.MinValue;
            Summary.SummaryReason = "NT-4C performance summary is hypothetical, observation-only, and non-executable.";

            UpdateSetupStats(outcome, resultR);
            UpdateBestAndWorstSetupTypes();
        }

        public bool ShouldPrintSummary(int everyClosedOutcomes)
        {
            int every = Math.Max(1, everyClosedOutcomes);
            return Summary.TotalClosedOutcomes > 0
                && Summary.TotalClosedOutcomes - LastPrintedSummaryClosedOutcomes >= every;
        }

        public List<SetupPerformanceStats> GetSetupStats()
        {
            List<SetupPerformanceStats> stats = new List<SetupPerformanceStats>();
            foreach (KeyValuePair<string, SetupPerformanceStats> item in bySetup)
            {
                SetupPerformanceStats source = item.Value;
                stats.Add(new SetupPerformanceStats
                {
                    SetupType = source.SetupType,
                    Total = source.Total,
                    TargetHits = source.TargetHits,
                    StopHits = source.StopHits,
                    Timeouts = source.Timeouts,
                    WinRate = source.WinRate,
                    AverageR = source.AverageR,
                    TotalR = source.TotalR,
                    AverageMfeR = source.AverageMfeR,
                    AverageMaeR = source.AverageMaeR,
                    BestR = source.BestR,
                    WorstR = source.WorstR
                });
            }

            return stats;
        }

        public void PrintSummary(bool printSetupBreakdown, Action<string> print)
        {
            LastPrintedSummaryClosedOutcomes = Summary.TotalClosedOutcomes;
            SafePrint(print, SummaryPrefix
                + "Total=" + Summary.TotalClosedOutcomes
                + " TargetHits=" + Summary.TargetHits
                + " StopHits=" + Summary.StopHits
                + " Timeouts=" + Summary.Timeouts
                + " WinRate=" + Format(Summary.WinRate)
                + " AvgR=" + Format(Summary.AverageR)
                + " TotalR=" + Format(Summary.TotalR)
                + " BestSetup=" + Summary.BestSetupType
                + " WorstSetup=" + Summary.WorstSetupType);

            if (!printSetupBreakdown)
            {
                return;
            }

            foreach (KeyValuePair<string, SetupPerformanceStats> item in bySetup)
            {
                SetupPerformanceStats stats = item.Value;
                SafePrint(print, SetupStatsPrefix
                    + "Setup=" + stats.SetupType
                    + " Total=" + stats.Total
                    + " WinRate=" + Format(stats.WinRate)
                    + " AvgR=" + Format(stats.AverageR)
                    + " TotalR=" + Format(stats.TotalR));
            }
        }

        private double CalculateResultR(HypotheticalSignalOutcome outcome)
        {
            if (outcome.OutcomeState == HypotheticalOutcomeState.TargetHit.ToString())
            {
                return outcome.RewardRisk > 0 ? outcome.RewardRisk : DefaultTargetRewardR;
            }

            if (outcome.OutcomeState == HypotheticalOutcomeState.StopHit.ToString())
            {
                return -1.0;
            }

            if (outcome.OutcomeState == HypotheticalOutcomeState.Timeout.ToString())
            {
                return TimeoutResultR;
            }

            if (outcome.OutcomeState == HypotheticalOutcomeState.Invalidated.ToString())
            {
                return InvalidatedResultR;
            }

            return 0.0;
        }

        private void UpdateSetupStats(HypotheticalSignalOutcome outcome, double resultR)
        {
            string setupType = string.IsNullOrEmpty(outcome.CandidateSetupType)
                ? "UNKNOWN"
                : outcome.CandidateSetupType;

            if (!bySetup.ContainsKey(setupType))
            {
                bySetup[setupType] = new SetupPerformanceStats
                {
                    SetupType = setupType
                };
            }

            SetupPerformanceStats stats = bySetup[setupType];
            stats.Total++;
            stats.TotalR += resultR;
            stats.AverageMfeR = RunningAverage(stats.AverageMfeR, outcome.MaxFavorableR, stats.Total);
            stats.AverageMaeR = RunningAverage(stats.AverageMaeR, outcome.MaxAdverseR, stats.Total);
            stats.BestR = Math.Max(stats.BestR, resultR);
            stats.WorstR = Math.Min(stats.WorstR, resultR);

            if (outcome.OutcomeState == HypotheticalOutcomeState.TargetHit.ToString())
            {
                stats.TargetHits++;
            }
            else if (outcome.OutcomeState == HypotheticalOutcomeState.StopHit.ToString())
            {
                stats.StopHits++;
            }
            else if (outcome.OutcomeState == HypotheticalOutcomeState.Timeout.ToString())
            {
                stats.Timeouts++;
            }

            stats.AverageR = stats.Total == 0 ? 0 : stats.TotalR / stats.Total;
            stats.WinRate = Rate(stats.TargetHits, stats.Total);
        }

        private void UpdateBestAndWorstSetupTypes()
        {
            string best = string.Empty;
            string worst = string.Empty;
            double bestAverage = double.MinValue;
            double worstAverage = double.MaxValue;

            foreach (KeyValuePair<string, SetupPerformanceStats> item in bySetup)
            {
                if (item.Value.AverageR > bestAverage)
                {
                    bestAverage = item.Value.AverageR;
                    best = item.Value.SetupType;
                }

                if (item.Value.AverageR < worstAverage)
                {
                    worstAverage = item.Value.AverageR;
                    worst = item.Value.SetupType;
                }
            }

            Summary.BestSetupType = best;
            Summary.WorstSetupType = worst;
        }

        private static double RunningAverage(double previousAverage, double newValue, int count)
        {
            if (count <= 1)
            {
                return newValue;
            }

            return previousAverage + ((newValue - previousAverage) / count);
        }

        private static double Rate(int count, int total)
        {
            return total <= 0 ? 0 : (100.0 * count) / total;
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
