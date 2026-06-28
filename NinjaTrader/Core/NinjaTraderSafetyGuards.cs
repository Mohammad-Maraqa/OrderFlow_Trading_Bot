using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public static class NinjaTraderSafetyGuards
    {
        public static bool IsLongSetupAllowed(LongSetupType setupType)
        {
            switch (setupType)
            {
                case LongSetupType.FailedBreakdownLong:
                case LongSetupType.PullbackContinuationLong:
                case LongSetupType.ValueReclaimLong:
                case LongSetupType.BreakoutPullbackLong:
                case LongSetupType.DeviationRejectionLong:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsShortSetupForbidden(string setupName)
        {
            if (string.IsNullOrWhiteSpace(setupName))
            {
                return false;
            }

            string normalized = setupName.Trim().ToUpperInvariant();
            return normalized.Contains("SHORT") || normalized.Contains("SELL_TO_OPEN");
        }

        public static bool IsExecutionAllowed(
            bool evaluationOnlyMode,
            bool useSimOnly,
            bool allowLiveTrading)
        {
            return false;
        }

        public static bool IsEvaluationOnly(bool evaluationOnlyMode)
        {
            return evaluationOnlyMode;
        }

        public static bool IsLiveTradingForbidden(bool allowLiveTrading)
        {
            return !allowLiveTrading;
        }

        public static bool ValidateLongOnlyDecision(LongDecisionResult decision)
        {
            if (decision == null
                || decision.DecisionStatus != LongDecisionStatus.LongValid
                || !decision.SetupType.HasValue
                || !IsLongSetupAllowed(decision.SetupType.Value)
                || !decision.EntryPrice.HasValue
                || !decision.StopPrice.HasValue
                || !decision.TargetPrice.HasValue)
            {
                return false;
            }

            return decision.StopPrice.Value < decision.EntryPrice.Value
                && decision.EntryPrice.Value < decision.TargetPrice.Value;
        }
    }
}
