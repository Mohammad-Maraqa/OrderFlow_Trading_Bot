namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class LongSetupCandidateSnapshot
    {
        public LongSetupCandidateSnapshot()
        {
            CandidateState = LongSetupCandidateState.None;
            RequiresOrderFlowConfirmation = true;
            Reason = string.Empty;
            ContextState = string.Empty;
            LocationState = string.Empty;
            ValueState = string.Empty;
        }

        public LongSetupType CandidateSetupType { get; set; }
        public LongSetupCandidateState CandidateState { get; set; }
        public bool HasCandidate { get; set; }
        public bool RequiresOrderFlowConfirmation { get; set; }
        public string Reason { get; set; }
        public double CandidateEntryPrice { get; set; }
        public double CandidateStopPrice { get; set; }
        public double CandidateTargetPrice { get; set; }
        public double CandidateRewardRisk { get; set; }
        public string ContextState { get; set; }
        public string LocationState { get; set; }
        public string ValueState { get; set; }
    }
}
