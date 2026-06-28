using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ReplayValidationSession
    {
        public ReplayValidationSession()
        {
            SessionId = string.Empty;
            Instrument = string.Empty;
            StrategyName = string.Empty;
            DataMode = string.Empty;
            BarType = string.Empty;
            Timeframe = string.Empty;
            TradingHoursTemplate = string.Empty;
            Notes = "NT-4D replay validation session metadata; observation-only and non-executable.";
        }

        public string SessionId { get; set; }
        public string Instrument { get; set; }
        public string StrategyName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string DataMode { get; set; }
        public string BarType { get; set; }
        public string Timeframe { get; set; }
        public string TradingHoursTemplate { get; set; }
        public bool EvaluationOnlyMode { get; set; }
        public bool ExecutionDisabled { get; set; }
        public bool UsesApproximateOrderFlow { get; set; }
        public bool UsesApproximateVolumeProfile { get; set; }
        public int StartingBar { get; set; }
        public int EndingBar { get; set; }
        public int TotalBarsProcessed { get; set; }
        public string Notes { get; set; }
    }
}
