namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OteZoneEvaluator
    {
        public OteZoneSnapshot Evaluate(
            double entryPrice,
            double dealingRangeLow,
            double dealingRangeHigh,
            double oteLowerLevel,
            double oteUpperLevel,
            bool allowDiscountButOutsideOte,
            bool rejectPremiumLongEntries)
        {
            if (dealingRangeHigh <= dealingRangeLow || entryPrice <= 0)
            {
                return new OteZoneSnapshot
                {
                    OteState = OteZoneState.Invalid,
                    DealingRangeLow = dealingRangeLow,
                    DealingRangeHigh = dealingRangeHigh,
                    Reason = "OTE dealing range is invalid; observation-only."
                };
            }

            double range = dealingRangeHigh - dealingRangeLow;
            double retracement = (dealingRangeHigh - entryPrice) / range;
            bool inOte = retracement >= oteLowerLevel && retracement <= oteUpperLevel;
            bool discount = entryPrice <= dealingRangeLow + (range * 0.5);
            OteZoneState state = OteZoneState.NotInOte;
            if (inOte)
            {
                state = OteZoneState.InOteZone;
            }
            else if (discount && allowDiscountButOutsideOte)
            {
                state = OteZoneState.InDiscount;
            }
            else if (rejectPremiumLongEntries && !discount)
            {
                state = OteZoneState.InPremium;
            }

            return new OteZoneSnapshot
            {
                OteState = state,
                DealingRangeLow = dealingRangeLow,
                DealingRangeHigh = dealingRangeHigh,
                IsInOteZone = inOte,
                IsInDiscount = discount,
                Retracement = retracement,
                Reason = "OTE proxy evaluates discount and 0.61-0.79 retracement; observation-only."
            };
        }
    }
}
