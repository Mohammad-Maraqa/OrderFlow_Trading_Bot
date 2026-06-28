using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ValueRoadmapEvaluator
    {
        public ValueRoadmapSnapshot Evaluate(
            double currentPrice,
            double sessionVwap,
            double developingValueHigh,
            double developingValueLow,
            double developingPoc,
            double upperDeviation15,
            double upperDeviation20,
            double lowerDeviation15,
            double lowerDeviation20,
            double nearestCvaHigh,
            double nearestCvaLow,
            double tickSize,
            int nearValueEdgeTicks)
        {
            double near = Math.Max(1, nearValueEdgeTicks) * Math.Max(0.00000001, tickSize);
            bool insideValue = currentPrice <= developingValueHigh && currentPrice >= developingValueLow;
            bool nearVwap = Math.Abs(currentPrice - sessionVwap) <= near;
            bool nearUpperDeviation = Math.Abs(currentPrice - upperDeviation20) <= near || Math.Abs(currentPrice - upperDeviation15) <= near;
            bool nearLowerDeviation = Math.Abs(currentPrice - lowerDeviation20) <= near || Math.Abs(currentPrice - lowerDeviation15) <= near;
            bool aboveValue = currentPrice > developingValueHigh;
            bool belowValue = currentPrice < developingValueLow;

            ValueRoadmapSnapshot snapshot = new ValueRoadmapSnapshot
            {
                CurrentPrice = currentPrice,
                SessionVwap = sessionVwap,
                DevelopingValueHigh = developingValueHigh,
                DevelopingValueLow = developingValueLow,
                DevelopingPoc = developingPoc,
                UpperDeviation15 = upperDeviation15,
                UpperDeviation20 = upperDeviation20,
                LowerDeviation15 = lowerDeviation15,
                LowerDeviation20 = lowerDeviation20,
                NearestCvaHigh = nearestCvaHigh,
                NearestCvaLow = nearestCvaLow,
                IsRotationalDayCandidate = insideValue || nearVwap || nearUpperDeviation || nearLowerDeviation,
                IsTrendDayCandidate = aboveValue || belowValue,
                HasClearRoadmap = developingValueHigh > developingValueLow && sessionVwap > 0,
                Reason = "Original value roadmap uses RTH value, VWAP, deviation bands, and approximate CVA; observation-only."
            };

            if (!snapshot.HasClearRoadmap)
            {
                snapshot.RoadmapState = "NoClearRoadmap";
                snapshot.PrimaryTradeIdea = "None";
                snapshot.TargetReason = "Missing value/VWAP structure.";
                return snapshot;
            }

            if (nearLowerDeviation || belowValue)
            {
                snapshot.RoadmapState = "ReturnToValueFromLowerDeviation";
                snapshot.PrimaryTradeIdea = "ReturnPullbackToValue";
                snapshot.TargetLevel1 = developingValueLow;
                snapshot.TargetLevel2 = sessionVwap;
                snapshot.TargetLevelFinal = developingValueHigh;
                snapshot.TargetReason = "Target back toward VWAP and opposite side of value.";
            }
            else if (aboveValue)
            {
                snapshot.RoadmapState = "BreakoutPullbackToUpperValue";
                snapshot.PrimaryTradeIdea = "BreakoutPullbackFromValue";
                snapshot.TargetLevel1 = Math.Max(developingValueHigh, nearestCvaHigh);
                snapshot.TargetLevel2 = upperDeviation15;
                snapshot.TargetLevelFinal = upperDeviation20;
                snapshot.TargetReason = "Target upper value/CVA and VWAP deviation objectives.";
            }
            else if (insideValue)
            {
                snapshot.RoadmapState = "RotationalInsideValue";
                snapshot.PrimaryTradeIdea = "ValueContinuation";
                snapshot.TargetLevel1 = developingPoc;
                snapshot.TargetLevel2 = sessionVwap;
                snapshot.TargetLevelFinal = developingValueHigh;
                snapshot.TargetReason = "Target POC/VWAP/opposite side of value.";
            }
            else
            {
                snapshot.RoadmapState = "NoClearRoadmap";
                snapshot.PrimaryTradeIdea = "None";
                snapshot.HasClearRoadmap = false;
                snapshot.TargetReason = "No original value roadmap condition matched.";
            }

            return snapshot;
        }
    }
}
