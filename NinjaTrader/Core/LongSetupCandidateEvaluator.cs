using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class LongSetupCandidateEvaluator
    {
        // Candidate priority:
        // 1. FAILED_BREAKDOWN_LONG
        // 2. VALUE_RECLAIM_LONG
        // 3. DEVIATION_REJECTION_LONG
        // 4. BREAKOUT_PULLBACK_LONG
        // 5. PULLBACK_CONTINUATION_LONG
        public LongSetupCandidateSnapshot Evaluate(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            ValueAreaState previousValueState,
            MarketContextState previousContextState,
            PriceLocationState previousLocationState,
            double previousClosePrice,
            int barsSinceBelowValue,
            int barsSinceAboveValue,
            double minimumRewardRisk,
            double tickSize)
        {
            if (context == null || session == null)
            {
                return None("Context or value structure snapshot is missing.");
            }

            if (!context.HasValidContext)
            {
                return Blocked(LongSetupCandidateState.InvalidContext,
                    "Context is not valid for candidate detection.", context, session);
            }

            if (!session.HasValidValueStructure)
            {
                return Blocked(LongSetupCandidateState.InvalidLocation,
                    "Value structure is not valid for candidate detection.", context, session);
            }

            double safeTickSize = tickSize > 0 ? tickSize : 1.0;
            double safeMinimumRewardRisk = minimumRewardRisk > 0 ? minimumRewardRisk : 2.0;

            LongSetupCandidateSnapshot candidate;

            candidate = EvaluateFailedBreakdownLong(context, session, safeTickSize, safeMinimumRewardRisk);
            if (candidate.HasCandidate)
            {
                return candidate;
            }

            candidate = EvaluateValueReclaimLong(
                context,
                session,
                previousValueState,
                barsSinceBelowValue,
                safeTickSize,
                safeMinimumRewardRisk);
            if (candidate.HasCandidate)
            {
                return candidate;
            }

            candidate = EvaluateDeviationRejectionLong(context, session, safeTickSize, safeMinimumRewardRisk);
            if (candidate.HasCandidate)
            {
                return candidate;
            }

            candidate = EvaluateBreakoutPullbackLong(context, session, safeTickSize, safeMinimumRewardRisk);
            if (candidate.HasCandidate)
            {
                return candidate;
            }

            candidate = EvaluatePullbackContinuationLong(context, session, safeTickSize, safeMinimumRewardRisk);
            if (candidate.HasCandidate)
            {
                return candidate;
            }

            return None("No long setup candidate detected.");
        }

        private static LongSetupCandidateSnapshot EvaluateFailedBreakdownLong(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            double tickSize,
            double minimumRewardRisk)
        {
            bool valueLocation = session.ValueState == ValueAreaState.BelowValue
                || session.ValueState == ValueAreaState.NearVAL;
            bool contextNotChase = context.ContextState != MarketContextState.ExtendedBearish
                || context.LocationState == PriceLocationState.BelowLowerDeviation;

            if (!valueLocation || !contextNotChase)
            {
                return None("FAILED_BREAKDOWN_LONG conditions not present.");
            }

            double stop = LowerOfPositive(session.SessionLow, session.ApproxVal) - 2 * tickSize;
            return Candidate(
                LongSetupType.FailedBreakdownLong,
                "FAILED_BREAKDOWN_LONG",
                context,
                session,
                stop,
                minimumRewardRisk,
                "Possible failed breakdown / value reclaim area; order-flow confirmation required.");
        }

        private static LongSetupCandidateSnapshot EvaluateValueReclaimLong(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            ValueAreaState previousValueState,
            int barsSinceBelowValue,
            double tickSize,
            double minimumRewardRisk)
        {
            bool recentlyBelowValue = previousValueState == ValueAreaState.BelowValue
                || barsSinceBelowValue >= 0;
            bool reclaimedValue = session.ValueState == ValueAreaState.InsideValue
                || session.ValueState == ValueAreaState.NearVAL;

            if (!recentlyBelowValue || !reclaimedValue)
            {
                return None("VALUE_RECLAIM_LONG conditions not present.");
            }

            double stop = session.ApproxVal > 0
                ? session.ApproxVal - 2 * tickSize
                : session.SessionLow - 2 * tickSize;
            return Candidate(
                LongSetupType.ValueReclaimLong,
                "VALUE_RECLAIM_LONG",
                context,
                session,
                stop,
                minimumRewardRisk,
                "Possible value reclaim; order-flow confirmation required.");
        }

        private static LongSetupCandidateSnapshot EvaluateDeviationRejectionLong(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            double tickSize,
            double minimumRewardRisk)
        {
            bool nearLowerDeviation = context.LocationState == PriceLocationState.BelowLowerDeviation
                || (context.LowerVwapBand > 0
                    && Math.Abs(context.CurrentPrice - context.LowerVwapBand) <= 8 * tickSize);
            bool valueLocation = session.ValueState == ValueAreaState.BelowValue
                || session.ValueState == ValueAreaState.NearVAL
                || session.ValueState == ValueAreaState.InsideValue;

            if (!nearLowerDeviation || !valueLocation)
            {
                return None("DEVIATION_REJECTION_LONG conditions not present.");
            }

            double stop = context.LowerVwapBand > 0
                ? context.LowerVwapBand - 2 * tickSize
                : context.CurrentPrice - 8 * tickSize;
            return Candidate(
                LongSetupType.DeviationRejectionLong,
                "DEVIATION_REJECTION_LONG",
                context,
                session,
                stop,
                minimumRewardRisk,
                "Possible lower deviation rejection; order-flow confirmation required.");
        }

        private static LongSetupCandidateSnapshot EvaluateBreakoutPullbackLong(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            double tickSize,
            double minimumRewardRisk)
        {
            bool bullishContext = context.ContextState == MarketContextState.Bullish
                || context.ContextState == MarketContextState.ExtendedBullish;
            bool aboveValue = session.ValueState == ValueAreaState.AboveValue
                || session.ValueState == ValueAreaState.NearVAH;

            if (!bullishContext || !aboveValue)
            {
                return None("BREAKOUT_PULLBACK_LONG conditions not present.");
            }

            double stop = session.ApproxVah > 0
                ? session.ApproxVah - 2 * tickSize
                : session.ApproxPoc - 2 * tickSize;
            return Candidate(
                LongSetupType.BreakoutPullbackLong,
                "BREAKOUT_PULLBACK_LONG",
                context,
                session,
                stop,
                minimumRewardRisk,
                "Possible breakout pullback / acceptance above value; order-flow confirmation required.");
        }

        private static LongSetupCandidateSnapshot EvaluatePullbackContinuationLong(
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            double tickSize,
            double minimumRewardRisk)
        {
            bool bullishContext = context.ContextState == MarketContextState.Bullish
                || context.ContextState == MarketContextState.ExtendedBullish;
            bool vwapLocation = context.LocationState == PriceLocationState.AboveVwap
                || context.LocationState == PriceLocationState.NearVwap;
            bool valueLocation = session.ValueState == ValueAreaState.InsideValue
                || session.ValueState == ValueAreaState.NearPOC
                || session.ValueState == ValueAreaState.NearVAH
                || session.ValueState == ValueAreaState.AboveValue;

            if (!bullishContext || !vwapLocation || !valueLocation)
            {
                return None("PULLBACK_CONTINUATION_LONG conditions not present.");
            }

            double stop = context.Vwap > 0
                ? context.Vwap - 2 * tickSize
                : session.ApproxPoc - 2 * tickSize;
            return Candidate(
                LongSetupType.PullbackContinuationLong,
                "PULLBACK_CONTINUATION_LONG",
                context,
                session,
                stop,
                minimumRewardRisk,
                "Possible bullish pullback continuation; order-flow confirmation required.");
        }

        private static LongSetupCandidateSnapshot Candidate(
            LongSetupType setupType,
            string setupName,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            double stop,
            double minimumRewardRisk,
            string reason)
        {
            double entry = context.CurrentPrice;
            double safeStop = stop > 0 && stop < entry
                ? stop
                : entry - Math.Max(1.0, Math.Abs(entry) * 0.001);
            double risk = entry - safeStop;
            double target = entry + risk * minimumRewardRisk;
            double rewardRisk = risk > 0 ? (target - entry) / risk : 0;
            LongSetupCandidateState state = rewardRisk >= minimumRewardRisk
                ? LongSetupCandidateState.WaitingForConfirmation
                : LongSetupCandidateState.InvalidLocation;

            string finalReason = rewardRisk >= minimumRewardRisk
                ? reason
                : reason + " Risk/reward insufficient.";

            return new LongSetupCandidateSnapshot
            {
                CandidateSetupType = setupType,
                CandidateState = state,
                HasCandidate = true,
                RequiresOrderFlowConfirmation = true,
                Reason = setupName + ": " + finalReason,
                CandidateEntryPrice = entry,
                CandidateStopPrice = safeStop,
                CandidateTargetPrice = target,
                CandidateRewardRisk = rewardRisk,
                ContextState = context.ContextState.ToString(),
                LocationState = context.LocationState.ToString(),
                ValueState = session.ValueState.ToString()
            };
        }

        private static LongSetupCandidateSnapshot Blocked(
            LongSetupCandidateState state,
            string reason,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session)
        {
            return new LongSetupCandidateSnapshot
            {
                CandidateState = state,
                HasCandidate = false,
                RequiresOrderFlowConfirmation = true,
                Reason = reason,
                CandidateEntryPrice = context == null ? 0 : context.CurrentPrice,
                ContextState = context == null ? string.Empty : context.ContextState.ToString(),
                LocationState = context == null ? string.Empty : context.LocationState.ToString(),
                ValueState = session == null ? string.Empty : session.ValueState.ToString()
            };
        }

        private static LongSetupCandidateSnapshot None(string reason)
        {
            return new LongSetupCandidateSnapshot
            {
                CandidateState = LongSetupCandidateState.None,
                HasCandidate = false,
                RequiresOrderFlowConfirmation = true,
                Reason = reason
            };
        }

        private static double LowerOfPositive(double left, double right)
        {
            if (left > 0 && right > 0)
            {
                return Math.Min(left, right);
            }

            if (left > 0)
            {
                return left;
            }

            return right;
        }
    }
}
