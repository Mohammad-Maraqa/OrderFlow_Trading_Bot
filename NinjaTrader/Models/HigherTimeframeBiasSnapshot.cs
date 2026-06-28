namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HigherTimeframeBiasSnapshot
    {
        public HigherTimeframeBiasSnapshot()
        {
            BiasState = HigherTimeframeBiasState.Unknown;
            Reason = string.Empty;
        }

        public HigherTimeframeBiasState BiasState { get; set; }
        public double FastEma { get; set; }
        public double SlowEma { get; set; }
        public double SlowEmaSlope { get; set; }
        public bool AllowsLongs { get; set; }
        public string Reason { get; set; }
    }
}
