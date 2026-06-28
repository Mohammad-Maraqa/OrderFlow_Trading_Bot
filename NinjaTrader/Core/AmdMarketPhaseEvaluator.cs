using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class AmdMarketPhaseEvaluator
    {
        public MarketPhaseSnapshot Evaluate(
            double rangeHigh,
            double rangeLow,
            double currentPrice,
            double vwap,
            double poc,
            double tickSize,
            int maxAccumulationRangeTicks,
            LiquiditySweepSnapshot sweepSnapshot,
            DisplacementMomentumSnapshot displacementSnapshot,
            bool requireAccumulationBeforeManipulation,
            bool requireDistributionAfterManipulation)
        {
            double widthTicks = tickSize <= 0 ? 0 : (rangeHigh - rangeLow) / tickSize;
            bool narrowRange = widthTicks > 0 && widthTicks <= Math.Max(1, maxAccumulationRangeTicks);
            bool nearValue = Math.Abs(currentPrice - vwap) <= (widthTicks * tickSize * 0.35)
                || Math.Abs(currentPrice - poc) <= (widthTicks * tickSize * 0.35);
            bool hasAccumulation = narrowRange && nearValue;
            bool hasManipulation = sweepSnapshot != null && sweepSnapshot.HasSellSideSweep;
            bool hasDistribution = displacementSnapshot != null && displacementSnapshot.HasBullishDisplacement;

            MarketPhaseState state = MarketPhaseState.Chop;
            if (hasManipulation && hasDistribution)
            {
                state = MarketPhaseState.Distribution;
            }
            else if (hasDistribution)
            {
                state = MarketPhaseState.Expansion;
            }
            else if (hasManipulation)
            {
                state = MarketPhaseState.Manipulation;
            }
            else if (hasAccumulation)
            {
                state = MarketPhaseState.Accumulation;
            }

            return new MarketPhaseSnapshot
            {
                PhaseState = state,
                RangeHigh = rangeHigh,
                RangeLow = rangeLow,
                HasAccumulation = !requireAccumulationBeforeManipulation || hasAccumulation,
                HasManipulation = hasManipulation,
                HasDistribution = !requireDistributionAfterManipulation || hasDistribution,
                BarsSinceManipulation = sweepSnapshot == null ? -1 : sweepSnapshot.BarsSinceSweep,
                Reason = "AMD phase proxy uses range compression, sell-side sweep, and displacement; observation-only."
            };
        }
    }
}
