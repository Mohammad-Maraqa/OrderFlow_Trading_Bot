namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class StrategyFilterResult
    {
        public StrategyFilterResult()
        {
            FilterReason = string.Empty;
            CandidateSetupType = string.Empty;
            FilterProfile = string.Empty;
            ConfirmationState = string.Empty;
            ContextState = string.Empty;
            LocationState = string.Empty;
            ValueState = string.Empty;
            ExecutionDisabled = true;
            EvaluationOnlyMode = true;
        }

        public bool IsAllowed { get; set; }
        public bool IsFiltered { get; set; }
        public string FilterReason { get; set; }
        public string CandidateSetupType { get; set; }
        public string FilterProfile { get; set; }
        public double ConfirmationScore { get; set; }
        public string ConfirmationState { get; set; }
        public string ContextState { get; set; }
        public string LocationState { get; set; }
        public string ValueState { get; set; }
        public double CandidateRewardRisk { get; set; }
        public bool ExecutionDisabled { get; set; }
        public bool EvaluationOnlyMode { get; set; }
    }
}
