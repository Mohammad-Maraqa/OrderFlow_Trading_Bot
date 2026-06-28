namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ValueRoadmapSnapshot
    {
        public ValueRoadmapSnapshot()
        {
            RoadmapState = string.Empty;
            PrimaryTradeIdea = string.Empty;
            TargetReason = string.Empty;
            Reason = string.Empty;
        }

        public string RoadmapState { get; set; }
        public string PrimaryTradeIdea { get; set; }
        public bool HasClearRoadmap { get; set; }
        public bool IsRotationalDayCandidate { get; set; }
        public bool IsTrendDayCandidate { get; set; }
        public double CurrentPrice { get; set; }
        public double SessionVwap { get; set; }
        public double DevelopingValueHigh { get; set; }
        public double DevelopingValueLow { get; set; }
        public double DevelopingPoc { get; set; }
        public double UpperDeviation15 { get; set; }
        public double UpperDeviation20 { get; set; }
        public double LowerDeviation15 { get; set; }
        public double LowerDeviation20 { get; set; }
        public double NearestCvaHigh { get; set; }
        public double NearestCvaLow { get; set; }
        public double TargetLevel1 { get; set; }
        public double TargetLevel2 { get; set; }
        public double TargetLevelFinal { get; set; }
        public string TargetReason { get; set; }
        public string Reason { get; set; }
    }
}
