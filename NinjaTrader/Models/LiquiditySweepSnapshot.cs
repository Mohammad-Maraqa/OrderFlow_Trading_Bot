namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class LiquiditySweepSnapshot
    {
        public LiquiditySweepSnapshot()
        {
            SweepState = LiquiditySweepState.None;
            SweepSource = string.Empty;
            Reason = string.Empty;
        }

        public LiquiditySweepState SweepState { get; set; }
        public double SweptLevel { get; set; }
        public bool HasSellSideSweep { get; set; }
        public bool HasReclaim { get; set; }
        public int BarsSinceSweep { get; set; }
        public string SweepSource { get; set; }
        public string Reason { get; set; }
    }
}
