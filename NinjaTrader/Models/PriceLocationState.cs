namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum PriceLocationState
    {
        Unknown,
        AboveVwap,
        BelowVwap,
        NearVwap,
        AboveUpperDeviation,
        BelowLowerDeviation,
        NearSessionHigh,
        NearSessionLow,
        InsideSessionRange
    }
}
