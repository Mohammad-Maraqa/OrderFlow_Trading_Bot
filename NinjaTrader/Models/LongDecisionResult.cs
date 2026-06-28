namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum LongDecisionStatus
    {
        DataMissing,
        WaitingForConfirmation,
        InvalidContext,
        InvalidLocation,
        InvalidRiskReward,
        NoTrade,
        LongValid
    }

    public sealed class LongDecisionResult
    {
        public LongDecisionResult()
        {
            Symbol = string.Empty;
            Reason = string.Empty;
        }

        public string Symbol { get; set; }
        public LongSetupType? SetupType { get; set; }
        public OrderFlowSignalState SignalState { get; set; }
        public LongDecisionStatus DecisionStatus { get; set; }
        public string Reason { get; set; }
        public double? EntryPrice { get; set; }
        public double? StopPrice { get; set; }
        public double? TargetPrice { get; set; }
        public double? RewardRiskRatio { get; set; }
    }
}
