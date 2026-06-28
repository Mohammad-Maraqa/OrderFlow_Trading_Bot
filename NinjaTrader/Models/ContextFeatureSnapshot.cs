namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ContextFeatureSnapshot
    {
        public ContextFeatureSnapshot()
        {
            Reason = string.Empty;
            ContextState = MarketContextState.Unknown;
            LocationState = PriceLocationState.Unknown;
        }

        public double CurrentPrice { get; set; }
        public double SessionHigh { get; set; }
        public double SessionLow { get; set; }
        public double Vwap { get; set; }
        public double UpperVwapBand { get; set; }
        public double LowerVwapBand { get; set; }
        public double DistanceFromVwapPoints { get; set; }
        public double DistanceFromVwapTicks { get; set; }
        public double DistanceFromVwapPercent { get; set; }
        public MarketContextState ContextState { get; set; }
        public PriceLocationState LocationState { get; set; }
        public bool HasValidContext { get; set; }
        public string Reason { get; set; }
    }
}
