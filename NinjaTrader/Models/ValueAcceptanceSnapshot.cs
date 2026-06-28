namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ValueAcceptanceSnapshot
    {
        public ValueAcceptanceSnapshot()
        {
            AcceptanceState = ValueAcceptanceState.Unknown;
            Reason = string.Empty;
        }

        public ValueAcceptanceState AcceptanceState { get; set; }
        public bool HasAcceptance { get; set; }
        public bool HasRejection { get; set; }
        public bool IsLongSupportive { get; set; }
        public bool IsShortSupportive { get; set; }
        public int BarsAccepted { get; set; }
        public double AcceptedLevel { get; set; }
        public string Reason { get; set; }
    }
}
