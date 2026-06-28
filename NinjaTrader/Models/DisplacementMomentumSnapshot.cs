namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class DisplacementMomentumSnapshot
    {
        public DisplacementMomentumSnapshot()
        {
            MomentumState = DisplacementMomentumState.None;
            Reason = string.Empty;
        }

        public DisplacementMomentumState MomentumState { get; set; }
        public double BodyTicks { get; set; }
        public double BodyToRangeRatio { get; set; }
        public bool HasBullishDisplacement { get; set; }
        public bool CloseNearHigh { get; set; }
        public string Reason { get; set; }
    }
}
