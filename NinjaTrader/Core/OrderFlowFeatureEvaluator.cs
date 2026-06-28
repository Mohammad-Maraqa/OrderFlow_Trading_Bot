using System;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class OrderFlowFeatureEvaluator
    {
        public OrderFlowFeatureSnapshot EvaluateApproximate(
            string symbol,
            DateTime timestamp,
            double open,
            double high,
            double low,
            double close,
            double volume,
            double previousClose,
            double priorCumulativeDelta,
            double priorDeltaMovingAverage,
            double priorVolumeMovingAverage,
            double highVolumeMultiplier)
        {
            double safeVolume = Math.Max(0, volume);
            double buyRatio;
            double sellRatio;

            if (close > open)
            {
                buyRatio = 0.65;
                sellRatio = 0.35;
            }
            else if (close < open)
            {
                buyRatio = 0.35;
                sellRatio = 0.65;
            }
            else
            {
                buyRatio = 0.50;
                sellRatio = 0.50;
            }

            double approximateBuyVolume = safeVolume * buyRatio;
            double approximateSellVolume = safeVolume * sellRatio;
            double barDelta = approximateBuyVolume - approximateSellVolume;
            double cumulativeDelta = priorCumulativeDelta + barDelta;
            double volumeMovingAverage = priorVolumeMovingAverage;
            double deltaMovingAverage = priorDeltaMovingAverage;
            bool hasVolumeAverage = volumeMovingAverage > 0;
            bool isHighVolumeBar = hasVolumeAverage
                && safeVolume > volumeMovingAverage * Math.Max(0.1, highVolumeMultiplier);
            bool isPositiveDelta = barDelta > 0;
            bool isNegativeDelta = barDelta < 0;
            bool isCvdRising = cumulativeDelta > priorCumulativeDelta;
            bool isCvdFalling = cumulativeDelta < priorCumulativeDelta;
            double deltaStrength = safeVolume > 0 ? Math.Abs(barDelta) / safeVolume : 0;
            double volumeRatio = volumeMovingAverage > 0 ? safeVolume / volumeMovingAverage : 0;

            OrderFlowBiasState bias = OrderFlowBiasState.Balanced;
            if (isPositiveDelta && isHighVolumeBar)
            {
                bias = OrderFlowBiasState.StrongBuyerPressure;
            }
            else if (isNegativeDelta && isHighVolumeBar)
            {
                bias = OrderFlowBiasState.StrongSellerPressure;
            }
            else if (isPositiveDelta)
            {
                bias = OrderFlowBiasState.BuyerPressure;
            }
            else if (isNegativeDelta)
            {
                bias = OrderFlowBiasState.SellerPressure;
            }

            OrderFlowPressureState pressure = OrderFlowPressureState.LowVolume;
            if (isPositiveDelta && isHighVolumeBar)
            {
                pressure = OrderFlowPressureState.HighVolumePositiveDelta;
            }
            else if (isNegativeDelta && isHighVolumeBar)
            {
                pressure = OrderFlowPressureState.HighVolumeNegativeDelta;
            }
            else if (isPositiveDelta)
            {
                pressure = OrderFlowPressureState.PositiveDelta;
            }
            else if (isNegativeDelta)
            {
                pressure = OrderFlowPressureState.NegativeDelta;
            }

            return new OrderFlowFeatureSnapshot
            {
                Symbol = symbol ?? string.Empty,
                Timestamp = timestamp,
                CurrentPrice = close,
                IsComplete = false,
                HasCandidateSetup = false,
                LongConfirmationPresent = false,
                BarVolume = safeVolume,
                ApproxBuyVolume = approximateBuyVolume,
                ApproxSellVolume = approximateSellVolume,
                BarDelta = barDelta,
                CumulativeDelta = cumulativeDelta,
                DeltaMovingAverage = deltaMovingAverage,
                VolumeMovingAverage = volumeMovingAverage,
                DeltaStrength = deltaStrength,
                VolumeRatio = volumeRatio,
                IsHighVolumeBar = isHighVolumeBar,
                IsPositiveDelta = isPositiveDelta,
                IsNegativeDelta = isNegativeDelta,
                IsCvdRising = isCvdRising,
                IsCvdFalling = isCvdFalling,
                HasApproxOrderFlow = true,
                UsesApproximation = true,
                Source = "NT-3A approximate bar-direction volume model",
                SourceReason = "NT-3A order-flow feature layer is approximate; confirmation engine not implemented.",
                Reason = "Approximate order-flow features only; true bid/ask volumetric data not wired yet.",
                OrderFlowBias = bias,
                OrderFlowPressure = pressure
            };
        }
    }
}
