namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum OrderFlowPressureState
    {
        Unknown,
        PositiveDelta,
        NegativeDelta,
        HighVolumePositiveDelta,
        HighVolumeNegativeDelta,
        LowVolume,
        ExhaustionCandidate,
        AbsorptionCandidatePlaceholder
    }
}
