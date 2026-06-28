using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class DisplacementMomentumEvaluator
    {
        public DisplacementMomentumSnapshot Evaluate(
            double openPrice,
            double highPrice,
            double lowPrice,
            double closePrice,
            double volumeRatio,
            double barDelta,
            double tickSize,
            int minBodyTicks,
            double minBodyToRangeRatio,
            bool requireCloseNearHigh,
            double closeNearHighPercent,
            bool requirePositiveDelta)
        {
            double body = closePrice - openPrice;
            double range = Math.Max(tickSize, highPrice - lowPrice);
            double bodyTicks = tickSize <= 0 ? 0 : Math.Abs(body) / tickSize;
            double ratio = Math.Abs(body) / range;
            bool bullish = body > 0;
            bool nearHigh = (highPrice - closePrice) <= range * closeNearHighPercent;
            bool positiveDelta = !requirePositiveDelta || barDelta > 0;
            bool valid = bullish
                && bodyTicks >= minBodyTicks
                && ratio >= minBodyToRangeRatio
                && (!requireCloseNearHigh || nearHigh)
                && positiveDelta;

            DisplacementMomentumState state = DisplacementMomentumState.Weak;
            if (valid && volumeRatio >= 1.5)
            {
                state = DisplacementMomentumState.StrongBullishDisplacement;
            }
            else if (valid)
            {
                state = DisplacementMomentumState.BullishDisplacement;
            }
            else if (!bullish && bodyTicks >= minBodyTicks)
            {
                state = DisplacementMomentumState.BearishDisplacement;
            }

            return new DisplacementMomentumSnapshot
            {
                MomentumState = state,
                BodyTicks = bodyTicks,
                BodyToRangeRatio = ratio,
                HasBullishDisplacement = valid,
                CloseNearHigh = nearHigh,
                Reason = "Displacement proxy checks body size, body/range, close location, volume, and delta; observation-only."
            };
        }
    }
}
