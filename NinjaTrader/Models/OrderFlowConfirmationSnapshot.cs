namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OrderFlowConfirmationSnapshot
    {
        public OrderFlowConfirmationSnapshot()
        {
            ConfirmationState = OrderFlowConfirmationState.None;
            ConfirmationType = OrderFlowConfirmationType.None;
            RequiresExecutionDisabled = true;
            UsesApproximateOrderFlow = true;
            CandidateSetupType = string.Empty;
            Reason = string.Empty;
            OrderFlowBias = string.Empty;
        }

        public OrderFlowConfirmationState ConfirmationState { get; set; }
        public OrderFlowConfirmationType ConfirmationType { get; set; }
        public bool HasConfirmation { get; set; }
        public bool RequiresExecutionDisabled { get; set; }
        public bool UsesApproximateOrderFlow { get; set; }
        public string CandidateSetupType { get; set; }
        public string Reason { get; set; }
        public double ConfirmationScore { get; set; }
        public double BarDelta { get; set; }
        public double CumulativeDelta { get; set; }
        public string OrderFlowBias { get; set; }
        public bool IsHighVolumeBar { get; set; }
    }
}
