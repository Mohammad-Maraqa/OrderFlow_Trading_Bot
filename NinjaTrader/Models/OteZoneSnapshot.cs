namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OteZoneSnapshot
    {
        public OteZoneSnapshot()
        {
            OteState = OteZoneState.Unknown;
            Reason = string.Empty;
        }

        public OteZoneState OteState { get; set; }
        public double DealingRangeLow { get; set; }
        public double DealingRangeHigh { get; set; }
        public bool IsInOteZone { get; set; }
        public bool IsInDiscount { get; set; }
        public double Retracement { get; set; }
        public string Reason { get; set; }
    }
}
