using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class AdaptiveTargetPlanner
    {
        public AdaptiveTargetPlan Build(
            double entryPrice,
            double stopPrice,
            ValueRoadmapSnapshot roadmap,
            int minimumLogicalTargetRoomTicks,
            double tickSize)
        {
            AdaptiveTargetPlan plan = new AdaptiveTargetPlan
            {
                EntryPrice = entryPrice,
                StopPrice = stopPrice,
                MoveStopToBreakevenAfterTarget1 = true,
                Reason = "Adaptive target plan uses VWAP/value/CVA/deviation levels, not fixed RR; observation-only."
            };

            if (roadmap == null || !roadmap.HasClearRoadmap || entryPrice <= 0 || stopPrice <= 0)
            {
                plan.Reason = "No logical value roadmap target plan.";
                return plan;
            }

            plan.Target1 = FirstAbove(entryPrice, roadmap.TargetLevel1, roadmap.SessionVwap, roadmap.DevelopingPoc, roadmap.DevelopingValueHigh);
            plan.Target2 = FirstAbove(plan.Target1, roadmap.TargetLevel2, roadmap.NearestCvaHigh, roadmap.UpperDeviation15, roadmap.DevelopingValueHigh);
            plan.FinalTarget = FirstAbove(plan.Target2, roadmap.TargetLevelFinal, roadmap.UpperDeviation20, roadmap.NearestCvaHigh, roadmap.DevelopingValueHigh);
            plan.Target1Reason = "Nearest logical value/VWAP level.";
            plan.Target2Reason = "Next value/CVA/deviation objective.";
            plan.FinalTargetReason = roadmap.TargetReason;

            double roomTicks = tickSize <= 0 ? 0 : (plan.FinalTarget - entryPrice) / tickSize;
            double risk = Math.Max(0.00000001, entryPrice - stopPrice);
            plan.EstimatedRewardRiskToFinal = (plan.FinalTarget - entryPrice) / risk;
            plan.HasTargetPlan = plan.FinalTarget > entryPrice && roomTicks >= minimumLogicalTargetRoomTicks;
            plan.IsLogicalTargetPlan = plan.HasTargetPlan;
            if (!plan.HasTargetPlan)
            {
                plan.Reason = "Logical target room is too small.";
            }

            return plan;
        }

        private static double FirstAbove(double reference, params double[] levels)
        {
            double best = 0;
            for (int index = 0; index < levels.Length; index++)
            {
                double level = levels[index];
                if (level > reference && (best <= reference || level < best))
                {
                    best = level;
                }
            }

            return best <= reference ? reference : best;
        }
    }
}
