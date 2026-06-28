using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HypotheticalPerformanceSummary
    {
        public HypotheticalPerformanceSummary()
        {
            BestSetupType = string.Empty;
            WorstSetupType = string.Empty;
            SummaryReason = "NT-4C hypothetical performance summary; observation-only and non-executable.";
        }

        public int TotalClosedOutcomes { get; set; }
        public int TargetHits { get; set; }
        public int StopHits { get; set; }
        public int Timeouts { get; set; }
        public int Invalidated { get; set; }
        public int OpenOutcomes { get; set; }
        public double WinRate { get; set; }
        public double StopRate { get; set; }
        public double TimeoutRate { get; set; }
        public double AverageR { get; set; }
        public double TotalR { get; set; }
        public double BestR { get; set; }
        public double WorstR { get; set; }
        public double AverageMaxFavorableR { get; set; }
        public double AverageMaxAdverseR { get; set; }
        public string BestSetupType { get; set; }
        public string WorstSetupType { get; set; }
        public DateTime LastUpdated { get; set; }
        public string SummaryReason { get; set; }
    }
}
