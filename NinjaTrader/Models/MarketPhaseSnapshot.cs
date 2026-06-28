namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class MarketPhaseSnapshot
    {
        public MarketPhaseSnapshot()
        {
            PhaseState = MarketPhaseState.Unknown;
            Reason = string.Empty;
        }

        public MarketPhaseState PhaseState { get; set; }
        public double RangeHigh { get; set; }
        public double RangeLow { get; set; }
        public bool HasAccumulation { get; set; }
        public bool HasManipulation { get; set; }
        public bool HasDistribution { get; set; }
        public int BarsSinceManipulation { get; set; }
        public string Reason { get; set; }
    }
}
