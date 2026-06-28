namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class HigherTimeframeBiasEvaluator
    {
        public HigherTimeframeBiasSnapshot Evaluate(
            double price,
            double fastEma,
            double slowEma,
            double previousSlowEma,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot valueSnapshot,
            bool requireBiasForLongs,
            bool allowBalanced,
            bool rejectStrongBearish)
        {
            double slope = slowEma - previousSlowEma;
            HigherTimeframeBiasState state = HigherTimeframeBiasState.Balanced;
            if (fastEma > slowEma && slope > 0)
            {
                state = SupportsUpside(contextSnapshot, valueSnapshot)
                    ? HigherTimeframeBiasState.StrongBullish
                    : HigherTimeframeBiasState.Bullish;
            }
            else if (fastEma < slowEma && slope < 0)
            {
                state = SupportsDownside(contextSnapshot, valueSnapshot)
                    ? HigherTimeframeBiasState.StrongBearish
                    : HigherTimeframeBiasState.Bearish;
            }

            bool allowsLongs = !requireBiasForLongs
                || state == HigherTimeframeBiasState.StrongBullish
                || state == HigherTimeframeBiasState.Bullish
                || (allowBalanced && state == HigherTimeframeBiasState.Balanced);

            if (rejectStrongBearish && state == HigherTimeframeBiasState.StrongBearish)
            {
                allowsLongs = false;
            }

            return new HigherTimeframeBiasSnapshot
            {
                BiasState = state,
                FastEma = fastEma,
                SlowEma = slowEma,
                SlowEmaSlope = slope,
                AllowsLongs = allowsLongs,
                Reason = "HTF bias proxy uses current-chart EMA slope, VWAP, and value context; observation-only."
            };
        }

        private static bool SupportsUpside(ContextFeatureSnapshot contextSnapshot, SessionStructureSnapshot valueSnapshot)
        {
            bool context = contextSnapshot != null
                && (contextSnapshot.ContextState == MarketContextState.Bullish
                    || contextSnapshot.ContextState == MarketContextState.ExtendedBullish
                    || contextSnapshot.LocationState == PriceLocationState.AboveVwap);
            bool value = valueSnapshot != null
                && (valueSnapshot.ValueState == ValueAreaState.AboveValue
                    || valueSnapshot.ValueState == ValueAreaState.NearVAH);
            return context || value;
        }

        private static bool SupportsDownside(ContextFeatureSnapshot contextSnapshot, SessionStructureSnapshot valueSnapshot)
        {
            bool context = contextSnapshot != null
                && (contextSnapshot.ContextState == MarketContextState.Bearish
                    || contextSnapshot.ContextState == MarketContextState.ExtendedBearish
                    || contextSnapshot.LocationState == PriceLocationState.BelowVwap);
            bool value = valueSnapshot != null
                && (valueSnapshot.ValueState == ValueAreaState.BelowValue
                    || valueSnapshot.ValueState == ValueAreaState.NearVAL);
            return context || value;
        }
    }
}
