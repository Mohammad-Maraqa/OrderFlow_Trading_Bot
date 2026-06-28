using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class MarketContextEvaluator
    {
        public ContextFeatureSnapshot Evaluate(
            ContextFeatureSnapshot snapshot,
            double tickSize,
            int nearVwapTicks)
        {
            if (snapshot == null)
            {
                return Invalid("Context snapshot is missing.");
            }

            if (tickSize <= 0)
            {
                return InvalidFrom(snapshot, "Tick size is missing or invalid.");
            }

            if (snapshot.Vwap <= 0)
            {
                return InvalidFrom(snapshot, "Approximate session VWAP is not available yet.");
            }

            double nearThresholdPoints = Math.Max(1, nearVwapTicks) * tickSize;
            snapshot.DistanceFromVwapPoints = snapshot.CurrentPrice - snapshot.Vwap;
            snapshot.DistanceFromVwapTicks = snapshot.DistanceFromVwapPoints / tickSize;
            snapshot.DistanceFromVwapPercent = snapshot.Vwap == 0
                ? 0
                : snapshot.DistanceFromVwapPoints / snapshot.Vwap;
            snapshot.HasValidContext = true;

            double absoluteDistance = Math.Abs(snapshot.DistanceFromVwapPoints);
            bool nearVwap = absoluteDistance <= nearThresholdPoints;
            bool aboveUpper = snapshot.UpperVwapBand > 0
                && snapshot.CurrentPrice > snapshot.UpperVwapBand;
            bool belowLower = snapshot.LowerVwapBand > 0
                && snapshot.CurrentPrice < snapshot.LowerVwapBand;

            if (nearVwap)
            {
                snapshot.ContextState = MarketContextState.Balanced;
                snapshot.LocationState = PriceLocationState.NearVwap;
            }
            else if (aboveUpper)
            {
                snapshot.ContextState = MarketContextState.ExtendedBullish;
                snapshot.LocationState = PriceLocationState.AboveUpperDeviation;
            }
            else if (belowLower)
            {
                snapshot.ContextState = MarketContextState.ExtendedBearish;
                snapshot.LocationState = PriceLocationState.BelowLowerDeviation;
            }
            else if (snapshot.CurrentPrice > snapshot.Vwap)
            {
                snapshot.ContextState = MarketContextState.Bullish;
                snapshot.LocationState = PriceLocationState.AboveVwap;
            }
            else if (snapshot.CurrentPrice < snapshot.Vwap)
            {
                snapshot.ContextState = MarketContextState.Bearish;
                snapshot.LocationState = PriceLocationState.BelowVwap;
            }
            else
            {
                snapshot.ContextState = MarketContextState.Balanced;
                snapshot.LocationState = PriceLocationState.NearVwap;
            }

            string sessionReason = SessionLocationReason(snapshot, nearThresholdPoints);
            snapshot.Reason = "Approximate session VWAP context evaluated." + sessionReason;
            return snapshot;
        }

        private static ContextFeatureSnapshot Invalid(string reason)
        {
            return new ContextFeatureSnapshot
            {
                HasValidContext = false,
                ContextState = MarketContextState.Unknown,
                LocationState = PriceLocationState.Unknown,
                Reason = reason
            };
        }

        private static ContextFeatureSnapshot InvalidFrom(
            ContextFeatureSnapshot snapshot,
            string reason)
        {
            snapshot.HasValidContext = false;
            snapshot.ContextState = MarketContextState.Unknown;
            snapshot.LocationState = PriceLocationState.Unknown;
            snapshot.Reason = reason;
            return snapshot;
        }

        private static string SessionLocationReason(
            ContextFeatureSnapshot snapshot,
            double nearThresholdPoints)
        {
            if (snapshot.SessionHigh > 0
                && Math.Abs(snapshot.SessionHigh - snapshot.CurrentPrice) <= nearThresholdPoints)
            {
                return " Price is near session high.";
            }

            if (snapshot.SessionLow > 0
                && Math.Abs(snapshot.CurrentPrice - snapshot.SessionLow) <= nearThresholdPoints)
            {
                return " Price is near session low.";
            }

            if (snapshot.SessionHigh > snapshot.SessionLow)
            {
                return " Price is inside session range.";
            }

            return string.Empty;
        }
    }
}
