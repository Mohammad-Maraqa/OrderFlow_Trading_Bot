using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class StrategyDiagnosticSummary
    {
        public StrategyDiagnosticSummary()
        {
            SessionId = string.Empty;
            BestSetupType = string.Empty;
            WorstSetupType = string.Empty;
            EligibilityReason = "Diagnostics have not run.";
            OverallGrade = "NotRun";
            PrimaryProblem = string.Empty;
            RecommendedAction = string.Empty;
            Warnings = new string[0];
        }

        public DateTime Timestamp { get; set; }
        public string SessionId { get; set; }
        public int TotalClosedOutcomes { get; set; }
        public double WinRate { get; set; }
        public double AverageR { get; set; }
        public double TotalR { get; set; }
        public string BestSetupType { get; set; }
        public string WorstSetupType { get; set; }
        public bool IsEligibleForSim101 { get; set; }
        public string EligibilityReason { get; set; }
        public string OverallGrade { get; set; }
        public string PrimaryProblem { get; set; }
        public string RecommendedAction { get; set; }
        public string[] Warnings { get; set; }
    }
}
