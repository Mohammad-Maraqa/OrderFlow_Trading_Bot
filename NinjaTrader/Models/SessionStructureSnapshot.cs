namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SessionStructureSnapshot
    {
        public SessionStructureSnapshot()
        {
            ValueState = ValueAreaState.Unknown;
            Reason = string.Empty;
        }

        public double CurrentPrice { get; set; }
        public double SessionHigh { get; set; }
        public double SessionLow { get; set; }
        public double PriorSessionHigh { get; set; }
        public double PriorSessionLow { get; set; }
        public double OpeningRangeHigh { get; set; }
        public double OpeningRangeLow { get; set; }
        public double ApproxPoc { get; set; }
        public double ApproxVah { get; set; }
        public double ApproxVal { get; set; }
        public double DistanceFromPocTicks { get; set; }
        public double DistanceFromVahTicks { get; set; }
        public double DistanceFromValTicks { get; set; }
        public ValueAreaState ValueState { get; set; }
        public bool HasValidValueStructure { get; set; }
        public string Reason { get; set; }
    }
}
