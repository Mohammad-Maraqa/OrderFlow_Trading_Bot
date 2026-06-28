using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class ValueAcceptanceEvaluator
    {
        public ValueAcceptanceSnapshot Evaluate(
            double closePrice,
            double highPrice,
            double lowPrice,
            ValueRoadmapSnapshot roadmap,
            int acceptanceBarsRequired,
            int rejectionBarsRequired,
            double tickSize,
            int nearValueEdgeTicks)
        {
            double near = Math.Max(1, nearValueEdgeTicks) * Math.Max(0.00000001, tickSize);
            ValueAcceptanceSnapshot snapshot = new ValueAcceptanceSnapshot
            {
                BarsAccepted = Math.Max(1, acceptanceBarsRequired),
                Reason = "Original value acceptance proxy uses closes around VAH/VAL/VWAP/CVA; observation-only."
            };

            if (roadmap == null || !roadmap.HasClearRoadmap)
            {
                snapshot.AcceptanceState = ValueAcceptanceState.NoAcceptance;
                snapshot.Reason = "No clear value roadmap for acceptance.";
                return snapshot;
            }

            bool acceptedAbove = closePrice >= roadmap.DevelopingValueHigh - near;
            bool acceptedInside = closePrice <= roadmap.DevelopingValueHigh && closePrice >= roadmap.DevelopingValueLow;
            bool rejectedBelow = lowPrice < roadmap.LowerDeviation15 && closePrice > roadmap.DevelopingValueLow - near;
            bool rejectedAbove = highPrice > roadmap.UpperDeviation15 && closePrice < roadmap.DevelopingValueHigh + near;

            if (acceptedAbove)
            {
                snapshot.AcceptanceState = ValueAcceptanceState.AcceptedAboveValue;
                snapshot.HasAcceptance = true;
                snapshot.IsLongSupportive = true;
                snapshot.AcceptedLevel = roadmap.DevelopingValueHigh;
            }
            else if (rejectedBelow)
            {
                snapshot.AcceptanceState = ValueAcceptanceState.RejectedBelowValue;
                snapshot.HasRejection = true;
                snapshot.IsLongSupportive = true;
                snapshot.AcceptedLevel = roadmap.DevelopingValueLow;
            }
            else if (acceptedInside)
            {
                snapshot.AcceptanceState = ValueAcceptanceState.RotationalInsideValue;
                snapshot.HasAcceptance = true;
                snapshot.IsLongSupportive = true;
                snapshot.AcceptedLevel = roadmap.SessionVwap;
            }
            else if (rejectedAbove)
            {
                snapshot.AcceptanceState = ValueAcceptanceState.RejectedAboveValue;
                snapshot.HasRejection = true;
                snapshot.IsShortSupportive = true;
                snapshot.AcceptedLevel = roadmap.DevelopingValueHigh;
            }
            else
            {
                snapshot.AcceptanceState = ValueAcceptanceState.NoAcceptance;
            }

            return snapshot;
        }
    }
}
