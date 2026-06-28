namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class FairValueGapEvaluator
    {
        public FairValueGapSnapshot Evaluate(
            double candleOneHigh,
            double candleThreeLow,
            double currentLow,
            double tickSize,
            int minFvgSizeTicks,
            DisplacementMomentumSnapshot displacementSnapshot,
            LiquiditySweepSnapshot sweepSnapshot,
            bool requireFvgAfterSweep,
            bool trackRetest)
        {
            double sizeTicks = tickSize <= 0 ? 0 : (candleThreeLow - candleOneHigh) / tickSize;
            bool hasFvg = candleOneHigh > 0
                && candleThreeLow > candleOneHigh
                && sizeTicks >= minFvgSizeTicks
                && displacementSnapshot != null
                && displacementSnapshot.HasBullishDisplacement
                && (!requireFvgAfterSweep || (sweepSnapshot != null && sweepSnapshot.HasReclaim));
            bool retest = hasFvg && trackRetest && currentLow <= candleThreeLow && currentLow >= candleOneHigh;

            return new FairValueGapSnapshot
            {
                FvgState = retest ? FairValueGapState.BullishFvgRetest : (hasFvg ? FairValueGapState.BullishFvg : FairValueGapState.None),
                FvgHigh = hasFvg ? candleThreeLow : 0,
                FvgLow = hasFvg ? candleOneHigh : 0,
                FvgMidpoint = hasFvg ? (candleThreeLow + candleOneHigh) / 2.0 : 0,
                HasBullishFvg = hasFvg,
                HasRetest = retest,
                Reason = "Bullish FVG proxy uses three-candle gap after sweep/displacement; observation-only."
            };
        }
    }
}
