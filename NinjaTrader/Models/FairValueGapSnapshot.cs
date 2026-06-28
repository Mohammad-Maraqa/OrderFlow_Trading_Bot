namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class FairValueGapSnapshot
    {
        public FairValueGapSnapshot()
        {
            FvgState = FairValueGapState.None;
            Reason = string.Empty;
        }

        public FairValueGapState FvgState { get; set; }
        public double FvgHigh { get; set; }
        public double FvgLow { get; set; }
        public double FvgMidpoint { get; set; }
        public bool HasBullishFvg { get; set; }
        public bool HasRetest { get; set; }
        public string Reason { get; set; }
    }
}
