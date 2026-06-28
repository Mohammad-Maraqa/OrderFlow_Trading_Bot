namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OrderFlowConfirmationEvaluator
    {
        // NT-3B confirmation is approximate and non-executable until true volumetric data is wired.
        public OrderFlowConfirmationSnapshot Evaluate(
            LongSetupCandidateSnapshot candidate,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            OrderFlowFeatureSnapshot orderFlow,
            double minimumConfirmationScore,
            double weakConfirmationScore)
        {
            if (candidate == null || !candidate.HasCandidate)
            {
                return None("No long setup candidate is available for confirmation.");
            }

            if (orderFlow == null || !orderFlow.HasApproxOrderFlow)
            {
                return Waiting(candidate, "Order-flow features are missing; waiting for confirmation.");
            }

            double score = Score(candidate, context, session, orderFlow);
            OrderFlowConfirmationType confirmationType = ConfirmationTypeFor(
                candidate,
                context,
                session,
                orderFlow);
            string reason = ReasonFor(candidate, confirmationType, orderFlow)
                + " Approximate confirmation only; true volumetric confirmation not wired yet.";

            OrderFlowConfirmationState state;
            bool hasConfirmation;
            if (score >= minimumConfirmationScore)
            {
                state = OrderFlowConfirmationState.ConfirmationObserved;
                hasConfirmation = true;
            }
            else if (score >= weakConfirmationScore)
            {
                state = OrderFlowConfirmationState.WeakConfirmation;
                hasConfirmation = false;
            }
            else
            {
                state = OrderFlowConfirmationState.NoConfirmation;
                hasConfirmation = false;
            }

            return new OrderFlowConfirmationSnapshot
            {
                ConfirmationState = state,
                ConfirmationType = confirmationType,
                HasConfirmation = hasConfirmation,
                RequiresExecutionDisabled = true,
                UsesApproximateOrderFlow = true,
                CandidateSetupType = candidate.CandidateSetupType.ToString(),
                Reason = reason,
                ConfirmationScore = score,
                BarDelta = orderFlow.BarDelta,
                CumulativeDelta = orderFlow.CumulativeDelta,
                OrderFlowBias = orderFlow.OrderFlowBias.ToString(),
                IsHighVolumeBar = orderFlow.IsHighVolumeBar
            };
        }

        private static double Score(
            LongSetupCandidateSnapshot candidate,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            OrderFlowFeatureSnapshot orderFlow)
        {
            double score = 0;
            if (candidate != null && candidate.HasCandidate)
            {
                score += 30;
            }

            if (IsBuyerPressure(orderFlow))
            {
                score += 25;
            }
            else if (orderFlow != null && orderFlow.OrderFlowBias == OrderFlowBiasState.Balanced)
            {
                score += 10;
            }

            if (orderFlow != null && orderFlow.BarDelta > 0)
            {
                score += 20;
            }

            if (orderFlow != null && orderFlow.IsHighVolumeBar && orderFlow.BarDelta > 0)
            {
                score += 15;
            }

            if (SupportiveContext(candidate, context, session))
            {
                score += 10;
            }

            return score > 100 ? 100 : score;
        }

        private static bool IsBuyerPressure(OrderFlowFeatureSnapshot orderFlow)
        {
            return orderFlow != null
                && (orderFlow.OrderFlowBias == OrderFlowBiasState.BuyerPressure
                    || orderFlow.OrderFlowBias == OrderFlowBiasState.StrongBuyerPressure);
        }

        private static bool IsStrongSellerPressure(OrderFlowFeatureSnapshot orderFlow)
        {
            return orderFlow != null
                && orderFlow.OrderFlowBias == OrderFlowBiasState.StrongSellerPressure;
        }

        private static bool SupportiveContext(
            LongSetupCandidateSnapshot candidate,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session)
        {
            if (candidate == null || context == null || session == null)
            {
                return false;
            }

            switch (candidate.CandidateSetupType)
            {
                case LongSetupType.FailedBreakdownLong:
                    return session.ValueState == ValueAreaState.BelowValue
                        || session.ValueState == ValueAreaState.NearVAL
                        || session.ValueState == ValueAreaState.InsideValue;
                case LongSetupType.ValueReclaimLong:
                    return session.ValueState == ValueAreaState.InsideValue
                        || session.ValueState == ValueAreaState.NearVAL;
                case LongSetupType.PullbackContinuationLong:
                    return context.ContextState == MarketContextState.Bullish
                        || context.ContextState == MarketContextState.ExtendedBullish;
                case LongSetupType.BreakoutPullbackLong:
                    return session.ValueState == ValueAreaState.AboveValue
                        || session.ValueState == ValueAreaState.NearVAH;
                case LongSetupType.DeviationRejectionLong:
                    return context.LocationState == PriceLocationState.BelowLowerDeviation
                        || context.ContextState == MarketContextState.ExtendedBearish;
                default:
                    return false;
            }
        }

        private static OrderFlowConfirmationType ConfirmationTypeFor(
            LongSetupCandidateSnapshot candidate,
            ContextFeatureSnapshot context,
            SessionStructureSnapshot session,
            OrderFlowFeatureSnapshot orderFlow)
        {
            if (candidate == null)
            {
                return OrderFlowConfirmationType.None;
            }

            if (orderFlow != null && orderFlow.IsHighVolumeBar && orderFlow.BarDelta > 0)
            {
                return OrderFlowConfirmationType.HighVolumeReversalCandidate;
            }

            if (orderFlow != null && orderFlow.IsCvdRising && orderFlow.BarDelta > 0)
            {
                return OrderFlowConfirmationType.CvdReclaimCandidate;
            }

            if (orderFlow != null && orderFlow.BarDelta > 0)
            {
                return OrderFlowConfirmationType.DeltaShiftCandidate;
            }

            if (IsBuyerPressure(orderFlow))
            {
                return OrderFlowConfirmationType.BuyerPressureConfirmation;
            }

            if (candidate.CandidateSetupType == LongSetupType.FailedBreakdownLong
                || candidate.CandidateSetupType == LongSetupType.DeviationRejectionLong)
            {
                return OrderFlowConfirmationType.SellerExhaustionCandidate;
            }

            if (IsStrongSellerPressure(orderFlow))
            {
                return OrderFlowConfirmationType.SellerAbsorptionCandidate;
            }

            return OrderFlowConfirmationType.ApproximationOnly;
        }

        private static string ReasonFor(
            LongSetupCandidateSnapshot candidate,
            OrderFlowConfirmationType confirmationType,
            OrderFlowFeatureSnapshot orderFlow)
        {
            if (candidate == null)
            {
                return "No candidate available.";
            }

            if (IsStrongSellerPressure(orderFlow))
            {
                return "Seller pressure remains strong; no executable confirmation.";
            }

            switch (candidate.CandidateSetupType)
            {
                case LongSetupType.FailedBreakdownLong:
                    return "Possible failed selling / buyer response after breakdown.";
                case LongSetupType.ValueReclaimLong:
                    return "Possible value reclaim with buyer response.";
                case LongSetupType.PullbackContinuationLong:
                    return "Possible continuation with buyer pressure.";
                case LongSetupType.BreakoutPullbackLong:
                    return "Possible acceptance above value with buyer pressure.";
                case LongSetupType.DeviationRejectionLong:
                    return "Possible lower deviation rejection with buyer response.";
                default:
                    return "Approximate confirmation classification only.";
            }
        }

        private static OrderFlowConfirmationSnapshot None(string reason)
        {
            return new OrderFlowConfirmationSnapshot
            {
                ConfirmationState = OrderFlowConfirmationState.None,
                ConfirmationType = OrderFlowConfirmationType.None,
                HasConfirmation = false,
                RequiresExecutionDisabled = true,
                UsesApproximateOrderFlow = true,
                Reason = reason
            };
        }

        private static OrderFlowConfirmationSnapshot Waiting(
            LongSetupCandidateSnapshot candidate,
            string reason)
        {
            return new OrderFlowConfirmationSnapshot
            {
                ConfirmationState = OrderFlowConfirmationState.WaitingForConfirmation,
                ConfirmationType = OrderFlowConfirmationType.None,
                HasConfirmation = false,
                RequiresExecutionDisabled = true,
                UsesApproximateOrderFlow = true,
                CandidateSetupType = candidate == null ? string.Empty : candidate.CandidateSetupType.ToString(),
                Reason = reason
            };
        }
    }
}
