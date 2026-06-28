using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class SessionStructureEvaluator
    {
        public SessionStructureSnapshot Evaluate(
            SessionStructureSnapshot snapshot,
            double tickSize,
            int nearValueTicks)
        {
            if (snapshot == null)
            {
                return Invalid("Session structure snapshot is missing.");
            }

            if (tickSize <= 0)
            {
                return InvalidFrom(snapshot, "Tick size is missing or invalid.");
            }

            if (snapshot.ApproxPoc <= 0 || snapshot.ApproxVah <= 0 || snapshot.ApproxVal <= 0)
            {
                return InvalidFrom(snapshot, "Approximate POC/VAH/VAL are not available yet.");
            }

            double nearThresholdPoints = Math.Max(1, nearValueTicks) * tickSize;
            snapshot.DistanceFromPocTicks = (snapshot.CurrentPrice - snapshot.ApproxPoc) / tickSize;
            snapshot.DistanceFromVahTicks = (snapshot.CurrentPrice - snapshot.ApproxVah) / tickSize;
            snapshot.DistanceFromValTicks = (snapshot.CurrentPrice - snapshot.ApproxVal) / tickSize;
            snapshot.HasValidValueStructure = true;

            if (Math.Abs(snapshot.CurrentPrice - snapshot.ApproxPoc) <= nearThresholdPoints)
            {
                snapshot.ValueState = ValueAreaState.NearPOC;
            }
            else if (Math.Abs(snapshot.CurrentPrice - snapshot.ApproxVah) <= nearThresholdPoints)
            {
                snapshot.ValueState = ValueAreaState.NearVAH;
            }
            else if (Math.Abs(snapshot.CurrentPrice - snapshot.ApproxVal) <= nearThresholdPoints)
            {
                snapshot.ValueState = ValueAreaState.NearVAL;
            }
            else if (snapshot.CurrentPrice > snapshot.ApproxVah)
            {
                snapshot.ValueState = ValueAreaState.AboveValue;
            }
            else if (snapshot.CurrentPrice < snapshot.ApproxVal)
            {
                snapshot.ValueState = ValueAreaState.BelowValue;
            }
            else
            {
                snapshot.ValueState = ValueAreaState.InsideValue;
            }

            snapshot.Reason = "Approximate value structure evaluated."
                + PriorSessionReason(snapshot)
                + SessionExtremeReason(snapshot, nearThresholdPoints);
            return snapshot;
        }

        private static SessionStructureSnapshot Invalid(string reason)
        {
            return new SessionStructureSnapshot
            {
                HasValidValueStructure = false,
                ValueState = ValueAreaState.Unknown,
                Reason = reason
            };
        }

        private static SessionStructureSnapshot InvalidFrom(
            SessionStructureSnapshot snapshot,
            string reason)
        {
            snapshot.HasValidValueStructure = false;
            snapshot.ValueState = ValueAreaState.Unknown;
            snapshot.Reason = reason;
            return snapshot;
        }

        private static string PriorSessionReason(SessionStructureSnapshot snapshot)
        {
            if (snapshot.PriorSessionHigh > 0 && snapshot.CurrentPrice > snapshot.PriorSessionHigh)
            {
                return " Price is above prior session high.";
            }

            if (snapshot.PriorSessionLow > 0 && snapshot.CurrentPrice < snapshot.PriorSessionLow)
            {
                return " Price is below prior session low.";
            }

            if (snapshot.PriorSessionHigh > snapshot.PriorSessionLow
                && snapshot.CurrentPrice <= snapshot.PriorSessionHigh
                && snapshot.CurrentPrice >= snapshot.PriorSessionLow)
            {
                return " Price is inside prior session range.";
            }

            return string.Empty;
        }

        private static string SessionExtremeReason(
            SessionStructureSnapshot snapshot,
            double nearThresholdPoints)
        {
            if (snapshot.SessionHigh > 0
                && Math.Abs(snapshot.SessionHigh - snapshot.CurrentPrice) <= nearThresholdPoints)
            {
                return " Price is at session high.";
            }

            if (snapshot.SessionLow > 0
                && Math.Abs(snapshot.CurrentPrice - snapshot.SessionLow) <= nearThresholdPoints)
            {
                return " Price is at session low.";
            }

            return string.Empty;
        }
    }
}
