using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ReplayValidationSummary
    {
        public ReplayValidationSummary()
        {
            SessionId = string.Empty;
            Instrument = string.Empty;
            BestSetupType = string.Empty;
            WorstSetupType = string.Empty;
            ReviewWarnings = string.Empty;
            SummaryReason = "NT-4D replay validation summary; observation-only and non-executable.";
        }

        public string SessionId { get; set; }
        public string Instrument { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public int TotalBarsProcessed { get; set; }
        public int JournaledCandidates { get; set; }
        public int ConfirmedCandidates { get; set; }
        public int WeakConfirmations { get; set; }
        public int NoConfirmations { get; set; }
        public int OpenOutcomes { get; set; }
        public int ClosedOutcomes { get; set; }
        public int TargetHits { get; set; }
        public int StopHits { get; set; }
        public int Timeouts { get; set; }
        public double WinRate { get; set; }
        public double AverageR { get; set; }
        public double TotalR { get; set; }
        public string BestSetupType { get; set; }
        public string WorstSetupType { get; set; }
        public bool IsReviewable { get; set; }
        public string ReviewWarnings { get; set; }
        public string SummaryReason { get; set; }
    }
}
