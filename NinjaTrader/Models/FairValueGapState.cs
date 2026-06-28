namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum FairValueGapState
    {
        None,
        BullishFvg,
        BearishFvg,
        BullishFvgRetest,
        BearishFvgRetest,
        Filled,
        Unknown
    }
}
