namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class AdaptiveTargetPlan
    {
        public AdaptiveTargetPlan()
        {
            Target1Reason = string.Empty;
            Target2Reason = string.Empty;
            FinalTargetReason = string.Empty;
            Reason = string.Empty;
        }

        public bool HasTargetPlan { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
        public double Target1 { get; set; }
        public double Target2 { get; set; }
        public double FinalTarget { get; set; }
        public string Target1Reason { get; set; }
        public string Target2Reason { get; set; }
        public string FinalTargetReason { get; set; }
        public bool MoveStopToBreakevenAfterTarget1 { get; set; }
        public bool IsLogicalTargetPlan { get; set; }
        public double EstimatedRewardRiskToFinal { get; set; }
        public string Reason { get; set; }
    }
}
