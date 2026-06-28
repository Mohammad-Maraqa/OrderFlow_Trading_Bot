using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class LongOnlyOrderFlowEvaluator
    {
        public LongDecisionResult Evaluate(
            OrderFlowFeatureSnapshot snapshot,
            double minimumRewardRisk,
            Func<LongSetupType, bool> isSetupEnabled)
        {
            if (snapshot == null || !snapshot.IsComplete)
            {
                return Blocked(
                    snapshot,
                    OrderFlowSignalState.DataMissing,
                    LongDecisionStatus.DataMissing,
                    snapshot == null ? "Feature snapshot is missing."
                        : snapshot.SourceReason ?? "Feature snapshot is incomplete.");
            }

            if (!snapshot.HasCandidateSetup
                || !NinjaTraderSafetyGuards.IsLongSetupAllowed(snapshot.CandidateSetup)
                || isSetupEnabled == null
                || !isSetupEnabled(snapshot.CandidateSetup))
            {
                return Blocked(snapshot, OrderFlowSignalState.Rejected,
                    LongDecisionStatus.NoTrade, "No enabled long setup candidate is present.");
            }

            if (!snapshot.LongContextAllowed)
            {
                return Blocked(snapshot, OrderFlowSignalState.Rejected,
                    LongDecisionStatus.InvalidContext, "Long context validation failed.");
            }

            if (!snapshot.LongLocationValid)
            {
                return Blocked(snapshot, OrderFlowSignalState.Rejected,
                    LongDecisionStatus.InvalidLocation, "Long location validation failed.");
            }

            if (!snapshot.LongConfirmationPresent)
            {
                return Blocked(snapshot, OrderFlowSignalState.WaitingForConfirmation,
                    LongDecisionStatus.WaitingForConfirmation,
                    "Waiting for order-flow confirmation.");
            }

            if (!(snapshot.StopPrice < snapshot.EntryPrice
                && snapshot.EntryPrice < snapshot.TargetPrice))
            {
                return Blocked(snapshot, OrderFlowSignalState.Rejected,
                    LongDecisionStatus.InvalidRiskReward,
                    "Long geometry requires stop below entry and target above entry.");
            }

            double risk = snapshot.EntryPrice - snapshot.StopPrice;
            double reward = snapshot.TargetPrice - snapshot.EntryPrice;
            double ratio = reward / risk;
            if (minimumRewardRisk <= 0 || ratio < minimumRewardRisk)
            {
                return Blocked(snapshot, OrderFlowSignalState.Rejected,
                    LongDecisionStatus.InvalidRiskReward,
                    "Reward-to-risk is below the configured minimum.");
            }

            return new LongDecisionResult
            {
                Symbol = snapshot.Symbol ?? string.Empty,
                SetupType = snapshot.CandidateSetup,
                SignalState = OrderFlowSignalState.ConfirmedLong,
                DecisionStatus = LongDecisionStatus.LongValid,
                Reason = snapshot.SourceReason ?? "Long-only CLC candidate validated.",
                EntryPrice = snapshot.EntryPrice,
                StopPrice = snapshot.StopPrice,
                TargetPrice = snapshot.TargetPrice,
                RewardRiskRatio = ratio
            };
        }

        private static LongDecisionResult Blocked(
            OrderFlowFeatureSnapshot snapshot,
            OrderFlowSignalState signalState,
            LongDecisionStatus decisionStatus,
            string reason)
        {
            return new LongDecisionResult
            {
                Symbol = snapshot == null ? string.Empty : snapshot.Symbol ?? string.Empty,
                SetupType = snapshot != null && snapshot.HasCandidateSetup
                    ? (LongSetupType?)snapshot.CandidateSetup
                    : null,
                SignalState = signalState,
                DecisionStatus = decisionStatus,
                Reason = reason ?? string.Empty
            };
        }
    }
}
