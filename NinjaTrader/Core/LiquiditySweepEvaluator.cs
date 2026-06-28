namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class LiquiditySweepEvaluator
    {
        public LiquiditySweepSnapshot Evaluate(
            double lowPrice,
            double closePrice,
            double swingLow,
            double rangeLow,
            double val,
            double priorLow,
            double tickSize,
            int sweepBufferTicks,
            bool requireReclaim,
            bool allowValSweep,
            bool allowRangeLowSweep,
            bool allowSwingLowSweep,
            bool allowPriorLowSweep)
        {
            double buffer = tickSize * sweepBufferTicks;
            double level = 0;
            string source = string.Empty;
            if (allowSwingLowSweep && swingLow > 0 && lowPrice < swingLow - buffer)
            {
                level = swingLow;
                source = "SwingLow";
            }
            else if (allowRangeLowSweep && rangeLow > 0 && lowPrice < rangeLow - buffer)
            {
                level = rangeLow;
                source = "RangeLow";
            }
            else if (allowValSweep && val > 0 && lowPrice < val - buffer)
            {
                level = val;
                source = "VAL";
            }
            else if (allowPriorLowSweep && priorLow > 0 && lowPrice < priorLow - buffer)
            {
                level = priorLow;
                source = "PriorLow";
            }

            bool swept = level > 0;
            bool reclaimed = swept && closePrice > level;
            LiquiditySweepState state = LiquiditySweepState.None;
            if (swept && reclaimed)
            {
                state = LiquiditySweepState.SellSideSweepAndReclaim;
            }
            else if (swept)
            {
                state = LiquiditySweepState.SellSideSweep;
            }

            return new LiquiditySweepSnapshot
            {
                SweepState = state,
                SweptLevel = level,
                HasSellSideSweep = swept,
                HasReclaim = !requireReclaim || reclaimed,
                BarsSinceSweep = swept ? 0 : -1,
                SweepSource = source,
                Reason = "Liquidity sweep proxy checks swing/range/VAL/prior low sweep and reclaim; observation-only."
            };
        }
    }
}
