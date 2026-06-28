using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OrderFlowFeatureSnapshot
    {
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public double CurrentPrice { get; set; }
        public bool IsComplete { get; set; }
        public bool HasCandidateSetup { get; set; }
        public LongSetupType CandidateSetup { get; set; }
        public bool LongContextAllowed { get; set; }
        public bool LongLocationValid { get; set; }
        public bool LongConfirmationPresent { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
        public double TargetPrice { get; set; }
        public string SourceReason { get; set; }
        public double BarVolume { get; set; }
        public double ApproxBuyVolume { get; set; }
        public double ApproxSellVolume { get; set; }
        public double BarDelta { get; set; }
        public double CumulativeDelta { get; set; }
        public double DeltaMovingAverage { get; set; }
        public double VolumeMovingAverage { get; set; }
        public double DeltaStrength { get; set; }
        public double VolumeRatio { get; set; }
        public bool IsHighVolumeBar { get; set; }
        public bool IsPositiveDelta { get; set; }
        public bool IsNegativeDelta { get; set; }
        public bool IsCvdRising { get; set; }
        public bool IsCvdFalling { get; set; }
        public bool HasApproxOrderFlow { get; set; }
        public bool UsesApproximation { get; set; }
        public string Source { get; set; }
        public string Reason { get; set; }
        public OrderFlowBiasState OrderFlowBias { get; set; }
        public OrderFlowPressureState OrderFlowPressure { get; set; }
    }
}
