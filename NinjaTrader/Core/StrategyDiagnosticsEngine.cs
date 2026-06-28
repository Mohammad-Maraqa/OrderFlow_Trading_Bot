using System;
using System.Collections.Generic;
using System.Globalization;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class StrategyDiagnosticsEngine
    {
        public const string StrategyPrefix = "STRATEGY_DIAGNOSTICS=";
        public const string SetupPrefix = "SETUP_DIAGNOSTIC=";

        public StrategyDiagnosticsEngine()
        {
            MinimumClosedOutcomesForDiagnostics = 100;
            MinimumSetupOutcomesForDecision = 20;
            MinimumAverageRForSim101 = 0.05;
            MinimumSetupAverageRToKeep = 0.0;
            LastSummary = new StrategyDiagnosticSummary();
            LastSetupResults = new List<SetupDiagnosticResult>();
        }

        public int MinimumClosedOutcomesForDiagnostics { get; set; }
        public int MinimumSetupOutcomesForDecision { get; set; }
        public double MinimumAverageRForSim101 { get; set; }
        public double MinimumSetupAverageRToKeep { get; set; }
        public StrategyDiagnosticSummary LastSummary { get; private set; }
        public List<SetupDiagnosticResult> LastSetupResults { get; private set; }

        public StrategyDiagnosticSummary Evaluate(
            HypotheticalPerformanceSummary performanceSummary,
            IList<SetupPerformanceStats> setupStats,
            string sessionId,
            int minimumClosedOutcomesForDiagnostics,
            int minimumSetupOutcomesForDecision,
            double minimumAverageRForSim101,
            double minimumSetupAverageRToKeep)
        {
            MinimumClosedOutcomesForDiagnostics = Math.Max(1, minimumClosedOutcomesForDiagnostics);
            MinimumSetupOutcomesForDecision = Math.Max(1, minimumSetupOutcomesForDecision);
            MinimumAverageRForSim101 = minimumAverageRForSim101;
            MinimumSetupAverageRToKeep = minimumSetupAverageRToKeep;

            HypotheticalPerformanceSummary safeSummary =
                performanceSummary ?? new HypotheticalPerformanceSummary();
            List<SetupDiagnosticResult> setupResults = EvaluateSetups(
                setupStats,
                safeSummary.WorstSetupType,
                MinimumSetupOutcomesForDecision,
                MinimumSetupAverageRToKeep);

            StrategyDiagnosticSummary diagnostics = new StrategyDiagnosticSummary
            {
                Timestamp = DateTime.Now,
                SessionId = sessionId,
                TotalClosedOutcomes = safeSummary.TotalClosedOutcomes,
                WinRate = safeSummary.WinRate,
                AverageR = safeSummary.AverageR,
                TotalR = safeSummary.TotalR,
                BestSetupType = safeSummary.BestSetupType,
                WorstSetupType = safeSummary.WorstSetupType,
                IsEligibleForSim101 = false
            };
            diagnostics.EligibilityReason = "NT-4E strategy diagnostics are observation-only and non-executable.";

            List<string> warnings = new List<string>();
            if (safeSummary.TotalClosedOutcomes < MinimumClosedOutcomesForDiagnostics)
            {
                diagnostics.OverallGrade = "InsufficientSample";
                diagnostics.PrimaryProblem = "Not enough closed hypothetical outcomes.";
                diagnostics.RecommendedAction = "Continue replay validation before changing execution readiness.";
                diagnostics.EligibilityReason = "Minimum closed outcomes for diagnostics not met.";
                warnings.Add("Insufficient sample for diagnostics");
            }
            else if (safeSummary.AverageR < 0)
            {
                diagnostics.OverallGrade = "NegativeExpectancy";
                diagnostics.PrimaryProblem = safeSummary.WinRate < 35
                    ? "Poor signal quality / too many weak candidates"
                    : "Negative average R despite enough samples";
                diagnostics.RecommendedAction = "Do not proceed to Sim101. Disable or tighten worst setups first.";
                diagnostics.EligibilityReason = "AverageR is negative.";
                warnings.Add("Negative expectancy blocks Sim101");
            }
            else
            {
                diagnostics.OverallGrade = "NeedsReview";
                diagnostics.PrimaryProblem = "Replay evidence is not strong enough for execution.";
                diagnostics.RecommendedAction = "Keep replay validation active and review setup-level consistency.";
                diagnostics.EligibilityReason = "Sim101 criteria not fully met.";
            }

            if (MeetsSim101Criteria(safeSummary, setupResults))
            {
                diagnostics.IsEligibleForSim101 = true;
                diagnostics.OverallGrade = "Sim101Candidate";
                diagnostics.PrimaryProblem = "No blocking replay diagnostic found.";
                diagnostics.RecommendedAction = "Review manually before any separate NT-5A Sim101 approval.";
                diagnostics.EligibilityReason = "Replay diagnostics meet minimum Sim101 consideration thresholds.";
            }
            else
            {
                diagnostics.IsEligibleForSim101 = false;
            }

            diagnostics.Warnings = warnings.ToArray();
            LastSummary = diagnostics;
            LastSetupResults = setupResults;
            return diagnostics;
        }

        public List<SetupDiagnosticResult> EvaluateSetups(
            IList<SetupPerformanceStats> setupStats,
            string worstSetupType,
            int minimumSetupOutcomesForDecision,
            double minimumSetupAverageRToKeep)
        {
            List<SetupDiagnosticResult> results = new List<SetupDiagnosticResult>();
            if (setupStats == null)
            {
                return results;
            }

            int minimumOutcomes = Math.Max(1, minimumSetupOutcomesForDecision);
            for (int index = 0; index < setupStats.Count; index++)
            {
                SetupPerformanceStats stats = setupStats[index];
                if (stats == null)
                {
                    continue;
                }

                SetupDiagnosticResult result = new SetupDiagnosticResult
                {
                    SetupType = stats.SetupType,
                    Total = stats.Total,
                    WinRate = stats.WinRate,
                    AverageR = stats.AverageR,
                    TotalR = stats.TotalR
                };

                if (stats.Total < minimumOutcomes)
                {
                    result.ShouldKeepTesting = true;
                    result.RecommendedAction = "KeepTesting";
                    result.DiagnosticReason = "Insufficient sample; keep testing before deciding.";
                }
                else if (stats.AverageR <= -0.25)
                {
                    result.ShouldDisable = true;
                    result.RecommendedAction = "Disable";
                    result.DiagnosticReason = stats.SetupType == worstSetupType
                        ? "Worst setup and negative expectancy."
                        : "Setup average R is materially negative.";
                }
                else if (stats.AverageR < minimumSetupAverageRToKeep)
                {
                    result.ShouldTighten = true;
                    result.RecommendedAction = "Tighten";
                    result.DiagnosticReason = "Setup average R is below keep threshold.";
                }
                else
                {
                    result.ShouldKeepTesting = true;
                    result.RecommendedAction = "KeepTesting";
                    result.DiagnosticReason = stats.AverageR > 0
                        ? "Positive setup expectancy; keep testing."
                        : "Near breakeven; keep testing with tighter review.";
                }

                results.Add(result);
            }

            return results;
        }

        public void PrintDiagnostics(
            HypotheticalPerformanceSummary performanceSummary,
            IList<SetupPerformanceStats> setupStats,
            string sessionId,
            int minimumClosedOutcomesForDiagnostics,
            int minimumSetupOutcomesForDecision,
            double minimumAverageRForSim101,
            double minimumSetupAverageRToKeep,
            Action<string> print)
        {
            StrategyDiagnosticSummary diagnostics = Evaluate(
                performanceSummary,
                setupStats,
                sessionId,
                minimumClosedOutcomesForDiagnostics,
                minimumSetupOutcomesForDecision,
                minimumAverageRForSim101,
                minimumSetupAverageRToKeep);

            SafePrint(print, StrategyPrefix
                + "Total=" + diagnostics.TotalClosedOutcomes
                + " WinRate=" + Format(diagnostics.WinRate)
                + " AvgR=" + Format(diagnostics.AverageR)
                + " TotalR=" + Format(diagnostics.TotalR)
                + " Grade=" + diagnostics.OverallGrade
                + " EligibleForSim101=" + diagnostics.IsEligibleForSim101
                + " PrimaryProblem=" + diagnostics.PrimaryProblem
                + " RecommendedAction=" + diagnostics.RecommendedAction);

            for (int index = 0; index < LastSetupResults.Count; index++)
            {
                SetupDiagnosticResult setup = LastSetupResults[index];
                SafePrint(print, SetupPrefix
                    + "Setup=" + setup.SetupType
                    + " Total=" + setup.Total
                    + " WinRate=" + Format(setup.WinRate)
                    + " AvgR=" + Format(setup.AverageR)
                    + " TotalR=" + Format(setup.TotalR)
                    + " Action=" + setup.RecommendedAction
                    + " Reason=" + setup.DiagnosticReason);
            }
        }

        private bool MeetsSim101Criteria(
            HypotheticalPerformanceSummary summary,
            IList<SetupDiagnosticResult> setupResults)
        {
            if (summary.TotalClosedOutcomes < MinimumClosedOutcomesForDiagnostics
                || summary.AverageR <= MinimumAverageRForSim101
                || summary.TotalR <= 0)
            {
                return false;
            }

            for (int index = 0; index < setupResults.Count; index++)
            {
                SetupDiagnosticResult setup = setupResults[index];
                if (setup.Total >= 30 && setup.AverageR > 0.10)
                {
                    return true;
                }
            }

            return false;
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
