namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public enum LiquiditySweepState
    {
        None,
        SellSideSweep,
        BuySideSweep,
        SellSideSweepAndReclaim,
        BuySideSweepAndReject,
        Unknown
    }
}
