using System;
using System.Globalization;

namespace NinjaTrader.NinjaScript.OrderFlowAgent
{
    public sealed class StrategyFilterEngine
    {
        public const string FilteredCandidatePrefix = "FILTERED_CANDIDATE=";
        public const string FilterSummaryPrefix = "FILTER_SUMMARY=";
        public const string QualityGatePassedPrefix = "QUALITY_GATE_PASSED=";
        public const string OriginalStrategyFilteredPrefix = "ORIGINAL_STRATEGY_FILTERED=";
        public const string OriginalStrategyCandidatePrefix = "ORIGINAL_STRATEGY_CANDIDATE=";
        public const string AdaptiveTargetPlanPrefix = "ADAPTIVE_TARGET_PLAN=";

        public int TotalCandidatesSeen { get; private set; }
        public int TotalCandidatesAllowed { get; private set; }
        public int TotalCandidatesFiltered { get; private set; }
        public int FilteredBySetupDisabled { get; private set; }
        public int FilteredByConfirmation { get; private set; }
        public int FilteredByScore { get; private set; }
        public int FilteredByLocation { get; private set; }
        public int FilteredByOrderFlowPressure { get; private set; }
        public int AllowedByProfile { get; private set; }
        public int HtfRejected { get; private set; }
        public int AmdRejected { get; private set; }
        public int SweepRejected { get; private set; }
        public int FvgRejected { get; private set; }
        public int DisplacementRejected { get; private set; }
        public int OteRejected { get; private set; }
        public int TargetQualityRejected { get; private set; }
        public int QualityGatePassed { get; private set; }
        public int NoRoadmapRejected { get; private set; }
        public int AcceptanceRejected { get; private set; }
        public int OriginalSetupRejected { get; private set; }
        public int NoTargetPlanRejected { get; private set; }
        public int ChasingRejected { get; private set; }
        public int OriginalStrategyPassed { get; private set; }

        public StrategyFilterResult Evaluate(
            bool enableStrategyFilterLayer,
            StrategyFilterProfile profile,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            bool allowBreakoutPullbackLong,
            bool allowFailedBreakdownLong,
            bool allowValueReclaimLong,
            bool allowDeviationRejectionLong,
            bool allowPullbackContinuationLong,
            int minimumConfirmationScore,
            double minimumRewardRisk,
            bool requireConfirmationObserved,
            bool requireBuyerPressure,
            bool rejectStrongSellerPressure,
            bool rejectNoConfirmation,
            bool rejectWeakConfirmation,
            bool rejectBreakoutAboveUpperDeviation,
            bool allowBreakoutOnlyNearOrAboveVah,
            bool allowFailedBreakdownOnlyBelowOrNearVal,
            bool allowLongOnlyWhenContextNotStronglyBearishForBreakout,
            bool rejectInsideValueBreakoutChase,
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot,
            double minimumTargetRewardRisk,
            double preferredTargetRewardRisk,
            ValueRoadmapSnapshot valueRoadmapSnapshot,
            ValueAcceptanceSnapshot valueAcceptanceSnapshot,
            OriginalStrategySetupType originalSetupType,
            AdaptiveTargetPlan adaptiveTargetPlan,
            bool enableOriginalStrategyAlignment,
            bool requireRthSessionOnly,
            bool isRthSession,
            bool requireClearValueRoadmap,
            bool requireValueAcceptance,
            bool requireOriginalSetupType,
            bool requireLogicalValueTarget,
            int minimumLogicalTargetRoomTicks,
            int minOriginalConfirmationScore)
        {
            StrategyFilterResult result = BuildResult(profile, candidateSnapshot, confirmationSnapshot, contextSnapshot, sessionStructureSnapshot);
            if (candidateSnapshot == null || !candidateSnapshot.HasCandidate)
            {
                result.IsAllowed = false;
                result.IsFiltered = false;
                result.FilterReason = "No candidate.";
                return result;
            }

            TotalCandidatesSeen++;

            if (!enableStrategyFilterLayer || profile == StrategyFilterProfile.Baseline)
            {
                return Allow(result, "Baseline profile allowed observation.");
            }

            if (profile == StrategyFilterProfile.IctAmdLiquidityV1)
            {
                StrategyFilterResult ictResult = ApplyIctAmdLiquidityV1(
                    result,
                    htfBiasSnapshot,
                    marketPhaseSnapshot,
                    liquiditySweepSnapshot,
                    fairValueGapSnapshot,
                    displacementMomentumSnapshot,
                    oteZoneSnapshot,
                    targetQualitySnapshot,
                    confirmationSnapshot,
                    candidateSnapshot,
                    minimumConfirmationScore,
                    minimumTargetRewardRisk,
                    preferredTargetRewardRisk);
                if (ictResult.IsAllowed)
                {
                    QualityGatePassed++;
                }

                return ictResult;
            }

            if (profile == StrategyFilterProfile.OriginalValueRoadmapV1)
            {
                StrategyFilterResult originalResult = ApplyOriginalValueRoadmapV1(
                    result,
                    valueRoadmapSnapshot,
                    valueAcceptanceSnapshot,
                    originalSetupType,
                    adaptiveTargetPlan,
                    confirmationSnapshot,
                    enableOriginalStrategyAlignment,
                    requireRthSessionOnly,
                    isRthSession,
                    requireClearValueRoadmap,
                    requireValueAcceptance,
                    requireOriginalSetupType,
                    requireLogicalValueTarget,
                    minimumLogicalTargetRoomTicks,
                    minOriginalConfirmationScore);
                if (originalResult.IsAllowed)
                {
                    OriginalStrategyPassed++;
                }

                return originalResult;
            }

            int scoreThreshold = profile == StrategyFilterProfile.StrictReplayValidation
                ? Math.Max(90, minimumConfirmationScore)
                : minimumConfirmationScore;

            string setupDisabledReason = DisabledSetupReason(
                candidateSnapshot.CandidateSetupType,
                allowBreakoutPullbackLong,
                allowFailedBreakdownLong,
                allowValueReclaimLong,
                allowDeviationRejectionLong,
                allowPullbackContinuationLong);
            if (!string.IsNullOrEmpty(setupDisabledReason))
            {
                FilteredBySetupDisabled++;
                return Filter(result, setupDisabledReason);
            }

            if (confirmationSnapshot == null)
            {
                FilteredByConfirmation++;
                return Filter(result, "Missing confirmation snapshot.");
            }

            if (requireConfirmationObserved
                && confirmationSnapshot.ConfirmationState != OrderFlowConfirmationState.ConfirmationObserved)
            {
                FilteredByConfirmation++;
                return Filter(result, "ConfirmationState is not ConfirmationObserved.");
            }

            if (rejectNoConfirmation
                && confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.NoConfirmation)
            {
                FilteredByConfirmation++;
                return Filter(result, "NoConfirmation rejected by profile.");
            }

            if (rejectWeakConfirmation
                && confirmationSnapshot.ConfirmationState == OrderFlowConfirmationState.WeakConfirmation)
            {
                FilteredByConfirmation++;
                return Filter(result, "WeakConfirmation rejected by profile.");
            }

            if (confirmationSnapshot.ConfirmationScore < scoreThreshold)
            {
                FilteredByScore++;
                return Filter(result, "ConfirmationScore below profile threshold.");
            }

            if (candidateSnapshot.CandidateRewardRisk < minimumRewardRisk)
            {
                FilteredByScore++;
                return Filter(result, "CandidateRewardRisk below profile threshold.");
            }

            if (rejectStrongSellerPressure
                && orderFlowSnapshot != null
                && orderFlowSnapshot.OrderFlowBias == OrderFlowBiasState.StrongSellerPressure)
            {
                FilteredByOrderFlowPressure++;
                return Filter(result, "StrongSellerPressure rejected by profile.");
            }

            if (requireBuyerPressure
                && orderFlowSnapshot != null
                && orderFlowSnapshot.OrderFlowBias != OrderFlowBiasState.BuyerPressure
                && orderFlowSnapshot.OrderFlowBias != OrderFlowBiasState.StrongBuyerPressure)
            {
                FilteredByOrderFlowPressure++;
                return Filter(result, "Buyer pressure required by profile.");
            }

            StrategyFilterResult locationResult = ApplyLocationFilters(
                result,
                candidateSnapshot,
                rejectBreakoutAboveUpperDeviation,
                allowBreakoutOnlyNearOrAboveVah,
                allowFailedBreakdownOnlyBelowOrNearVal,
                allowLongOnlyWhenContextNotStronglyBearishForBreakout,
                rejectInsideValueBreakoutChase,
                profile == StrategyFilterProfile.StrictReplayValidation);
            if (locationResult.IsFiltered)
            {
                FilteredByLocation++;
                return locationResult;
            }

            return Allow(result, "Allowed by profile.");
        }

        public void PrintFilteredCandidate(StrategyFilterResult result, Action<string> print)
        {
            if (result == null || !result.IsFiltered)
            {
                return;
            }

            SafePrint(print, FilteredCandidatePrefix
                + "Candidate=" + result.CandidateSetupType
                + " Reason=" + result.FilterReason
                + " Profile=" + result.FilterProfile
                + " ConfirmationState=" + result.ConfirmationState
                + " Score=" + Format(result.ConfirmationScore)
                + " Context=" + result.ContextState
                + " Location=" + result.LocationState
                + " ValueState=" + result.ValueState
                + " RR=" + Format(result.CandidateRewardRisk));
        }

        public void PrintIctFilteredCandidate(
            StrategyFilterResult result,
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot,
            Action<string> print)
        {
            if (result == null || !result.IsFiltered)
            {
                return;
            }

            SafePrint(print, FilteredCandidatePrefix
                + "Candidate=" + result.CandidateSetupType
                + " Reason=" + result.FilterReason
                + " Profile=" + result.FilterProfile
                + IctSnapshotText(htfBiasSnapshot, marketPhaseSnapshot, liquiditySweepSnapshot, fairValueGapSnapshot, displacementMomentumSnapshot, oteZoneSnapshot, targetQualitySnapshot)
                + " Score=" + Format(result.ConfirmationScore)
                + " RR=" + Format(result.CandidateRewardRisk));
        }

        public void PrintQualityGatePassed(
            StrategyFilterResult result,
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot,
            Action<string> print)
        {
            if (result == null || !result.IsAllowed)
            {
                return;
            }

            SafePrint(print, QualityGatePassedPrefix
                + "Candidate=" + result.CandidateSetupType
                + " Profile=" + result.FilterProfile
                + IctSnapshotText(htfBiasSnapshot, marketPhaseSnapshot, liquiditySweepSnapshot, fairValueGapSnapshot, displacementMomentumSnapshot, oteZoneSnapshot, targetQualitySnapshot)
                + " Score=" + Format(result.ConfirmationScore)
                + " RR=" + Format(result.CandidateRewardRisk));
        }

        public void PrintOriginalStrategyFiltered(
            StrategyFilterResult result,
            OriginalStrategySetupType originalSetupType,
            ValueRoadmapSnapshot roadmap,
            ValueAcceptanceSnapshot acceptance,
            AdaptiveTargetPlan targetPlan,
            Action<string> print)
        {
            if (result == null || !result.IsFiltered)
            {
                return;
            }

            SafePrint(print, OriginalStrategyFilteredPrefix
                + "Candidate=" + result.CandidateSetupType
                + " OriginalSetup=" + originalSetupType
                + " Reason=" + result.FilterReason
                + " Roadmap=" + RoadmapName(roadmap)
                + " Acceptance=" + AcceptanceName(acceptance)
                + " TargetPlan=" + (targetPlan != null && targetPlan.HasTargetPlan)
                + " ConfirmationState=" + result.ConfirmationState
                + " Score=" + Format(result.ConfirmationScore)
                + " ExecutionDisabled=True");
        }

        public void PrintOriginalStrategyCandidate(
            StrategyFilterResult result,
            OriginalStrategySetupType originalSetupType,
            ValueRoadmapSnapshot roadmap,
            ValueAcceptanceSnapshot acceptance,
            AdaptiveTargetPlan targetPlan,
            Action<string> print)
        {
            if (result == null || !result.IsAllowed)
            {
                return;
            }

            SafePrint(print, OriginalStrategyCandidatePrefix
                + "OriginalSetup=" + originalSetupType
                + " Roadmap=" + RoadmapName(roadmap)
                + " Acceptance=" + AcceptanceName(acceptance)
                + " Target1=" + Format(targetPlan == null ? 0 : targetPlan.Target1)
                + " Target2=" + Format(targetPlan == null ? 0 : targetPlan.Target2)
                + " FinalTarget=" + Format(targetPlan == null ? 0 : targetPlan.FinalTarget)
                + " ConfirmationState=" + result.ConfirmationState
                + " Score=" + Format(result.ConfirmationScore)
                + " ExecutionDisabled=True");
        }

        public void PrintAdaptiveTargetPlan(AdaptiveTargetPlan targetPlan, Action<string> print)
        {
            if (targetPlan == null || !targetPlan.HasTargetPlan)
            {
                return;
            }

            SafePrint(print, AdaptiveTargetPlanPrefix
                + "Entry=" + Format(targetPlan.EntryPrice)
                + " Stop=" + Format(targetPlan.StopPrice)
                + " Target1=" + Format(targetPlan.Target1)
                + " Target1Reason=" + targetPlan.Target1Reason
                + " Target2=" + Format(targetPlan.Target2)
                + " Target2Reason=" + targetPlan.Target2Reason
                + " FinalTarget=" + Format(targetPlan.FinalTarget)
                + " FinalTargetReason=" + targetPlan.FinalTargetReason
                + " EstimatedRR=" + Format(targetPlan.EstimatedRewardRiskToFinal));
        }

        public void PrintFilterSummary(StrategyFilterProfile profile, Action<string> print)
        {
            SafePrint(print, FilterSummaryPrefix
                + "Profile=" + profile
                + " Seen=" + TotalCandidatesSeen
                + " Allowed=" + TotalCandidatesAllowed
                + " Filtered=" + TotalCandidatesFiltered
                + " SetupDisabled=" + FilteredBySetupDisabled
                + " Confirmation=" + FilteredByConfirmation
                + " Score=" + FilteredByScore
                + " Location=" + FilteredByLocation
                + " OrderFlow=" + FilteredByOrderFlowPressure
                + " HtfRejected=" + HtfRejected
                + " AmdRejected=" + AmdRejected
                + " SweepRejected=" + SweepRejected
                + " FvgRejected=" + FvgRejected
                + " DisplacementRejected=" + DisplacementRejected
                + " OteRejected=" + OteRejected
                + " TargetQualityRejected=" + TargetQualityRejected
                + " QualityGatePassed=" + QualityGatePassed
                + " NoRoadmapRejected=" + NoRoadmapRejected
                + " AcceptanceRejected=" + AcceptanceRejected
                + " OriginalSetupRejected=" + OriginalSetupRejected
                + " NoTargetPlanRejected=" + NoTargetPlanRejected
                + " ChasingRejected=" + ChasingRejected
                + " OriginalStrategyPassed=" + OriginalStrategyPassed);
        }

        private StrategyFilterResult ApplyOriginalValueRoadmapV1(
            StrategyFilterResult result,
            ValueRoadmapSnapshot roadmap,
            ValueAcceptanceSnapshot acceptance,
            OriginalStrategySetupType originalSetupType,
            AdaptiveTargetPlan targetPlan,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            bool enableOriginalStrategyAlignment,
            bool requireRthSessionOnly,
            bool isRthSession,
            bool requireClearValueRoadmap,
            bool requireValueAcceptance,
            bool requireOriginalSetupType,
            bool requireLogicalValueTarget,
            int minimumLogicalTargetRoomTicks,
            int minOriginalConfirmationScore)
        {
            // Order flow is the final confirmation, not the strategy; this gate is observation-only and non-executable.
            if (!enableOriginalStrategyAlignment)
            {
                return Allow(result, "Original strategy alignment disabled.");
            }

            if (requireRthSessionOnly && !isRthSession)
            {
                ChasingRejected++;
                return Filter(result, "Not RTH / New York session.");
            }

            if (requireClearValueRoadmap && (roadmap == null || !roadmap.HasClearRoadmap))
            {
                NoRoadmapRejected++;
                return Filter(result, "No clear value roadmap.");
            }

            if (requireValueAcceptance && (acceptance == null || (!acceptance.HasAcceptance && !acceptance.HasRejection) || !acceptance.IsLongSupportive))
            {
                AcceptanceRejected++;
                return Filter(result, "No acceptance/rejection signal.");
            }

            if (requireOriginalSetupType
                && (originalSetupType == OriginalStrategySetupType.None
                    || originalSetupType == OriginalStrategySetupType.Invalid))
            {
                OriginalSetupRejected++;
                return Filter(result, "Candidate does not map to original setup type.");
            }

            if (confirmationSnapshot == null
                || confirmationSnapshot.ConfirmationState != OrderFlowConfirmationState.ConfirmationObserved)
            {
                FilteredByConfirmation++;
                return Filter(result, "Order-flow confirmation is not observed.");
            }

            if (confirmationSnapshot.ConfirmationScore < minOriginalConfirmationScore)
            {
                FilteredByScore++;
                return Filter(result, "Confirmation score below original strategy threshold.");
            }

            if (requireLogicalValueTarget && (targetPlan == null || !targetPlan.HasTargetPlan || !targetPlan.IsLogicalTargetPlan))
            {
                NoTargetPlanRejected++;
                return Filter(result, "No logical value/VWAP/CVA target plan.");
            }

            if (targetPlan != null && targetPlan.EstimatedRewardRiskToFinal <= 0)
            {
                NoTargetPlanRejected++;
                return Filter(result, "Target room is too small.");
            }

            return Allow(result, "Original value roadmap candidate passed.");
        }

        private StrategyFilterResult ApplyIctAmdLiquidityV1(
            StrategyFilterResult result,
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            int minimumConfirmationScore,
            double minimumTargetRewardRisk,
            double preferredTargetRewardRisk)
        {
            if (htfBiasSnapshot == null || !htfBiasSnapshot.AllowsLongs)
            {
                HtfRejected++;
                return Filter(result, htfBiasSnapshot != null && htfBiasSnapshot.BiasState == HigherTimeframeBiasState.StrongBearish
                    ? "Strong bearish higher timeframe bias."
                    : "HTF bias does not allow longs.");
            }

            if (marketPhaseSnapshot == null || !marketPhaseSnapshot.HasAccumulation || !marketPhaseSnapshot.HasManipulation || !marketPhaseSnapshot.HasDistribution)
            {
                AmdRejected++;
                return Filter(result, "No accumulation phase before manipulation.");
            }

            if (liquiditySweepSnapshot == null || !liquiditySweepSnapshot.HasSellSideSweep)
            {
                SweepRejected++;
                return Filter(result, "No sell-side liquidity sweep.");
            }

            if (!liquiditySweepSnapshot.HasReclaim)
            {
                SweepRejected++;
                return Filter(result, "No reclaim after sweep.");
            }

            if (displacementMomentumSnapshot == null || !displacementMomentumSnapshot.HasBullishDisplacement)
            {
                DisplacementRejected++;
                return Filter(result, "No bullish displacement after sweep.");
            }

            if (fairValueGapSnapshot == null || !fairValueGapSnapshot.HasBullishFvg)
            {
                FvgRejected++;
                return Filter(result, "No bullish FVG after displacement.");
            }

            if (oteZoneSnapshot == null || (!oteZoneSnapshot.IsInOteZone && !oteZoneSnapshot.IsInDiscount))
            {
                OteRejected++;
                return Filter(result, "Entry is not in OTE/discount.");
            }

            if (oteZoneSnapshot.OteState == OteZoneState.InPremium)
            {
                OteRejected++;
                return Filter(result, "Entry is in premium.");
            }

            if (confirmationSnapshot == null
                || confirmationSnapshot.ConfirmationState != OrderFlowConfirmationState.ConfirmationObserved)
            {
                FilteredByConfirmation++;
                return Filter(result, "Order-flow confirmation is not observed.");
            }

            if (confirmationSnapshot.ConfirmationScore < minimumConfirmationScore)
            {
                FilteredByScore++;
                return Filter(result, "ConfirmationScore below profile threshold.");
            }

            if (targetQualitySnapshot == null
                || targetQualitySnapshot.TargetQualityState == IctTargetQualityState.Poor
                || targetQualitySnapshot.TargetQualityState == IctTargetQualityState.Unknown)
            {
                TargetQualityRejected++;
                return Filter(result, "Target quality is poor.");
            }

            if (targetQualitySnapshot.TargetRoomTicks <= 0)
            {
                TargetQualityRejected++;
                return Filter(result, "Not enough target room.");
            }

            if (targetQualitySnapshot.RewardRisk < minimumTargetRewardRisk
                || (preferredTargetRewardRisk > minimumTargetRewardRisk && targetQualitySnapshot.RewardRisk < minimumTargetRewardRisk))
            {
                TargetQualityRejected++;
                return Filter(result, "Reward:risk below preferred threshold.");
            }

            return Allow(result, "ICT AMD liquidity quality gate passed.");
        }

        private StrategyFilterResult ApplyLocationFilters(
            StrategyFilterResult result,
            LongSetupCandidateSnapshot candidateSnapshot,
            bool rejectBreakoutAboveUpperDeviation,
            bool allowBreakoutOnlyNearOrAboveVah,
            bool allowFailedBreakdownOnlyBelowOrNearVal,
            bool allowLongOnlyWhenContextNotStronglyBearishForBreakout,
            bool rejectInsideValueBreakoutChase,
            bool strictReplayValidation)
        {
            if (candidateSnapshot.CandidateSetupType == LongSetupType.BreakoutPullbackLong)
            {
                if (rejectBreakoutAboveUpperDeviation
                    && candidateSnapshot.LocationState == PriceLocationState.AboveUpperDeviation.ToString())
                {
                    return Filter(result, "Rejected breakout chase above upper deviation.");
                }

                if (allowBreakoutOnlyNearOrAboveVah
                    && candidateSnapshot.ValueState != ValueAreaState.AboveValue.ToString()
                    && candidateSnapshot.ValueState != ValueAreaState.NearVAH.ToString())
                {
                    return Filter(result, "Breakout pullback must be AboveValue or NearVAH.");
                }

                if (allowLongOnlyWhenContextNotStronglyBearishForBreakout
                    && candidateSnapshot.ContextState == MarketContextState.ExtendedBearish.ToString())
                {
                    return Filter(result, "Breakout pullback rejected in ExtendedBearish context.");
                }

                if (rejectInsideValueBreakoutChase
                    && candidateSnapshot.ValueState == ValueAreaState.InsideValue.ToString())
                {
                    return Filter(result, "Inside-value breakout chase rejected.");
                }
            }

            if (candidateSnapshot.CandidateSetupType == LongSetupType.FailedBreakdownLong
                && allowFailedBreakdownOnlyBelowOrNearVal
                && candidateSnapshot.ValueState != ValueAreaState.BelowValue.ToString()
                && candidateSnapshot.ValueState != ValueAreaState.NearVAL.ToString()
                && candidateSnapshot.ValueState != ValueAreaState.InsideValue.ToString())
            {
                return Filter(result, "Failed breakdown must be BelowValue, NearVAL, or InsideValue.");
            }

            if (strictReplayValidation
                && candidateSnapshot.LocationState == PriceLocationState.AboveUpperDeviation.ToString())
            {
                return Filter(result, "StrictReplayValidation rejects upper-deviation long chase.");
            }

            return result;
        }

        private static StrategyFilterResult BuildResult(
            StrategyFilterProfile profile,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot)
        {
            return new StrategyFilterResult
            {
                CandidateSetupType = candidateSnapshot == null || !candidateSnapshot.HasCandidate
                    ? "None"
                    : candidateSnapshot.CandidateSetupType.ToString(),
                FilterProfile = profile.ToString(),
                ConfirmationScore = confirmationSnapshot == null ? 0 : confirmationSnapshot.ConfirmationScore,
                ConfirmationState = confirmationSnapshot == null ? string.Empty : confirmationSnapshot.ConfirmationState.ToString(),
                ContextState = contextSnapshot == null ? string.Empty : contextSnapshot.ContextState.ToString(),
                LocationState = contextSnapshot == null ? string.Empty : contextSnapshot.LocationState.ToString(),
                ValueState = sessionStructureSnapshot == null ? string.Empty : sessionStructureSnapshot.ValueState.ToString(),
                CandidateRewardRisk = candidateSnapshot == null ? 0 : candidateSnapshot.CandidateRewardRisk,
                ExecutionDisabled = true,
                EvaluationOnlyMode = true,
                FilterReason = "NT-4F strategy filter is observation-only and non-executable."
            };
        }

        private StrategyFilterResult Allow(StrategyFilterResult result, string reason)
        {
            result.IsAllowed = true;
            result.IsFiltered = false;
            result.FilterReason = reason;
            TotalCandidatesAllowed++;
            AllowedByProfile++;
            return result;
        }

        private StrategyFilterResult Filter(StrategyFilterResult result, string reason)
        {
            result.IsAllowed = false;
            result.IsFiltered = true;
            result.FilterReason = reason;
            TotalCandidatesFiltered++;
            return result;
        }

        private static string DisabledSetupReason(
            LongSetupType setupType,
            bool allowBreakoutPullbackLong,
            bool allowFailedBreakdownLong,
            bool allowValueReclaimLong,
            bool allowDeviationRejectionLong,
            bool allowPullbackContinuationLong)
        {
            if (setupType == LongSetupType.BreakoutPullbackLong && !allowBreakoutPullbackLong)
            {
                return "Setup disabled by DiagnosticV2.";
            }

            if (setupType == LongSetupType.FailedBreakdownLong && !allowFailedBreakdownLong)
            {
                return "Setup disabled by DiagnosticV2.";
            }

            if (setupType == LongSetupType.ValueReclaimLong && !allowValueReclaimLong)
            {
                return "Setup disabled by DiagnosticV2.";
            }

            if (setupType == LongSetupType.DeviationRejectionLong && !allowDeviationRejectionLong)
            {
                return "Setup disabled by DiagnosticV2.";
            }

            if (setupType == LongSetupType.PullbackContinuationLong && !allowPullbackContinuationLong)
            {
                return "Setup disabled by DiagnosticV2.";
            }

            return string.Empty;
        }

        private static string IctSnapshotText(
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot)
        {
            return " HTFBias=" + (htfBiasSnapshot == null ? "Unknown" : htfBiasSnapshot.BiasState.ToString())
                + " AMD=" + (marketPhaseSnapshot == null ? "Unknown" : marketPhaseSnapshot.PhaseState.ToString())
                + " Sweep=" + (liquiditySweepSnapshot == null ? "Unknown" : liquiditySweepSnapshot.SweepState.ToString())
                + " FVG=" + (fairValueGapSnapshot == null ? "Unknown" : fairValueGapSnapshot.FvgState.ToString())
                + " Displacement=" + (displacementMomentumSnapshot == null ? "None" : displacementMomentumSnapshot.MomentumState.ToString())
                + " OTE=" + (oteZoneSnapshot == null ? "Unknown" : oteZoneSnapshot.OteState.ToString())
                + " TargetQuality=" + (targetQualitySnapshot == null ? "Unknown" : targetQualitySnapshot.TargetQualityState.ToString());
        }

        private static string RoadmapName(ValueRoadmapSnapshot roadmap)
        {
            return roadmap == null || string.IsNullOrEmpty(roadmap.RoadmapState)
                ? "Unknown"
                : roadmap.RoadmapState;
        }

        private static string AcceptanceName(ValueAcceptanceSnapshot acceptance)
        {
            return acceptance == null
                ? "Unknown"
                : acceptance.AcceptanceState.ToString();
        }

        private static string Format(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }

        private static void SafePrint(Action<string> print, string message)
        {
            if (print != null)
            {
                print(message);
            }
        }
    }
}
