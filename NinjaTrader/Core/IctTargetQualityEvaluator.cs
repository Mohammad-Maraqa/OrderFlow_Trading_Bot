using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class IctTargetQualityEvaluator
    {
        public IctTargetQualitySnapshot Evaluate(
            double entryPrice,
            double stopPrice,
            double candidateTarget,
            double swingHigh,
            double sessionHigh,
            double vah,
            double upperVwapBand,
            double tickSize,
            double minimumTargetRewardRisk,
            double preferredTargetRewardRisk,
            int minimumTargetRoomTicks,
            bool preferExternalLiquidityTargets,
            bool useSwingHigh,
            bool useSessionHigh,
            bool useVah,
            bool useUpperVwapBand)
        {
            double target = candidateTarget;
            string source = "CandidateTarget";
            target = ChooseTarget(target, source, swingHigh, "SwingHigh", entryPrice, useSwingHigh, out source);
            target = ChooseTarget(target, source, sessionHigh, "SessionHigh", entryPrice, useSessionHigh, out source);
            target = ChooseTarget(target, source, vah, "VAH", entryPrice, useVah, out source);
            target = ChooseTarget(target, source, upperVwapBand, "UpperVwapBand", entryPrice, useUpperVwapBand, out source);

            double risk = Math.Max(tickSize, entryPrice - stopPrice);
            double roomTicks = tickSize <= 0 ? 0 : (target - entryPrice) / tickSize;
            double rewardRisk = risk <= 0 ? 0 : (target - entryPrice) / risk;
            bool external = source != "CandidateTarget";
            IctTargetQualityState state = IctTargetQualityState.Poor;
            if (roomTicks >= minimumTargetRoomTicks && rewardRisk >= preferredTargetRewardRisk && external)
            {
                state = IctTargetQualityState.Good;
            }
            else if (roomTicks >= minimumTargetRoomTicks && rewardRisk >= minimumTargetRewardRisk)
            {
                state = preferExternalLiquidityTargets && !external
                    ? IctTargetQualityState.Acceptable
                    : IctTargetQualityState.Good;
            }

            return new IctTargetQualitySnapshot
            {
                TargetQualityState = state,
                TargetPrice = target,
                TargetRoomTicks = roomTicks,
                RewardRisk = rewardRisk,
                HasExternalLiquidityTarget = external,
                TargetSource = source,
                Reason = "Target quality proxy seeks buy-side liquidity and sufficient reward/risk; observation-only."
            };
        }

        private static double ChooseTarget(
            double currentTarget,
            string currentSource,
            double candidate,
            string candidateSource,
            double entryPrice,
            bool enabled,
            out string source)
        {
            source = currentSource;
            if (!enabled || candidate <= entryPrice)
            {
                return currentTarget;
            }

            if (currentTarget <= entryPrice || candidate < currentTarget)
            {
                source = candidateSource;
                return candidate;
            }

            return currentTarget;
        }
    }
}
