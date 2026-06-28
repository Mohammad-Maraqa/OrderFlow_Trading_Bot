namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum OrderFlowConfirmationType
    {
        None,
        BuyerPressureConfirmation,
        SellerExhaustionCandidate,
        SellerAbsorptionCandidate,
        CvdReclaimCandidate,
        DeltaShiftCandidate,
        HighVolumeReversalCandidate,
        ApproximationOnly
    }
}
