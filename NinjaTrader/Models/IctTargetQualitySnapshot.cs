namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class IctTargetQualitySnapshot
    {
        public IctTargetQualitySnapshot()
        {
            TargetQualityState = IctTargetQualityState.Unknown;
            TargetSource = string.Empty;
            Reason = string.Empty;
        }

        public IctTargetQualityState TargetQualityState { get; set; }
        public double TargetPrice { get; set; }
        public double TargetRoomTicks { get; set; }
        public double RewardRisk { get; set; }
        public bool HasExternalLiquidityTarget { get; set; }
        public string TargetSource { get; set; }
        public string Reason { get; set; }
    }
}
