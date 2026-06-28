namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum ValueAreaState
    {
        Unknown,
        InsideValue,
        AboveValue,
        BelowValue,
        NearVAH,
        NearVAL,
        NearPOC,
        AtSessionHigh,
        AtSessionLow,
        AbovePriorHigh,
        BelowPriorLow,
        InsidePriorRange
    }
}
