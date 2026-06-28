#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.OrderFlowAgent;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class LongOnlyOrderFlowAgentStrategy : Strategy
    {
        private LongOnlyOrderFlowEvaluator evaluator;
        private MarketContextEvaluator contextEvaluator;
        private SessionStructureEvaluator sessionStructureEvaluator;
        private LongSetupCandidateEvaluator setupCandidateEvaluator;
        private OrderFlowFeatureEvaluator orderFlowFeatureEvaluator;
        private OrderFlowConfirmationEvaluator orderFlowConfirmationEvaluator;
        private SignalObservationJournalWriter signalObservationJournalWriter;
        private HypotheticalOutcomeTracker hypotheticalOutcomeTracker;
        private HypotheticalPerformanceTracker hypotheticalPerformanceTracker;
        private ReplayValidationTracker replayValidationTracker;
        private StrategyDiagnosticsEngine strategyDiagnosticsEngine;
        private StrategyDiagnosticSummary latestStrategyDiagnosticSummary;
        private StrategyFilterEngine strategyFilterEngine;
        private HigherTimeframeBiasEvaluator higherTimeframeBiasEvaluator;
        private AmdMarketPhaseEvaluator amdMarketPhaseEvaluator;
        private LiquiditySweepEvaluator liquiditySweepEvaluator;
        private FairValueGapEvaluator fairValueGapEvaluator;
        private DisplacementMomentumEvaluator displacementMomentumEvaluator;
        private OteZoneEvaluator oteZoneEvaluator;
        private IctTargetQualityEvaluator ictTargetQualityEvaluator;
        private ValueRoadmapEvaluator valueRoadmapEvaluator;
        private ValueAcceptanceEvaluator valueAcceptanceEvaluator;
        private AdaptiveTargetPlanner adaptiveTargetPlanner;
        private int lastDiagnosticsClosedOutcomes;
        private bool startupSummaryPrinted;
        private int lastSessionResetBar = -1;
        private string lastJournalSignature = string.Empty;
        private int lastJournalBar = -1;
        private bool journaledThisBar;
        private double sessionHigh;
        private double sessionLow;
        private double priorSessionHigh;
        private double priorSessionLow;
        private double openingRangeHigh;
        private double openingRangeLow;
        private DateTime sessionStartTime;
        private double cumulativeTypicalPriceVolume;
        private double cumulativeVolume;
        private double approximateSessionVwap;
        private readonly Dictionary<double, double> volumeByPrice = new Dictionary<double, double>();
        private double approximatePoc;
        private double approximateVah;
        private double approximateVal;
        private ContextFeatureSnapshot latestContextSnapshot;
        private SessionStructureSnapshot latestSessionStructureSnapshot;
        private LongSetupCandidateSnapshot latestLongSetupCandidateSnapshot;
        private OrderFlowFeatureSnapshot latestOrderFlowFeatureSnapshot;
        private OrderFlowConfirmationSnapshot latestOrderFlowConfirmationSnapshot;
        private HigherTimeframeBiasSnapshot latestHigherTimeframeBiasSnapshot;
        private MarketPhaseSnapshot latestMarketPhaseSnapshot;
        private LiquiditySweepSnapshot latestLiquiditySweepSnapshot;
        private FairValueGapSnapshot latestFairValueGapSnapshot;
        private DisplacementMomentumSnapshot latestDisplacementMomentumSnapshot;
        private OteZoneSnapshot latestOteZoneSnapshot;
        private IctTargetQualitySnapshot latestIctTargetQualitySnapshot;
        private ValueRoadmapSnapshot latestValueRoadmapSnapshot;
        private ValueAcceptanceSnapshot latestValueAcceptanceSnapshot;
        private AdaptiveTargetPlan latestAdaptiveTargetPlan;
        private OriginalStrategySetupType latestOriginalStrategySetupType = OriginalStrategySetupType.None;
        private readonly Queue<double> recentDeltaValues = new Queue<double>();
        private readonly Queue<double> recentVolumeValues = new Queue<double>();
        private double cumulativeDelta;
        private ValueAreaState PreviousValueState;
        private MarketContextState PreviousContextState;
        private PriceLocationState PreviousLocationState;
        private double PreviousClosePrice;
        private int BarsSinceBelowValue = -1;
        private int BarsSinceAboveValue = -1;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "LongOnlyOrderFlowAgentStrategy";
                Description = "NT-4G evaluation-only long-side order-flow agent.";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 20;
                IsInstantiatedOnEachOptimizationIteration = true;

                EvaluationOnlyMode = true;
                UseSimOnly = true;
                AllowLiveTrading = false;
                Quantity = 1;
                MinRewardRisk = 2.0;
                EnableFailedBreakdownLong = true;
                EnablePullbackContinuationLong = true;
                EnableValueReclaimLong = true;
                EnableBreakoutPullbackLong = true;
                EnableDeviationRejectionLong = true;
                PrintDebug = true;
                PrintStartupSummary = true;
                PrintHeartbeat = true;
                DebugHeartbeatBars = 50;
                PrintEveryEvaluation = false;
                EnableContextLayer = true;
                UseApproximateSessionVwap = true;
                NearVwapTicks = 8;
                VwapBandTicks = 40;
                PrintContextEveryHeartbeat = true;
                EnableValueStructureLayer = true;
                UseApproximateVolumeProfile = true;
                ValueAreaPercent = 70;
                NearValueTicks = 8;
                OpeningRangeMinutes = 30;
                PrintValueStructureEveryHeartbeat = true;
                EnableSetupCandidateDetection = true;
                PrintCandidateEveryHeartbeat = true;
                CandidateLookbackBars = 20;
                MinCandidateRewardRisk = 2.0;
                EnableOrderFlowFeatureLayer = true;
                UseApproximateOrderFlow = true;
                DeltaMovingAveragePeriod = 20;
                VolumeMovingAveragePeriod = 20;
                HighVolumeMultiplier = 1.5;
                PrintOrderFlowEveryHeartbeat = true;
                EnableOrderFlowConfirmationEngine = true;
                PrintConfirmationEveryHeartbeat = true;
                MinConfirmationScore = 70;
                WeakConfirmationScore = 50;
                RequireConfirmationBeforeSignal = true;
                EnableSignalObservationJournal = true;
                JournalOnlyConfirmedCandidates = false;
                JournalOnlyWhenCandidateExists = true;
                JournalFileName = "orderflow_signal_observations.jsonl";
                PrintJournalEvents = true;
                MinimumJournalConfirmationScore = 0;
                JournalCooldownBars = 5;
                EnableHypotheticalOutcomeTracking = true;
                TrackWeakConfirmations = false;
                MaxBarsToTrackOutcome = 50;
                ConservativeSameBarResolution = true;
                PrintOutcomeEvents = true;
                PrintOpenOutcomeCountEveryHeartbeat = true;
                EnablePerformanceSummary = true;
                PrintPerformanceSummary = true;
                PerformanceSummaryEveryClosedOutcomes = 25;
                PrintSetupBreakdown = true;
                TimeoutResultR = 0.0;
                InvalidatedResultR = 0.0;
                DefaultTargetRewardR = 2.0;
                EnableReplayValidation = true;
                PrintReplayValidationSummary = true;
                PrintReplayValidationEveryBars = 500;
                MinimumClosedOutcomesForReview = 50;
                MinimumBarsForReview = 500;
                ReplaySessionLabel = "";
                EnableStrategyDiagnostics = true;
                PrintStrategyDiagnostics = true;
                DiagnosticsEveryClosedOutcomes = 100;
                MinimumClosedOutcomesForDiagnostics = 100;
                MinimumSetupOutcomesForDecision = 20;
                MinimumAverageRForSim101 = 0.05;
                MinimumSetupAverageRToKeep = 0.0;
                EnableStrategyFilterLayer = true;
                StrategyFilterProfile = StrategyFilterProfile.DiagnosticV2;
                PrintFilteredCandidates = true;
                PrintFilterSummaryEveryBars = 500;
                V2AllowBreakoutPullbackLong = true;
                V2AllowFailedBreakdownLong = true;
                V2AllowValueReclaimLong = false;
                V2AllowDeviationRejectionLong = false;
                V2AllowPullbackContinuationLong = false;
                V2MinimumConfirmationScore = 85;
                V2MinimumRewardRisk = 2.0;
                V2RequireConfirmationObserved = true;
                V2RequireBuyerPressure = false;
                V2RejectStrongSellerPressure = true;
                V2RejectNoConfirmation = true;
                V2RejectWeakConfirmation = true;
                V2RejectBreakoutAboveUpperDeviation = true;
                V2AllowBreakoutOnlyNearOrAboveVAH = true;
                V2AllowFailedBreakdownOnlyBelowOrNearVAL = true;
                V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout = true;
                V2RejectInsideValueBreakoutChase = true;
                EnableHigherTimeframeBiasFilter = true;
                HtfFastPeriod = 50;
                HtfSlowPeriod = 200;
                RequireHtfBiasForLongs = true;
                AllowLongsWhenHtfBalanced = false;
                RejectLongsWhenStrongBearish = true;
                EnableAmdPhaseFilter = true;
                RequireAccumulationBeforeManipulation = true;
                AccumulationLookbackBars = 30;
                MaxAccumulationRangeTicks = 80;
                ManipulationLookbackBars = 20;
                MaxBarsFromManipulationToEntry = 20;
                RequireDistributionAfterManipulation = true;
                EnableLiquiditySweepFilter = true;
                RequireSellSideSweepForLongs = true;
                SweepLookbackBars = 30;
                SweepBufferTicks = 2;
                MaxBarsAfterSweep = 15;
                RequireReclaimAfterSweep = true;
                AllowVALSweepAsLiquiditySweep = true;
                AllowRangeLowSweepAsLiquiditySweep = true;
                AllowSwingLowSweepAsLiquiditySweep = true;
                AllowPriorLowSweepAsLiquiditySweep = true;
                EnableFairValueGapFilter = true;
                RequireBullishFvgForLongs = true;
                RequireFvgAfterSweep = true;
                MinFvgSizeTicks = 4;
                TrackFvgRetest = true;
                RequireFvgRetestForEntry = false;
                EnableDisplacementFilter = true;
                RequireBullishDisplacementForLongs = true;
                MinDisplacementBodyTicks = 8;
                MinBodyToRangeRatio = 0.55;
                RequireCloseNearHighForDisplacement = true;
                CloseNearHighPercent = 0.30;
                RequirePositiveDeltaForDisplacement = true;
                EnableOteFilter = true;
                RequireOteForLongs = true;
                OteLowerLevel = 0.61;
                OteMidLevel = 0.70;
                OteUpperLevel = 0.79;
                AllowDiscountButOutsideOte = false;
                RejectPremiumLongEntries = true;
                EnableIctTargetQualityFilter = true;
                MinimumTargetRewardRisk = 2.0;
                PreferredTargetRewardRisk = 2.5;
                MinimumTargetRoomTicks = 20;
                PreferExternalLiquidityTargets = true;
                UseSwingHighAsBuySideLiquidity = true;
                UseSessionHighAsBuySideLiquidity = true;
                UseVAHAsTarget = true;
                UseUpperVwapBandAsTarget = true;
                RejectPoorTargetQuality = true;
                EnableValueAcceptanceLayer = true;
                AcceptanceBarsRequired = 3;
                RejectionBarsRequired = 1;
                NearValueEdgeTicks = 8;
                EnableOriginalStrategyAlignment = true;
                RequireRthSessionOnly = true;
                RequireClearValueRoadmap = true;
                RequireValueAcceptance = true;
                RequireOriginalSetupType = true;
                RequireLogicalValueTarget = true;
                MinimumLogicalTargetRoomTicks = 20;
                MinOriginalConfirmationScore = 85;
                PrintOriginalStrategyEvents = true;
            }
            else if (State == State.DataLoaded)
            {
                evaluator = new LongOnlyOrderFlowEvaluator();
                contextEvaluator = new MarketContextEvaluator();
                sessionStructureEvaluator = new SessionStructureEvaluator();
                setupCandidateEvaluator = new LongSetupCandidateEvaluator();
                orderFlowFeatureEvaluator = new OrderFlowFeatureEvaluator();
                orderFlowConfirmationEvaluator = new OrderFlowConfirmationEvaluator();
                signalObservationJournalWriter = new SignalObservationJournalWriter();
                hypotheticalOutcomeTracker = new HypotheticalOutcomeTracker();
                hypotheticalPerformanceTracker = new HypotheticalPerformanceTracker();
                replayValidationTracker = new ReplayValidationTracker();
                strategyDiagnosticsEngine = new StrategyDiagnosticsEngine();
                latestStrategyDiagnosticSummary = new StrategyDiagnosticSummary();
                strategyFilterEngine = new StrategyFilterEngine();
                higherTimeframeBiasEvaluator = new HigherTimeframeBiasEvaluator();
                amdMarketPhaseEvaluator = new AmdMarketPhaseEvaluator();
                liquiditySweepEvaluator = new LiquiditySweepEvaluator();
                fairValueGapEvaluator = new FairValueGapEvaluator();
                displacementMomentumEvaluator = new DisplacementMomentumEvaluator();
                oteZoneEvaluator = new OteZoneEvaluator();
                ictTargetQualityEvaluator = new IctTargetQualityEvaluator();
                valueRoadmapEvaluator = new ValueRoadmapEvaluator();
                valueAcceptanceEvaluator = new ValueAcceptanceEvaluator();
                adaptiveTargetPlanner = new AdaptiveTargetPlanner();
                lastDiagnosticsClosedOutcomes = 0;
                StartReplayValidationSession();
                PrintStartupSummaryOnce();
            }
            else if (State == State.Historical || State == State.Realtime)
            {
                PrintStartupSummaryOnce();
            }
            else if (State == State.Terminated)
            {
                PrintReplayValidationFinalSummary();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
            {
                return;
            }

            ContextFeatureSnapshot contextSnapshot = BuildContextSnapshot();
            SessionStructureSnapshot sessionStructureSnapshot = BuildSessionStructureSnapshot();
            LongSetupCandidateSnapshot candidateSnapshot = BuildLongSetupCandidateSnapshot(
                contextSnapshot,
                sessionStructureSnapshot);
            OrderFlowFeatureSnapshot snapshot = BuildApproxOrderFlowFeatureSnapshot();
            LongDecisionResult decision = EvaluateSnapshot(snapshot);
            OrderFlowConfirmationSnapshot confirmationSnapshot = BuildOrderFlowConfirmationSnapshot(
                candidateSnapshot,
                contextSnapshot,
                sessionStructureSnapshot,
                snapshot);
            HigherTimeframeBiasSnapshot htfBiasSnapshot = BuildHigherTimeframeBiasSnapshot(
                contextSnapshot,
                sessionStructureSnapshot);
            LiquiditySweepSnapshot liquiditySweepSnapshot = BuildLiquiditySweepSnapshot(sessionStructureSnapshot);
            DisplacementMomentumSnapshot displacementMomentumSnapshot = BuildDisplacementMomentumSnapshot(snapshot);
            MarketPhaseSnapshot marketPhaseSnapshot = BuildMarketPhaseSnapshot(
                sessionStructureSnapshot,
                liquiditySweepSnapshot,
                displacementMomentumSnapshot);
            FairValueGapSnapshot fairValueGapSnapshot = BuildFairValueGapSnapshot(
                liquiditySweepSnapshot,
                displacementMomentumSnapshot);
            OteZoneSnapshot oteZoneSnapshot = BuildOteZoneSnapshot(
                candidateSnapshot,
                liquiditySweepSnapshot,
                displacementMomentumSnapshot);
            IctTargetQualitySnapshot targetQualitySnapshot = BuildIctTargetQualitySnapshot(
                candidateSnapshot,
                sessionStructureSnapshot,
                contextSnapshot);
            ValueRoadmapSnapshot valueRoadmapSnapshot = BuildValueRoadmapSnapshot(
                sessionStructureSnapshot,
                contextSnapshot);
            ValueAcceptanceSnapshot valueAcceptanceSnapshot = BuildValueAcceptanceSnapshot(valueRoadmapSnapshot);
            OriginalStrategySetupType originalSetupType = MapOriginalStrategySetup(
                candidateSnapshot,
                valueRoadmapSnapshot,
                valueAcceptanceSnapshot,
                contextSnapshot,
                sessionStructureSnapshot);
            AdaptiveTargetPlan adaptiveTargetPlan = BuildAdaptiveTargetPlan(
                candidateSnapshot,
                valueRoadmapSnapshot);
            StrategyFilterResult filterResult = ApplyStrategyFilter(
                candidateSnapshot,
                confirmationSnapshot,
                contextSnapshot,
                sessionStructureSnapshot,
                snapshot,
                htfBiasSnapshot,
                marketPhaseSnapshot,
                liquiditySweepSnapshot,
                fairValueGapSnapshot,
                displacementMomentumSnapshot,
                oteZoneSnapshot,
                targetQualitySnapshot,
                valueRoadmapSnapshot,
                valueAcceptanceSnapshot,
                originalSetupType,
                adaptiveTargetPlan);
            MaybePrintFilterSummary();
            journaledThisBar = filterResult.IsAllowed && MaybeWriteSignalObservation(
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                snapshot,
                confirmationSnapshot,
                decision);
            MaybeTrackHypotheticalOutcome(
                journaledThisBar,
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                snapshot,
                confirmationSnapshot,
                decision);
            List<HypotheticalSignalOutcome> closedOutcomes = UpdateHypotheticalOutcomes();
            UpdatePerformanceSummary(closedOutcomes);
            UpdateStrategyDiagnostics(closedOutcomes);
            RecordReplayValidationBar(journaledThisBar, candidateSnapshot, confirmationSnapshot);
            MaybePrintReplayValidationProgress();
            PrintRuntimeState(
                snapshot,
                decision,
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                snapshot,
                confirmationSnapshot,
                journaledThisBar);
            UpdateRecentCandidateState(contextSnapshot, sessionStructureSnapshot);

            if (!NinjaTraderSafetyGuards.IsExecutionAllowed(
                EvaluationOnlyMode, UseSimOnly, AllowLiveTrading))
            {
                return;
            }
        }

        public LongDecisionResult EvaluateSnapshot(OrderFlowFeatureSnapshot snapshot)
        {
            if (evaluator == null)
            {
                evaluator = new LongOnlyOrderFlowEvaluator();
            }

            return evaluator.Evaluate(snapshot, MinRewardRisk, IsSetupEnabled);
        }

        private OrderFlowFeatureSnapshot BuildEvaluationSnapshot()
        {
            return new OrderFlowFeatureSnapshot
            {
                Symbol = Instrument == null ? string.Empty : Instrument.FullName,
                Timestamp = Time[0],
                CurrentPrice = Close[0],
                IsComplete = false,
                HasCandidateSetup = false,
                SourceReason = "NT-2A order-flow confirmation wiring is not implemented."
            };
        }

        private OrderFlowFeatureSnapshot BuildApproxOrderFlowFeatureSnapshot()
        {
            if (orderFlowFeatureEvaluator == null)
            {
                orderFlowFeatureEvaluator = new OrderFlowFeatureEvaluator();
            }

            if (!EnableOrderFlowFeatureLayer)
            {
                latestOrderFlowFeatureSnapshot = BuildEvaluationSnapshot();
                latestOrderFlowFeatureSnapshot.HasApproxOrderFlow = false;
                latestOrderFlowFeatureSnapshot.UsesApproximation = false;
                latestOrderFlowFeatureSnapshot.Source = "Order-flow feature layer disabled";
                latestOrderFlowFeatureSnapshot.Reason = "Order-flow feature layer is disabled.";
                return latestOrderFlowFeatureSnapshot;
            }

            if (!UseApproximateOrderFlow)
            {
                latestOrderFlowFeatureSnapshot = BuildEvaluationSnapshot();
                latestOrderFlowFeatureSnapshot.HasApproxOrderFlow = false;
                latestOrderFlowFeatureSnapshot.UsesApproximation = false;
                latestOrderFlowFeatureSnapshot.Source = "Approximate order-flow disabled";
                latestOrderFlowFeatureSnapshot.Reason = "True bid/ask volumetric data not wired yet.";
                return latestOrderFlowFeatureSnapshot;
            }

            double priorCumulativeDelta = cumulativeDelta;
            double priorDeltaMovingAverage = AverageQueue(recentDeltaValues);
            double priorVolumeMovingAverage = AverageQueue(recentVolumeValues);
            latestOrderFlowFeatureSnapshot = orderFlowFeatureEvaluator.EvaluateApproximate(
                Instrument == null ? string.Empty : Instrument.FullName,
                Time[0],
                Open[0],
                High[0],
                Low[0],
                Close[0],
                Volume[0],
                PreviousClosePrice,
                priorCumulativeDelta,
                priorDeltaMovingAverage,
                priorVolumeMovingAverage,
                HighVolumeMultiplier);

            cumulativeDelta = latestOrderFlowFeatureSnapshot.CumulativeDelta;
            PushRollingValue(recentDeltaValues, latestOrderFlowFeatureSnapshot.BarDelta, DeltaMovingAveragePeriod);
            PushRollingValue(recentVolumeValues, latestOrderFlowFeatureSnapshot.BarVolume, VolumeMovingAveragePeriod);
            latestOrderFlowFeatureSnapshot.DeltaMovingAverage = AverageQueue(recentDeltaValues);
            latestOrderFlowFeatureSnapshot.VolumeMovingAverage = AverageQueue(recentVolumeValues);
            latestOrderFlowFeatureSnapshot.SourceReason = "NT-3A order-flow feature layer is approximate; confirmation engine not implemented.";
            latestOrderFlowFeatureSnapshot.IsComplete = false;
            latestOrderFlowFeatureSnapshot.HasCandidateSetup = false;
            latestOrderFlowFeatureSnapshot.LongConfirmationPresent = false;
            return latestOrderFlowFeatureSnapshot;
        }

        private OrderFlowConfirmationSnapshot BuildOrderFlowConfirmationSnapshot(
            LongSetupCandidateSnapshot candidateSnapshot,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot)
        {
            if (orderFlowConfirmationEvaluator == null)
            {
                orderFlowConfirmationEvaluator = new OrderFlowConfirmationEvaluator();
            }

            if (!EnableOrderFlowConfirmationEngine)
            {
                latestOrderFlowConfirmationSnapshot = new OrderFlowConfirmationSnapshot
                {
                    ConfirmationState = OrderFlowConfirmationState.Disabled,
                    ConfirmationType = OrderFlowConfirmationType.None,
                    HasConfirmation = false,
                    RequiresExecutionDisabled = true,
                    UsesApproximateOrderFlow = true,
                    Reason = "Order-flow confirmation engine is disabled."
                };
                return latestOrderFlowConfirmationSnapshot;
            }

            latestOrderFlowConfirmationSnapshot = orderFlowConfirmationEvaluator.Evaluate(
                candidateSnapshot,
                contextSnapshot,
                sessionStructureSnapshot,
                orderFlowSnapshot,
                MinConfirmationScore,
                WeakConfirmationScore);
            return latestOrderFlowConfirmationSnapshot;
        }

        private HigherTimeframeBiasSnapshot BuildHigherTimeframeBiasSnapshot(
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot)
        {
            if (higherTimeframeBiasEvaluator == null)
            {
                higherTimeframeBiasEvaluator = new HigherTimeframeBiasEvaluator();
            }

            double fast = AverageClose(Math.Max(1, HtfFastPeriod), 0);
            double slow = AverageClose(Math.Max(1, HtfSlowPeriod), 0);
            double previousSlow = AverageClose(Math.Max(1, HtfSlowPeriod), 1);
            latestHigherTimeframeBiasSnapshot = higherTimeframeBiasEvaluator.Evaluate(
                Close[0],
                fast,
                slow,
                previousSlow,
                contextSnapshot,
                sessionStructureSnapshot,
                EnableHigherTimeframeBiasFilter && RequireHtfBiasForLongs,
                AllowLongsWhenHtfBalanced,
                RejectLongsWhenStrongBearish);
            return latestHigherTimeframeBiasSnapshot;
        }

        private MarketPhaseSnapshot BuildMarketPhaseSnapshot(
            SessionStructureSnapshot sessionStructureSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot)
        {
            if (amdMarketPhaseEvaluator == null)
            {
                amdMarketPhaseEvaluator = new AmdMarketPhaseEvaluator();
            }

            latestMarketPhaseSnapshot = amdMarketPhaseEvaluator.Evaluate(
                HighestHigh(Math.Max(1, AccumulationLookbackBars)),
                LowestLow(Math.Max(1, AccumulationLookbackBars)),
                Close[0],
                approximateSessionVwap,
                approximatePoc,
                TickSize(),
                MaxAccumulationRangeTicks,
                liquiditySweepSnapshot,
                displacementMomentumSnapshot,
                EnableAmdPhaseFilter && RequireAccumulationBeforeManipulation,
                EnableAmdPhaseFilter && RequireDistributionAfterManipulation);
            return latestMarketPhaseSnapshot;
        }

        private LiquiditySweepSnapshot BuildLiquiditySweepSnapshot(SessionStructureSnapshot sessionStructureSnapshot)
        {
            if (liquiditySweepEvaluator == null)
            {
                liquiditySweepEvaluator = new LiquiditySweepEvaluator();
            }

            latestLiquiditySweepSnapshot = liquiditySweepEvaluator.Evaluate(
                Low[0],
                Close[0],
                LowestLow(Math.Max(1, SweepLookbackBars)),
                LowestLow(Math.Max(1, AccumulationLookbackBars)),
                sessionStructureSnapshot == null ? 0 : sessionStructureSnapshot.ApproxVal,
                sessionStructureSnapshot == null ? 0 : sessionStructureSnapshot.PriorSessionLow,
                TickSize(),
                SweepBufferTicks,
                EnableLiquiditySweepFilter && RequireReclaimAfterSweep,
                AllowVALSweepAsLiquiditySweep,
                AllowRangeLowSweepAsLiquiditySweep,
                AllowSwingLowSweepAsLiquiditySweep,
                AllowPriorLowSweepAsLiquiditySweep);
            return latestLiquiditySweepSnapshot;
        }

        private FairValueGapSnapshot BuildFairValueGapSnapshot(
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot)
        {
            if (fairValueGapEvaluator == null)
            {
                fairValueGapEvaluator = new FairValueGapEvaluator();
            }

            latestFairValueGapSnapshot = fairValueGapEvaluator.Evaluate(
                CurrentBar >= 2 ? High[2] : 0,
                Low[0],
                Low[0],
                TickSize(),
                MinFvgSizeTicks,
                displacementMomentumSnapshot,
                liquiditySweepSnapshot,
                EnableFairValueGapFilter && RequireFvgAfterSweep,
                TrackFvgRetest);
            return latestFairValueGapSnapshot;
        }

        private DisplacementMomentumSnapshot BuildDisplacementMomentumSnapshot(OrderFlowFeatureSnapshot orderFlowSnapshot)
        {
            if (displacementMomentumEvaluator == null)
            {
                displacementMomentumEvaluator = new DisplacementMomentumEvaluator();
            }

            latestDisplacementMomentumSnapshot = displacementMomentumEvaluator.Evaluate(
                Open[0],
                High[0],
                Low[0],
                Close[0],
                orderFlowSnapshot == null ? 0 : orderFlowSnapshot.VolumeRatio,
                orderFlowSnapshot == null ? 0 : orderFlowSnapshot.BarDelta,
                TickSize(),
                MinDisplacementBodyTicks,
                MinBodyToRangeRatio,
                RequireCloseNearHighForDisplacement,
                CloseNearHighPercent,
                EnableDisplacementFilter && RequirePositiveDeltaForDisplacement);
            return latestDisplacementMomentumSnapshot;
        }

        private OteZoneSnapshot BuildOteZoneSnapshot(
            LongSetupCandidateSnapshot candidateSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot)
        {
            if (oteZoneEvaluator == null)
            {
                oteZoneEvaluator = new OteZoneEvaluator();
            }

            double rangeLow = liquiditySweepSnapshot == null || liquiditySweepSnapshot.SweptLevel <= 0
                ? LowestLow(Math.Max(1, ManipulationLookbackBars))
                : liquiditySweepSnapshot.SweptLevel;
            double rangeHigh = HighestHigh(Math.Max(1, ManipulationLookbackBars));
            double entry = candidateSnapshot == null ? Close[0] : candidateSnapshot.CandidateEntryPrice;
            latestOteZoneSnapshot = oteZoneEvaluator.Evaluate(
                entry,
                rangeLow,
                rangeHigh,
                OteLowerLevel,
                OteUpperLevel,
                AllowDiscountButOutsideOte,
                RejectPremiumLongEntries);
            return latestOteZoneSnapshot;
        }

        private IctTargetQualitySnapshot BuildIctTargetQualitySnapshot(
            LongSetupCandidateSnapshot candidateSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            ContextFeatureSnapshot contextSnapshot)
        {
            if (ictTargetQualityEvaluator == null)
            {
                ictTargetQualityEvaluator = new IctTargetQualityEvaluator();
            }

            double entry = candidateSnapshot == null ? Close[0] : candidateSnapshot.CandidateEntryPrice;
            double stop = candidateSnapshot == null ? Low[0] : candidateSnapshot.CandidateStopPrice;
            double target = candidateSnapshot == null ? High[0] : candidateSnapshot.CandidateTargetPrice;
            latestIctTargetQualitySnapshot = ictTargetQualityEvaluator.Evaluate(
                entry,
                stop,
                target,
                HighestHigh(Math.Max(1, SweepLookbackBars)),
                sessionStructureSnapshot == null ? sessionHigh : sessionStructureSnapshot.SessionHigh,
                sessionStructureSnapshot == null ? approximateVah : sessionStructureSnapshot.ApproxVah,
                contextSnapshot == null ? 0 : contextSnapshot.UpperVwapBand,
                TickSize(),
                MinimumTargetRewardRisk,
                PreferredTargetRewardRisk,
                MinimumTargetRoomTicks,
                PreferExternalLiquidityTargets,
                UseSwingHighAsBuySideLiquidity,
                UseSessionHighAsBuySideLiquidity,
                UseVAHAsTarget,
                UseUpperVwapBandAsTarget);
            return latestIctTargetQualitySnapshot;
        }

        private ValueRoadmapSnapshot BuildValueRoadmapSnapshot(
            SessionStructureSnapshot sessionStructureSnapshot,
            ContextFeatureSnapshot contextSnapshot)
        {
            if (valueRoadmapEvaluator == null)
            {
                valueRoadmapEvaluator = new ValueRoadmapEvaluator();
            }

            double vwap = contextSnapshot == null ? approximateSessionVwap : contextSnapshot.Vwap;
            double upperBand = contextSnapshot == null ? 0 : contextSnapshot.UpperVwapBand;
            double lowerBand = contextSnapshot == null ? 0 : contextSnapshot.LowerVwapBand;
            latestValueRoadmapSnapshot = valueRoadmapEvaluator.Evaluate(
                Close[0],
                vwap,
                sessionStructureSnapshot == null ? approximateVah : sessionStructureSnapshot.ApproxVah,
                sessionStructureSnapshot == null ? approximateVal : sessionStructureSnapshot.ApproxVal,
                sessionStructureSnapshot == null ? approximatePoc : sessionStructureSnapshot.ApproxPoc,
                vwap + ((upperBand - vwap) * 0.75),
                upperBand,
                vwap - ((vwap - lowerBand) * 0.75),
                lowerBand,
                sessionStructureSnapshot == null ? approximateVah : Math.Max(sessionStructureSnapshot.ApproxVah, sessionStructureSnapshot.PriorSessionHigh),
                sessionStructureSnapshot == null ? approximateVal : Math.Min(sessionStructureSnapshot.ApproxVal, sessionStructureSnapshot.PriorSessionLow),
                TickSize(),
                NearValueEdgeTicks);
            return latestValueRoadmapSnapshot;
        }

        private ValueAcceptanceSnapshot BuildValueAcceptanceSnapshot(ValueRoadmapSnapshot valueRoadmapSnapshot)
        {
            if (valueAcceptanceEvaluator == null)
            {
                valueAcceptanceEvaluator = new ValueAcceptanceEvaluator();
            }

            latestValueAcceptanceSnapshot = valueAcceptanceEvaluator.Evaluate(
                Close[0],
                High[0],
                Low[0],
                valueRoadmapSnapshot,
                AcceptanceBarsRequired,
                RejectionBarsRequired,
                TickSize(),
                NearValueEdgeTicks);
            return latestValueAcceptanceSnapshot;
        }

        private OriginalStrategySetupType MapOriginalStrategySetup(
            LongSetupCandidateSnapshot candidateSnapshot,
            ValueRoadmapSnapshot roadmapSnapshot,
            ValueAcceptanceSnapshot acceptanceSnapshot,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot)
        {
            latestOriginalStrategySetupType = OriginalStrategySetupType.None;
            if (candidateSnapshot == null || !candidateSnapshot.HasCandidate || roadmapSnapshot == null || acceptanceSnapshot == null)
            {
                return latestOriginalStrategySetupType;
            }

            if (candidateSnapshot.CandidateSetupType == LongSetupType.BreakoutPullbackLong
                && roadmapSnapshot.PrimaryTradeIdea == "BreakoutPullbackFromValue"
                && acceptanceSnapshot.AcceptanceState == ValueAcceptanceState.AcceptedAboveValue)
            {
                latestOriginalStrategySetupType = OriginalStrategySetupType.BreakoutPullbackFromValue;
            }
            else if (candidateSnapshot.CandidateSetupType == LongSetupType.PullbackContinuationLong
                && roadmapSnapshot.PrimaryTradeIdea == "ValueContinuation"
                && acceptanceSnapshot.HasAcceptance)
            {
                latestOriginalStrategySetupType = OriginalStrategySetupType.ValueContinuation;
            }
            else if (candidateSnapshot.CandidateSetupType == LongSetupType.DeviationRejectionLong
                && roadmapSnapshot.IsRotationalDayCandidate
                && acceptanceSnapshot.AcceptanceState == ValueAcceptanceState.RejectedBelowValue)
            {
                latestOriginalStrategySetupType = OriginalStrategySetupType.RotationalReversalFromDeviation;
            }
            else if ((candidateSnapshot.CandidateSetupType == LongSetupType.ValueReclaimLong
                    || candidateSnapshot.CandidateSetupType == LongSetupType.FailedBreakdownLong)
                && roadmapSnapshot.PrimaryTradeIdea == "ReturnPullbackToValue"
                && acceptanceSnapshot.IsLongSupportive)
            {
                latestOriginalStrategySetupType = OriginalStrategySetupType.ReturnPullbackToValue;
            }
            else
            {
                latestOriginalStrategySetupType = OriginalStrategySetupType.Invalid;
            }

            return latestOriginalStrategySetupType;
        }

        private AdaptiveTargetPlan BuildAdaptiveTargetPlan(
            LongSetupCandidateSnapshot candidateSnapshot,
            ValueRoadmapSnapshot roadmapSnapshot)
        {
            if (adaptiveTargetPlanner == null)
            {
                adaptiveTargetPlanner = new AdaptiveTargetPlanner();
            }

            double entry = candidateSnapshot == null ? Close[0] : candidateSnapshot.CandidateEntryPrice;
            double stop = candidateSnapshot == null ? Low[0] : candidateSnapshot.CandidateStopPrice;
            latestAdaptiveTargetPlan = adaptiveTargetPlanner.Build(
                entry,
                stop,
                roadmapSnapshot,
                MinimumLogicalTargetRoomTicks,
                TickSize());
            return latestAdaptiveTargetPlan;
        }

        private StrategyFilterResult ApplyStrategyFilter(
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            HigherTimeframeBiasSnapshot htfBiasSnapshot,
            MarketPhaseSnapshot marketPhaseSnapshot,
            LiquiditySweepSnapshot liquiditySweepSnapshot,
            FairValueGapSnapshot fairValueGapSnapshot,
            DisplacementMomentumSnapshot displacementMomentumSnapshot,
            OteZoneSnapshot oteZoneSnapshot,
            IctTargetQualitySnapshot targetQualitySnapshot,
            ValueRoadmapSnapshot valueRoadmapSnapshot,
            ValueAcceptanceSnapshot valueAcceptanceSnapshot,
            OriginalStrategySetupType originalSetupType,
            AdaptiveTargetPlan adaptiveTargetPlan)
        {
            if (strategyFilterEngine == null)
            {
                strategyFilterEngine = new StrategyFilterEngine();
            }

            StrategyFilterResult result = strategyFilterEngine.Evaluate(
                EnableStrategyFilterLayer,
                StrategyFilterProfile,
                candidateSnapshot,
                confirmationSnapshot,
                contextSnapshot,
                sessionStructureSnapshot,
                orderFlowSnapshot,
                V2AllowBreakoutPullbackLong,
                V2AllowFailedBreakdownLong,
                V2AllowValueReclaimLong,
                V2AllowDeviationRejectionLong,
                V2AllowPullbackContinuationLong,
                V2MinimumConfirmationScore,
                V2MinimumRewardRisk,
                V2RequireConfirmationObserved,
                V2RequireBuyerPressure,
                V2RejectStrongSellerPressure,
                V2RejectNoConfirmation,
                V2RejectWeakConfirmation,
                V2RejectBreakoutAboveUpperDeviation,
                V2AllowBreakoutOnlyNearOrAboveVAH,
                V2AllowFailedBreakdownOnlyBelowOrNearVAL,
                V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout,
                V2RejectInsideValueBreakoutChase,
                htfBiasSnapshot,
                marketPhaseSnapshot,
                liquiditySweepSnapshot,
                fairValueGapSnapshot,
                displacementMomentumSnapshot,
                oteZoneSnapshot,
                targetQualitySnapshot,
                MinimumTargetRewardRisk,
                PreferredTargetRewardRisk,
                valueRoadmapSnapshot,
                valueAcceptanceSnapshot,
                originalSetupType,
                adaptiveTargetPlan,
                EnableOriginalStrategyAlignment,
                RequireRthSessionOnly,
                IsRthSession(),
                RequireClearValueRoadmap,
                RequireValueAcceptance,
                RequireOriginalSetupType,
                RequireLogicalValueTarget,
                MinimumLogicalTargetRoomTicks,
                MinOriginalConfirmationScore);

            if (PrintFilteredCandidates && result.IsFiltered)
            {
                // Emits FILTERED_CANDIDATE= through the observation-only filter engine.
                if (StrategyFilterProfile == StrategyFilterProfile.OriginalValueRoadmapV1)
                {
                    // Emits ORIGINAL_STRATEGY_FILTERED= for original value-roadmap rejections.
                    strategyFilterEngine.PrintOriginalStrategyFiltered(
                        result,
                        originalSetupType,
                        valueRoadmapSnapshot,
                        valueAcceptanceSnapshot,
                        adaptiveTargetPlan,
                        Print);
                }
                else if (StrategyFilterProfile == StrategyFilterProfile.IctAmdLiquidityV1)
                {
                    strategyFilterEngine.PrintIctFilteredCandidate(
                        result,
                        htfBiasSnapshot,
                        marketPhaseSnapshot,
                        liquiditySweepSnapshot,
                        fairValueGapSnapshot,
                        displacementMomentumSnapshot,
                        oteZoneSnapshot,
                        targetQualitySnapshot,
                        Print);
                }
                else
                {
                    strategyFilterEngine.PrintFilteredCandidate(result, Print);
                }
            }

            if (StrategyFilterProfile == StrategyFilterProfile.OriginalValueRoadmapV1
                && PrintOriginalStrategyEvents)
            {
                // Emits ADAPTIVE_TARGET_PLAN= when a logical value/VWAP/CVA target plan exists.
                strategyFilterEngine.PrintAdaptiveTargetPlan(adaptiveTargetPlan, Print);
                if (result.IsAllowed)
                {
                    // Emits ORIGINAL_STRATEGY_CANDIDATE= for allowed original roadmap candidates.
                    strategyFilterEngine.PrintOriginalStrategyCandidate(
                        result,
                        originalSetupType,
                        valueRoadmapSnapshot,
                        valueAcceptanceSnapshot,
                        adaptiveTargetPlan,
                        Print);
                }
            }

            if (result.IsAllowed && StrategyFilterProfile == StrategyFilterProfile.IctAmdLiquidityV1)
            {
                // Emits QUALITY_GATE_PASSED= when an ICT-filtered candidate reaches the observation pipeline.
                strategyFilterEngine.PrintQualityGatePassed(
                    result,
                    htfBiasSnapshot,
                    marketPhaseSnapshot,
                    liquiditySweepSnapshot,
                    fairValueGapSnapshot,
                    displacementMomentumSnapshot,
                    oteZoneSnapshot,
                    targetQualitySnapshot,
                    Print);
            }

            return result;
        }

        private void MaybePrintFilterSummary()
        {
            if (!EnableStrategyFilterLayer || strategyFilterEngine == null)
            {
                return;
            }

            int everyBars = Math.Max(1, PrintFilterSummaryEveryBars);
            if (CurrentBar <= 0 || CurrentBar % everyBars != 0)
            {
                return;
            }

            // Emits FILTER_SUMMARY= through the observation-only filter engine.
            strategyFilterEngine.PrintFilterSummary(StrategyFilterProfile, Print);
        }

        private bool MaybeWriteSignalObservation(
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            LongDecisionResult decision)
        {
            if (!EnableSignalObservationJournal)
            {
                return false;
            }

            bool hasCandidate = candidateSnapshot != null && candidateSnapshot.HasCandidate;
            if (JournalOnlyWhenCandidateExists && !hasCandidate)
            {
                return false;
            }

            if (JournalOnlyConfirmedCandidates
                && (confirmationSnapshot == null
                    || confirmationSnapshot.ConfirmationState != OrderFlowConfirmationState.ConfirmationObserved))
            {
                return false;
            }

            if (confirmationSnapshot != null
                && confirmationSnapshot.ConfirmationScore < MinimumJournalConfirmationScore)
            {
                return false;
            }

            string signature = SignatureFor(candidateSnapshot, confirmationSnapshot);
            if (signature == lastJournalSignature
                && lastJournalBar >= 0
                && CurrentBar - lastJournalBar < Math.Max(1, JournalCooldownBars))
            {
                return false;
            }

            if (signalObservationJournalWriter == null)
            {
                signalObservationJournalWriter = new SignalObservationJournalWriter();
            }

            SignalObservationRecord record = BuildSignalObservationRecord(
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                orderFlowSnapshot,
                confirmationSnapshot,
                decision);

            bool wrote = signalObservationJournalWriter.Write(record, JournalFileName, Print);
            lastJournalSignature = signature;
            lastJournalBar = CurrentBar;

            if (PrintJournalEvents)
            {
                Print(Name
                    + " signal observation journal event: Journaled=True"
                    + " RecordId=" + record.RecordId
                    + " Candidate=" + record.CandidateSetupType
                    + " ConfirmationState=" + record.ConfirmationState);
            }

            return wrote;
        }

        private bool MaybeTrackHypotheticalOutcome(
            bool journaled,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            LongDecisionResult decision)
        {
            if (!EnableHypotheticalOutcomeTracking || !journaled)
            {
                return false;
            }

            if (hypotheticalOutcomeTracker == null)
            {
                hypotheticalOutcomeTracker = new HypotheticalOutcomeTracker();
            }

            SignalObservationRecord record = BuildSignalObservationRecord(
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                orderFlowSnapshot,
                confirmationSnapshot,
                decision);

            return hypotheticalOutcomeTracker.TryOpenFromObservation(
                record,
                candidateSnapshot,
                confirmationSnapshot,
                MaxBarsToTrackOutcome,
                TrackWeakConfirmations,
                PrintOutcomeEvents,
                JournalFileName,
                Print);
        }

        private List<HypotheticalSignalOutcome> UpdateHypotheticalOutcomes()
        {
            if (!EnableHypotheticalOutcomeTracking)
            {
                return new List<HypotheticalSignalOutcome>();
            }

            if (hypotheticalOutcomeTracker == null)
            {
                hypotheticalOutcomeTracker = new HypotheticalOutcomeTracker();
            }

            return hypotheticalOutcomeTracker.UpdateOpenOutcomes(
                High[0],
                Low[0],
                Close[0],
                CurrentBar,
                Time[0],
                ConservativeSameBarResolution,
                PrintOutcomeEvents,
                JournalFileName,
                Print);
        }

        private void UpdatePerformanceSummary(List<HypotheticalSignalOutcome> closedOutcomes)
        {
            if (!EnablePerformanceSummary || closedOutcomes == null || closedOutcomes.Count == 0)
            {
                return;
            }

            if (hypotheticalPerformanceTracker == null)
            {
                hypotheticalPerformanceTracker = new HypotheticalPerformanceTracker();
            }

            int openOutcomes = hypotheticalOutcomeTracker == null ? 0 : hypotheticalOutcomeTracker.OpenOutcomeCount;
            hypotheticalPerformanceTracker.RecordClosedOutcomes(
                closedOutcomes,
                openOutcomes,
                TimeoutResultR,
                InvalidatedResultR,
                DefaultTargetRewardR);

            if (PrintPerformanceSummary
                && hypotheticalPerformanceTracker.ShouldPrintSummary(PerformanceSummaryEveryClosedOutcomes))
            {
                hypotheticalPerformanceTracker.PrintSummary(PrintSetupBreakdown, Print);
            }
        }

        private void UpdateStrategyDiagnostics(List<HypotheticalSignalOutcome> closedOutcomes)
        {
            if (!EnableStrategyDiagnostics
                || !PrintStrategyDiagnostics
                || closedOutcomes == null
                || closedOutcomes.Count == 0)
            {
                return;
            }

            if (strategyDiagnosticsEngine == null)
            {
                strategyDiagnosticsEngine = new StrategyDiagnosticsEngine();
            }

            int totalClosed = CurrentPerformanceSummary().TotalClosedOutcomes;
            int everyClosedOutcomes = Math.Max(1, DiagnosticsEveryClosedOutcomes);
            if (totalClosed <= 0 || totalClosed - lastDiagnosticsClosedOutcomes < everyClosedOutcomes)
            {
                return;
            }

            // STRATEGY_DIAGNOSTICS= and SETUP_DIAGNOSTIC= are observation-only recommendations.
            strategyDiagnosticsEngine.PrintDiagnostics(
                CurrentPerformanceSummary(),
                CurrentSetupPerformanceStats(),
                CurrentReplaySessionId(),
                MinimumClosedOutcomesForDiagnostics,
                MinimumSetupOutcomesForDecision,
                MinimumAverageRForSim101,
                MinimumSetupAverageRToKeep,
                Print);
            latestStrategyDiagnosticSummary = strategyDiagnosticsEngine.LastSummary;
            lastDiagnosticsClosedOutcomes = totalClosed;
        }

        private void StartReplayValidationSession()
        {
            if (!EnableReplayValidation)
            {
                return;
            }

            if (replayValidationTracker == null)
            {
                replayValidationTracker = new ReplayValidationTracker();
            }

            replayValidationTracker.StartSession(
                ReplaySessionLabel,
                InstrumentName(),
                Name,
                DateTime.Now,
                State.ToString(),
                BarsPeriodName(),
                TimeframeName(),
                TradingHoursName(),
                EvaluationOnlyMode,
                true,
                UseApproximateOrderFlow,
                UseApproximateVolumeProfile,
                CurrentBar);
        }

        private void RecordReplayValidationBar(
            bool journaled,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot)
        {
            if (!EnableReplayValidation)
            {
                return;
            }

            if (replayValidationTracker == null)
            {
                StartReplayValidationSession();
            }

            replayValidationTracker.RecordBar(
                CurrentBar,
                Time[0],
                journaled,
                candidateSnapshot,
                confirmationSnapshot);
        }

        private void MaybePrintReplayValidationProgress()
        {
            if (!EnableReplayValidation || !PrintReplayValidationSummary)
            {
                return;
            }

            if (replayValidationTracker == null)
            {
                StartReplayValidationSession();
            }

            int everyBars = Math.Max(1, PrintReplayValidationEveryBars);
            if (replayValidationTracker.Session == null
                || replayValidationTracker.Session.TotalBarsProcessed <= 0
                || replayValidationTracker.Session.TotalBarsProcessed % everyBars != 0)
            {
                return;
            }

            // Emits REPLAY_VALIDATION_PROGRESS= structured Output for exportable review.
            replayValidationTracker.PrintProgress(
                CurrentPerformanceSummary(),
                CurrentOpenOutcomeCount(),
                Time[0],
                MinimumClosedOutcomesForReview,
                MinimumBarsForReview,
                Print);
        }

        private void PrintReplayValidationFinalSummary()
        {
            if (!EnableReplayValidation || !PrintReplayValidationSummary || replayValidationTracker == null)
            {
                return;
            }

            // Emits REPLAY_VALIDATION_SUMMARY= as a best-effort termination/disable summary.
            replayValidationTracker.PrintFinalSummary(
                CurrentPerformanceSummary(),
                CurrentOpenOutcomeCount(),
                DateTime.Now,
                MinimumClosedOutcomesForReview,
                MinimumBarsForReview,
                Print);
        }

        private HypotheticalPerformanceSummary CurrentPerformanceSummary()
        {
            if (hypotheticalPerformanceTracker == null)
            {
                return new HypotheticalPerformanceSummary();
            }

            return hypotheticalPerformanceTracker.Summary;
        }

        private int CurrentOpenOutcomeCount()
        {
            return hypotheticalOutcomeTracker == null ? 0 : hypotheticalOutcomeTracker.OpenOutcomeCount;
        }

        private bool ReplayValidationReviewable()
        {
            return EnableReplayValidation
                && replayValidationTracker != null
                && replayValidationTracker.IsReviewable(
                    CurrentPerformanceSummary(),
                    MinimumClosedOutcomesForReview,
                    MinimumBarsForReview);
        }

        private List<SetupPerformanceStats> CurrentSetupPerformanceStats()
        {
            if (hypotheticalPerformanceTracker == null)
            {
                return new List<SetupPerformanceStats>();
            }

            return hypotheticalPerformanceTracker.GetSetupStats();
        }

        private string CurrentReplaySessionId()
        {
            if (replayValidationTracker == null || replayValidationTracker.Session == null)
            {
                return string.Empty;
            }

            return replayValidationTracker.Session.SessionId;
        }

        private string CurrentDiagnosticGrade()
        {
            if (latestStrategyDiagnosticSummary == null)
            {
                return "NotRun";
            }

            return latestStrategyDiagnosticSummary.OverallGrade;
        }

        private bool CurrentSim101Eligible()
        {
            return latestStrategyDiagnosticSummary != null
                && latestStrategyDiagnosticSummary.IsEligibleForSim101;
        }

        private SignalObservationRecord BuildSignalObservationRecord(
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            LongDecisionResult decision)
        {
            return new SignalObservationRecord
            {
                RecordId = InstrumentName() + "-" + CurrentBar + "-" + Time[0].Ticks,
                Timestamp = Time[0],
                Instrument = InstrumentName(),
                CurrentBar = CurrentBar,
                Price = Close[0],
                ContextState = contextSnapshot == null ? string.Empty : contextSnapshot.ContextState.ToString(),
                LocationState = contextSnapshot == null ? string.Empty : contextSnapshot.LocationState.ToString(),
                Vwap = contextSnapshot == null ? 0 : contextSnapshot.Vwap,
                Poc = sessionStructureSnapshot == null ? 0 : sessionStructureSnapshot.ApproxPoc,
                Vah = sessionStructureSnapshot == null ? 0 : sessionStructureSnapshot.ApproxVah,
                Val = sessionStructureSnapshot == null ? 0 : sessionStructureSnapshot.ApproxVal,
                ValueState = sessionStructureSnapshot == null ? string.Empty : sessionStructureSnapshot.ValueState.ToString(),
                CandidateSetupType = CandidateName(candidateSnapshot),
                CandidateState = candidateSnapshot == null ? string.Empty : candidateSnapshot.CandidateState.ToString(),
                CandidateRewardRisk = candidateSnapshot == null ? 0 : candidateSnapshot.CandidateRewardRisk,
                CandidateReason = candidateSnapshot == null ? string.Empty : candidateSnapshot.Reason,
                OrderFlowBias = orderFlowSnapshot == null ? string.Empty : orderFlowSnapshot.OrderFlowBias.ToString(),
                BarDelta = orderFlowSnapshot == null ? 0 : orderFlowSnapshot.BarDelta,
                CumulativeDelta = orderFlowSnapshot == null ? 0 : orderFlowSnapshot.CumulativeDelta,
                IsHighVolumeBar = orderFlowSnapshot != null && orderFlowSnapshot.IsHighVolumeBar,
                ConfirmationType = confirmationSnapshot == null ? string.Empty : confirmationSnapshot.ConfirmationType.ToString(),
                ConfirmationState = confirmationSnapshot == null ? string.Empty : confirmationSnapshot.ConfirmationState.ToString(),
                ConfirmationScore = confirmationSnapshot == null ? 0 : confirmationSnapshot.ConfirmationScore,
                ConfirmationReason = confirmationSnapshot == null ? string.Empty : confirmationSnapshot.Reason,
                ExecutionDisabled = true,
                EvaluationOnlyMode = EvaluationOnlyMode,
                DecisionState = decision == null ? string.Empty : decision.DecisionStatus.ToString(),
                Notes = "NT-4A signal observation journal; observation-only and non-executable."
            };
        }

        private static string SignatureFor(
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot)
        {
            return CandidateName(candidateSnapshot)
                + "|"
                + (confirmationSnapshot == null ? "None" : confirmationSnapshot.ConfirmationState.ToString())
                + "|"
                + (confirmationSnapshot == null ? "None" : confirmationSnapshot.ConfirmationType.ToString());
        }

        private string InstrumentName()
        {
            return Instrument == null ? string.Empty : Instrument.FullName;
        }

        private bool IsRthSession()
        {
            int time = ToTime(Time[0]);
            return time >= 93000 && time <= 160000;
        }

        private string BarsPeriodName()
        {
            try
            {
                return BarsPeriod == null ? "Unknown" : BarsPeriod.BarsPeriodType.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        private string TimeframeName()
        {
            try
            {
                return BarsPeriod == null ? "Unknown" : BarsPeriod.Value + " " + BarsPeriod.BarsPeriodType;
            }
            catch
            {
                return "Unknown";
            }
        }

        private string TradingHoursName()
        {
            try
            {
                return Bars == null || Bars.TradingHours == null ? "Unknown" : Bars.TradingHours.Name;
            }
            catch
            {
                return "Unknown";
            }
        }

        private ContextFeatureSnapshot BuildContextSnapshot()
        {
            if (contextEvaluator == null)
            {
                contextEvaluator = new MarketContextEvaluator();
            }

            if (!EnableContextLayer)
            {
                latestContextSnapshot = new ContextFeatureSnapshot
                {
                    CurrentPrice = Close[0],
                    HasValidContext = false,
                    Reason = "Context layer is disabled."
                };
                return latestContextSnapshot;
            }

            UpdateSessionContext();

            double tickSize = TickSize();
            double bandPoints = Math.Max(1, VwapBandTicks) * tickSize;
            ContextFeatureSnapshot snapshot = new ContextFeatureSnapshot
            {
                CurrentPrice = Close[0],
                SessionHigh = sessionHigh,
                SessionLow = sessionLow,
                Vwap = approximateSessionVwap,
                UpperVwapBand = approximateSessionVwap > 0
                    ? approximateSessionVwap + bandPoints
                    : 0,
                LowerVwapBand = approximateSessionVwap > 0
                    ? approximateSessionVwap - bandPoints
                    : 0,
                Reason = UseApproximateSessionVwap
                    ? "Using NT-2A approximate internal session VWAP."
                    : "Approximate session VWAP is disabled."
            };

            latestContextSnapshot = contextEvaluator.Evaluate(
                snapshot,
                tickSize,
                NearVwapTicks);
            return latestContextSnapshot;
        }

        private SessionStructureSnapshot BuildSessionStructureSnapshot()
        {
            if (sessionStructureEvaluator == null)
            {
                sessionStructureEvaluator = new SessionStructureEvaluator();
            }

            if (!EnableValueStructureLayer)
            {
                latestSessionStructureSnapshot = new SessionStructureSnapshot
                {
                    CurrentPrice = Close[0],
                    HasValidValueStructure = false,
                    Reason = "Value structure layer is disabled."
                };
                return latestSessionStructureSnapshot;
            }

            UpdateSessionStructure();

            SessionStructureSnapshot snapshot = new SessionStructureSnapshot
            {
                CurrentPrice = Close[0],
                SessionHigh = sessionHigh,
                SessionLow = sessionLow,
                PriorSessionHigh = priorSessionHigh,
                PriorSessionLow = priorSessionLow,
                OpeningRangeHigh = openingRangeHigh,
                OpeningRangeLow = openingRangeLow,
                ApproxPoc = approximatePoc,
                ApproxVah = approximateVah,
                ApproxVal = approximateVal,
                Reason = UseApproximateVolumeProfile
                    ? "Using NT-2B approximate internal volume profile."
                    : "Approximate volume profile is disabled."
            };

            latestSessionStructureSnapshot = sessionStructureEvaluator.Evaluate(
                snapshot,
                TickSize(),
                NearValueTicks);
            return latestSessionStructureSnapshot;
        }

        private LongSetupCandidateSnapshot BuildLongSetupCandidateSnapshot(
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot)
        {
            if (setupCandidateEvaluator == null)
            {
                setupCandidateEvaluator = new LongSetupCandidateEvaluator();
            }

            if (!EnableSetupCandidateDetection)
            {
                latestLongSetupCandidateSnapshot = new LongSetupCandidateSnapshot
                {
                    CandidateState = LongSetupCandidateState.Disabled,
                    HasCandidate = false,
                    RequiresOrderFlowConfirmation = true,
                    Reason = "Setup candidate detection is disabled."
                };
                return latestLongSetupCandidateSnapshot;
            }

            latestLongSetupCandidateSnapshot = setupCandidateEvaluator.Evaluate(
                contextSnapshot,
                sessionStructureSnapshot,
                PreviousValueState,
                PreviousContextState,
                PreviousLocationState,
                PreviousClosePrice,
                BarsSinceBelowValue >= 0 && BarsSinceBelowValue <= CandidateLookbackBars
                    ? BarsSinceBelowValue
                    : -1,
                BarsSinceAboveValue >= 0 && BarsSinceAboveValue <= CandidateLookbackBars
                    ? BarsSinceAboveValue
                    : -1,
                MinCandidateRewardRisk,
                TickSize());

            return latestLongSetupCandidateSnapshot;
        }

        private void UpdateSessionContext()
        {
            if (Bars != null && Bars.IsFirstBarOfSession)
            {
                ResetForNewSession();
            }
            else
            {
                sessionHigh = sessionHigh <= 0 ? High[0] : Math.Max(sessionHigh, High[0]);
                sessionLow = sessionLow <= 0 ? Low[0] : Math.Min(sessionLow, Low[0]);
            }

            if (!UseApproximateSessionVwap)
            {
                approximateSessionVwap = 0;
                return;
            }

            double volume = Math.Max(0, Volume[0]);
            if (volume <= 0)
            {
                return;
            }

            cumulativeTypicalPriceVolume += TypicalPrice() * volume;
            cumulativeVolume += volume;
            if (cumulativeVolume > 0)
            {
                approximateSessionVwap = cumulativeTypicalPriceVolume / cumulativeVolume;
            }
        }

        private void UpdateSessionStructure()
        {
            if (Bars != null && Bars.IsFirstBarOfSession)
            {
                ResetForNewSession();
            }
            else
            {
                sessionHigh = sessionHigh <= 0 ? High[0] : Math.Max(sessionHigh, High[0]);
                sessionLow = sessionLow <= 0 ? Low[0] : Math.Min(sessionLow, Low[0]);
            }

            UpdateOpeningRange();
            UpdateApproximateVolumeProfile();
            CalculateApproximateValueArea();
        }

        private void ResetForNewSession()
        {
            if (lastSessionResetBar == CurrentBar)
            {
                return;
            }

            lastSessionResetBar = CurrentBar;

            if (sessionHigh > 0 && sessionLow > 0)
            {
                priorSessionHigh = sessionHigh;
                priorSessionLow = sessionLow;
            }

            sessionHigh = High[0];
            sessionLow = Low[0];
            openingRangeHigh = High[0];
            openingRangeLow = Low[0];
            sessionStartTime = Time[0];
            cumulativeTypicalPriceVolume = 0;
            cumulativeVolume = 0;
            approximateSessionVwap = 0;
            volumeByPrice.Clear();
            approximatePoc = 0;
            approximateVah = 0;
            approximateVal = 0;
            cumulativeDelta = 0;
            recentDeltaValues.Clear();
            recentVolumeValues.Clear();
        }

        private void UpdateOpeningRange()
        {
            if (sessionStartTime == DateTime.MinValue)
            {
                sessionStartTime = Time[0];
                openingRangeHigh = High[0];
                openingRangeLow = Low[0];
            }

            if (Time[0] <= sessionStartTime.AddMinutes(Math.Max(1, OpeningRangeMinutes)))
            {
                openingRangeHigh = openingRangeHigh <= 0
                    ? High[0]
                    : Math.Max(openingRangeHigh, High[0]);
                openingRangeLow = openingRangeLow <= 0
                    ? Low[0]
                    : Math.Min(openingRangeLow, Low[0]);
            }
        }

        private void UpdateApproximateVolumeProfile()
        {
            if (!UseApproximateVolumeProfile)
            {
                volumeByPrice.Clear();
                approximatePoc = 0;
                approximateVah = 0;
                approximateVal = 0;
                return;
            }

            double volume = Math.Max(0, Volume[0]);
            if (volume <= 0)
            {
                return;
            }

            double priceLevel = RoundToTick(TypicalPrice());
            if (!volumeByPrice.ContainsKey(priceLevel))
            {
                volumeByPrice[priceLevel] = 0;
            }

            volumeByPrice[priceLevel] += volume;
        }

        private void CalculateApproximateValueArea()
        {
            if (!UseApproximateVolumeProfile || volumeByPrice.Count == 0)
            {
                approximatePoc = 0;
                approximateVah = 0;
                approximateVal = 0;
                return;
            }

            double totalVolume = 0;
            double maxVolume = -1;
            double poc = 0;
            foreach (KeyValuePair<double, double> level in volumeByPrice)
            {
                totalVolume += level.Value;
                if (level.Value > maxVolume)
                {
                    maxVolume = level.Value;
                    poc = level.Key;
                }
            }

            if (totalVolume <= 0 || poc <= 0)
            {
                approximatePoc = 0;
                approximateVah = 0;
                approximateVal = 0;
                return;
            }

            double targetVolume = totalVolume * Math.Max(1, Math.Min(100, ValueAreaPercent)) / 100.0;
            List<double> sortedPrices = new List<double>(volumeByPrice.Keys);
            sortedPrices.Sort(delegate(double left, double right)
            {
                int distanceCompare = Math.Abs(left - poc).CompareTo(Math.Abs(right - poc));
                if (distanceCompare != 0)
                {
                    return distanceCompare;
                }

                return left.CompareTo(right);
            });

            double includedVolume = 0;
            double vah = poc;
            double val = poc;
            foreach (double price in sortedPrices)
            {
                includedVolume += volumeByPrice[price];
                vah = Math.Max(vah, price);
                val = Math.Min(val, price);
                if (includedVolume >= targetVolume)
                {
                    break;
                }
            }

            approximatePoc = poc;
            approximateVah = vah;
            approximateVal = val;
        }

        private double TypicalPrice()
        {
            return (High[0] + Low[0] + Close[0]) / 3.0;
        }

        private double TickSize()
        {
            if (Instrument != null
                && Instrument.MasterInstrument != null
                && Instrument.MasterInstrument.TickSize > 0)
            {
                return Instrument.MasterInstrument.TickSize;
            }

            return 1.0;
        }

        private double RoundToTick(double value)
        {
            double tickSize = TickSize();
            if (tickSize <= 0)
            {
                return value;
            }

            return Math.Round(value / tickSize, MidpointRounding.AwayFromZero) * tickSize;
        }

        private double AverageClose(int period, int barsAgoOffset)
        {
            int safePeriod = Math.Max(1, period);
            int start = Math.Max(0, barsAgoOffset);
            int available = Math.Max(0, CurrentBar - start + 1);
            int count = Math.Min(safePeriod, available);
            if (count <= 0)
            {
                return Close[0];
            }

            double total = 0;
            for (int index = start; index < start + count; index++)
            {
                total += Close[index];
            }

            return total / count;
        }

        private double HighestHigh(int lookbackBars)
        {
            int count = Math.Min(Math.Max(1, lookbackBars), CurrentBar + 1);
            double highest = High[0];
            for (int index = 0; index < count; index++)
            {
                highest = Math.Max(highest, High[index]);
            }

            return highest;
        }

        private double LowestLow(int lookbackBars)
        {
            int count = Math.Min(Math.Max(1, lookbackBars), CurrentBar + 1);
            double lowest = Low[0];
            for (int index = 0; index < count; index++)
            {
                lowest = Math.Min(lowest, Low[index]);
            }

            return lowest;
        }

        private static void PushRollingValue(
            Queue<double> values,
            double value,
            int period)
        {
            values.Enqueue(value);
            int safePeriod = Math.Max(1, period);
            while (values.Count > safePeriod)
            {
                values.Dequeue();
            }
        }

        private static double AverageQueue(Queue<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double total = 0;
            foreach (double value in values)
            {
                total += value;
            }

            return total / values.Count;
        }

        private void UpdateRecentCandidateState(
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot)
        {
            if (sessionStructureSnapshot != null
                && sessionStructureSnapshot.HasValidValueStructure)
            {
                if (sessionStructureSnapshot.ValueState == ValueAreaState.BelowValue)
                {
                    BarsSinceBelowValue = 0;
                }
                else if (BarsSinceBelowValue >= 0)
                {
                    BarsSinceBelowValue++;
                }

                if (sessionStructureSnapshot.ValueState == ValueAreaState.AboveValue)
                {
                    BarsSinceAboveValue = 0;
                }
                else if (BarsSinceAboveValue >= 0)
                {
                    BarsSinceAboveValue++;
                }
            }

            if (contextSnapshot != null)
            {
                PreviousContextState = contextSnapshot.ContextState;
                PreviousLocationState = contextSnapshot.LocationState;
            }

            if (sessionStructureSnapshot != null)
            {
                PreviousValueState = sessionStructureSnapshot.ValueState;
            }

            PreviousClosePrice = Close[0];
        }

        private bool IsSetupEnabled(LongSetupType setupType)
        {
            switch (setupType)
            {
                case LongSetupType.FailedBreakdownLong:
                    return EnableFailedBreakdownLong;
                case LongSetupType.PullbackContinuationLong:
                    return EnablePullbackContinuationLong;
                case LongSetupType.ValueReclaimLong:
                    return EnableValueReclaimLong;
                case LongSetupType.BreakoutPullbackLong:
                    return EnableBreakoutPullbackLong;
                case LongSetupType.DeviationRejectionLong:
                    return EnableDeviationRejectionLong;
                default:
                    return false;
            }
        }

        private void PrintStartupSummaryOnce()
        {
            if (!PrintDebug || !PrintStartupSummary || startupSummaryPrinted)
            {
                return;
            }

            startupSummaryPrinted = true;

            Print(Name + " startup summary:"
                + " EvaluationOnlyMode=" + EvaluationOnlyMode
                + " AllowLiveTrading=" + AllowLiveTrading
                + " UseSimOnly=" + UseSimOnly
                + " Quantity=" + Quantity
                + " MinRewardRisk=" + MinRewardRisk
                + " EnableContextLayer=" + EnableContextLayer
                + " UseApproximateSessionVwap=" + UseApproximateSessionVwap
                + " NearVwapTicks=" + NearVwapTicks
                + " VwapBandTicks=" + VwapBandTicks
                + " Context layer active=" + EnableContextLayer
                + " EnableValueStructureLayer=" + EnableValueStructureLayer
                + " UseApproximateVolumeProfile=" + UseApproximateVolumeProfile
                + " ValueAreaPercent=" + ValueAreaPercent
                + " NearValueTicks=" + NearValueTicks
                + " OpeningRangeMinutes=" + OpeningRangeMinutes
                + " Value structure layer active=" + EnableValueStructureLayer
                + " Approximate volume profile active=" + UseApproximateVolumeProfile
                + " EnableSetupCandidateDetection=" + EnableSetupCandidateDetection
                + " CandidateLookbackBars=" + CandidateLookbackBars
                + " MinCandidateRewardRisk=" + MinCandidateRewardRisk
                + " Setup candidate detection active=" + EnableSetupCandidateDetection
                + " EnableOrderFlowFeatureLayer=" + EnableOrderFlowFeatureLayer
                + " UseApproximateOrderFlow=" + UseApproximateOrderFlow
                + " DeltaMovingAveragePeriod=" + DeltaMovingAveragePeriod
                + " VolumeMovingAveragePeriod=" + VolumeMovingAveragePeriod
                + " HighVolumeMultiplier=" + HighVolumeMultiplier
                + " Order-flow feature layer active=" + EnableOrderFlowFeatureLayer
                + " Approximate order-flow active=" + UseApproximateOrderFlow
                + " True bid/ask volumetric data not wired yet"
                + " Confirmation engine not implemented"
                + " EnableOrderFlowConfirmationEngine=" + EnableOrderFlowConfirmationEngine
                + " MinConfirmationScore=" + MinConfirmationScore
                + " WeakConfirmationScore=" + WeakConfirmationScore
                + " RequireConfirmationBeforeSignal=" + RequireConfirmationBeforeSignal
                + " Confirmation engine active=" + EnableOrderFlowConfirmationEngine
                + " Approximate confirmation only"
                + " True volumetric confirmation not wired yet"
                + " EnableSignalObservationJournal=" + EnableSignalObservationJournal
                + " JournalOnlyConfirmedCandidates=" + JournalOnlyConfirmedCandidates
                + " JournalOnlyWhenCandidateExists=" + JournalOnlyWhenCandidateExists
                + " JournalFileName=" + JournalFileName
                + " JournalCooldownBars=" + JournalCooldownBars
                + " Signal journal active=" + EnableSignalObservationJournal
                + " EnableHypotheticalOutcomeTracking=" + EnableHypotheticalOutcomeTracking
                + " TrackWeakConfirmations=" + TrackWeakConfirmations
                + " MaxBarsToTrackOutcome=" + MaxBarsToTrackOutcome
                + " ConservativeSameBarResolution=" + ConservativeSameBarResolution
                + " PrintOutcomeEvents=" + PrintOutcomeEvents
                + " PrintOpenOutcomeCountEveryHeartbeat=" + PrintOpenOutcomeCountEveryHeartbeat
                + " Outcome tracking active=" + EnableHypotheticalOutcomeTracking
                + " EnablePerformanceSummary=" + EnablePerformanceSummary
                + " PerformanceSummaryEveryClosedOutcomes=" + PerformanceSummaryEveryClosedOutcomes
                + " PrintSetupBreakdown=" + PrintSetupBreakdown
                + " TimeoutResultR=" + TimeoutResultR
                + " InvalidatedResultR=" + InvalidatedResultR
                + " DefaultTargetRewardR=" + DefaultTargetRewardR
                + " Performance summary active=" + EnablePerformanceSummary
                + " EnableReplayValidation=" + EnableReplayValidation
                + " PrintReplayValidationSummary=" + PrintReplayValidationSummary
                + " PrintReplayValidationEveryBars=" + PrintReplayValidationEveryBars
                + " MinimumClosedOutcomesForReview=" + MinimumClosedOutcomesForReview
                + " MinimumBarsForReview=" + MinimumBarsForReview
                + " ReplaySessionLabel=" + ReplaySessionLabel
                + " Replay validation active=" + EnableReplayValidation
                + " EnableStrategyDiagnostics=" + EnableStrategyDiagnostics
                + " PrintStrategyDiagnostics=" + PrintStrategyDiagnostics
                + " DiagnosticsEveryClosedOutcomes=" + DiagnosticsEveryClosedOutcomes
                + " MinimumClosedOutcomesForDiagnostics=" + MinimumClosedOutcomesForDiagnostics
                + " MinimumSetupOutcomesForDecision=" + MinimumSetupOutcomesForDecision
                + " MinimumAverageRForSim101=" + MinimumAverageRForSim101
                + " MinimumSetupAverageRToKeep=" + MinimumSetupAverageRToKeep
                + " Strategy diagnostics active=" + EnableStrategyDiagnostics
                + " EnableStrategyFilterLayer=" + EnableStrategyFilterLayer
                + " StrategyFilterProfile=" + StrategyFilterProfile
                + " PrintFilteredCandidates=" + PrintFilteredCandidates
                + " PrintFilterSummaryEveryBars=" + PrintFilterSummaryEveryBars
                + " V2AllowBreakoutPullbackLong=" + V2AllowBreakoutPullbackLong
                + " V2AllowFailedBreakdownLong=" + V2AllowFailedBreakdownLong
                + " V2AllowValueReclaimLong=" + V2AllowValueReclaimLong
                + " V2AllowDeviationRejectionLong=" + V2AllowDeviationRejectionLong
                + " V2AllowPullbackContinuationLong=" + V2AllowPullbackContinuationLong
                + " V2MinimumConfirmationScore=" + V2MinimumConfirmationScore
                + " V2MinimumRewardRisk=" + V2MinimumRewardRisk
                + " V2RequireConfirmationObserved=" + V2RequireConfirmationObserved
                + " V2RequireBuyerPressure=" + V2RequireBuyerPressure
                + " V2RejectStrongSellerPressure=" + V2RejectStrongSellerPressure
                + " V2RejectNoConfirmation=" + V2RejectNoConfirmation
                + " V2RejectWeakConfirmation=" + V2RejectWeakConfirmation
                + " V2RejectBreakoutAboveUpperDeviation=" + V2RejectBreakoutAboveUpperDeviation
                + " V2AllowBreakoutOnlyNearOrAboveVAH=" + V2AllowBreakoutOnlyNearOrAboveVAH
                + " V2AllowFailedBreakdownOnlyBelowOrNearVAL=" + V2AllowFailedBreakdownOnlyBelowOrNearVAL
                + " V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout=" + V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout
                + " V2RejectInsideValueBreakoutChase=" + V2RejectInsideValueBreakoutChase
                + " Strategy filter layer active=" + EnableStrategyFilterLayer
                + " EnableHigherTimeframeBiasFilter=" + EnableHigherTimeframeBiasFilter
                + " HtfFastPeriod=" + HtfFastPeriod
                + " HtfSlowPeriod=" + HtfSlowPeriod
                + " RequireHtfBiasForLongs=" + RequireHtfBiasForLongs
                + " AllowLongsWhenHtfBalanced=" + AllowLongsWhenHtfBalanced
                + " RejectLongsWhenStrongBearish=" + RejectLongsWhenStrongBearish
                + " EnableAmdPhaseFilter=" + EnableAmdPhaseFilter
                + " RequireAccumulationBeforeManipulation=" + RequireAccumulationBeforeManipulation
                + " AccumulationLookbackBars=" + AccumulationLookbackBars
                + " MaxAccumulationRangeTicks=" + MaxAccumulationRangeTicks
                + " ManipulationLookbackBars=" + ManipulationLookbackBars
                + " MaxBarsFromManipulationToEntry=" + MaxBarsFromManipulationToEntry
                + " RequireDistributionAfterManipulation=" + RequireDistributionAfterManipulation
                + " EnableLiquiditySweepFilter=" + EnableLiquiditySweepFilter
                + " RequireSellSideSweepForLongs=" + RequireSellSideSweepForLongs
                + " SweepLookbackBars=" + SweepLookbackBars
                + " SweepBufferTicks=" + SweepBufferTicks
                + " MaxBarsAfterSweep=" + MaxBarsAfterSweep
                + " RequireReclaimAfterSweep=" + RequireReclaimAfterSweep
                + " EnableFairValueGapFilter=" + EnableFairValueGapFilter
                + " RequireBullishFvgForLongs=" + RequireBullishFvgForLongs
                + " RequireFvgAfterSweep=" + RequireFvgAfterSweep
                + " MinFvgSizeTicks=" + MinFvgSizeTicks
                + " EnableDisplacementFilter=" + EnableDisplacementFilter
                + " RequireBullishDisplacementForLongs=" + RequireBullishDisplacementForLongs
                + " MinDisplacementBodyTicks=" + MinDisplacementBodyTicks
                + " MinBodyToRangeRatio=" + MinBodyToRangeRatio
                + " EnableOteFilter=" + EnableOteFilter
                + " RequireOteForLongs=" + RequireOteForLongs
                + " OteLowerLevel=" + OteLowerLevel
                + " OteMidLevel=" + OteMidLevel
                + " OteUpperLevel=" + OteUpperLevel
                + " EnableIctTargetQualityFilter=" + EnableIctTargetQualityFilter
                + " MinimumTargetRewardRisk=" + MinimumTargetRewardRisk
                + " PreferredTargetRewardRisk=" + PreferredTargetRewardRisk
                + " MinimumTargetRoomTicks=" + MinimumTargetRoomTicks
                + " IctAmdLiquidityV1 available"
                + " EnableValueAcceptanceLayer=" + EnableValueAcceptanceLayer
                + " AcceptanceBarsRequired=" + AcceptanceBarsRequired
                + " RejectionBarsRequired=" + RejectionBarsRequired
                + " NearValueEdgeTicks=" + NearValueEdgeTicks
                + " EnableOriginalStrategyAlignment=" + EnableOriginalStrategyAlignment
                + " RequireRthSessionOnly=" + RequireRthSessionOnly
                + " RequireClearValueRoadmap=" + RequireClearValueRoadmap
                + " RequireValueAcceptance=" + RequireValueAcceptance
                + " RequireOriginalSetupType=" + RequireOriginalSetupType
                + " RequireLogicalValueTarget=" + RequireLogicalValueTarget
                + " MinimumLogicalTargetRoomTicks=" + MinimumLogicalTargetRoomTicks
                + " MinOriginalConfirmationScore=" + MinOriginalConfirmationScore
                + " PrintOriginalStrategyEvents=" + PrintOriginalStrategyEvents
                + " OriginalValueRoadmapV1 available"
                + " Evaluation only"
                + " Order-flow confirmation not implemented"
                + " Execution enabled: false"
                + " Long-only safety active"
                + " No orders will be submitted in this phase"
                + " Mode=EVALUATION_ONLY"
                + " NO_EXECUTION_ENABLED=true");
        }

        private void PrintRuntimeState(
            OrderFlowFeatureSnapshot snapshot,
            LongDecisionResult decision,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            bool journaled)
        {
            if (PrintEveryEvaluation)
            {
                PrintDecision(snapshot, decision);
                PrintContext(contextSnapshot);
                PrintValueStructure(sessionStructureSnapshot);
                PrintLongSetupCandidate(candidateSnapshot);
                PrintOrderFlowFeatures(orderFlowSnapshot);
                PrintOrderFlowConfirmation(confirmationSnapshot);
                return;
            }

            PrintHeartbeatIfDue(
                decision,
                contextSnapshot,
                sessionStructureSnapshot,
                candidateSnapshot,
                orderFlowSnapshot,
                confirmationSnapshot,
                journaled);
        }

        private void PrintDecision(
            OrderFlowFeatureSnapshot snapshot,
            LongDecisionResult decision)
        {
            if (!PrintDebug)
            {
                return;
            }

            Print(Name
                + " SignalState=" + decision.SignalState
                + " DecisionStatus=" + decision.DecisionStatus
                + " Reason=" + decision.Reason
                + " EntryPrice=" + FormatPrice(decision.EntryPrice)
                + " StopPrice=" + FormatPrice(decision.StopPrice)
                + " TargetPrice=" + FormatPrice(decision.TargetPrice));
        }

        private void PrintContext(ContextFeatureSnapshot contextSnapshot)
        {
            if (!PrintDebug || !PrintContextEveryHeartbeat || contextSnapshot == null)
            {
                return;
            }

            Print(Name
                + " Context="
                + contextSnapshot.ContextState
                + " Location=" + contextSnapshot.LocationState
                + " Price=" + FormatPrice(contextSnapshot.CurrentPrice)
                + " VWAP=" + FormatPrice(contextSnapshot.Vwap)
                + " Reason=" + contextSnapshot.Reason);
        }

        private void PrintValueStructure(SessionStructureSnapshot sessionStructureSnapshot)
        {
            if (!PrintDebug
                || !PrintValueStructureEveryHeartbeat
                || sessionStructureSnapshot == null)
            {
                return;
            }

            Print(Name
                + " POC=" + FormatPrice(sessionStructureSnapshot.ApproxPoc)
                + " VAH=" + FormatPrice(sessionStructureSnapshot.ApproxVah)
                + " VAL=" + FormatPrice(sessionStructureSnapshot.ApproxVal)
                + " ValueState=" + sessionStructureSnapshot.ValueState
                + " Reason=" + sessionStructureSnapshot.Reason);
        }

        private void PrintLongSetupCandidate(LongSetupCandidateSnapshot candidateSnapshot)
        {
            if (!PrintDebug || !PrintCandidateEveryHeartbeat || candidateSnapshot == null)
            {
                return;
            }

            Print(Name
                + " Candidate=" + CandidateName(candidateSnapshot)
                + " CandidateState=" + candidateSnapshot.CandidateState
                + " RR=" + FormatPrice(candidateSnapshot.CandidateRewardRisk)
                + " Reason=" + ShortReason(candidateSnapshot.Reason));
        }

        private void PrintOrderFlowFeatures(OrderFlowFeatureSnapshot orderFlowSnapshot)
        {
            if (!PrintDebug || !PrintOrderFlowEveryHeartbeat || orderFlowSnapshot == null)
            {
                return;
            }

            Print(Name
                + " OFBias=" + orderFlowSnapshot.OrderFlowBias
                + " OFPressure=" + orderFlowSnapshot.OrderFlowPressure
                + " Delta=" + FormatPrice(orderFlowSnapshot.BarDelta)
                + " CVD=" + FormatPrice(orderFlowSnapshot.CumulativeDelta)
                + " HighVol=" + orderFlowSnapshot.IsHighVolumeBar
                + " Reason=" + ShortReason(orderFlowSnapshot.Reason));
        }

        private void PrintOrderFlowConfirmation(OrderFlowConfirmationSnapshot confirmationSnapshot)
        {
            if (!PrintDebug || !PrintConfirmationEveryHeartbeat || confirmationSnapshot == null)
            {
                return;
            }

            Print(Name
                + " Confirmation=" + confirmationSnapshot.ConfirmationType
                + " ConfirmationState=" + confirmationSnapshot.ConfirmationState
                + " Score=" + FormatPrice(confirmationSnapshot.ConfirmationScore)
                + " Reason=" + ShortReason(confirmationSnapshot.Reason));
        }

        private void PrintHeartbeatIfDue(
            LongDecisionResult decision,
            ContextFeatureSnapshot contextSnapshot,
            SessionStructureSnapshot sessionStructureSnapshot,
            LongSetupCandidateSnapshot candidateSnapshot,
            OrderFlowFeatureSnapshot orderFlowSnapshot,
            OrderFlowConfirmationSnapshot confirmationSnapshot,
            bool journaled)
        {
            if (!PrintDebug || !PrintHeartbeat)
            {
                return;
            }

            int heartbeatBars = Math.Max(1, DebugHeartbeatBars);
            if (CurrentBar % heartbeatBars != 0)
            {
                return;
            }

            string contextOutput = string.Empty;
            if (PrintContextEveryHeartbeat && contextSnapshot != null)
            {
                contextOutput = " Price=" + FormatPrice(contextSnapshot.CurrentPrice)
                    + " VWAP=" + FormatPrice(contextSnapshot.Vwap)
                    + " Context=" + contextSnapshot.ContextState
                    + " Location=" + contextSnapshot.LocationState;
            }

            string valueOutput = string.Empty;
            if (PrintValueStructureEveryHeartbeat && sessionStructureSnapshot != null)
            {
                valueOutput = " POC=" + FormatPrice(sessionStructureSnapshot.ApproxPoc)
                    + " VAH=" + FormatPrice(sessionStructureSnapshot.ApproxVah)
                    + " VAL=" + FormatPrice(sessionStructureSnapshot.ApproxVal)
                    + " ValueState=" + sessionStructureSnapshot.ValueState;
            }

            string candidateOutput = string.Empty;
            if (PrintCandidateEveryHeartbeat && candidateSnapshot != null)
            {
                candidateOutput = " Candidate=" + CandidateName(candidateSnapshot)
                    + " CandidateState=" + candidateSnapshot.CandidateState
                    + " RR=" + FormatPrice(candidateSnapshot.CandidateRewardRisk)
                    + " CandidateReason=" + ShortReason(candidateSnapshot.Reason);
            }

            string orderFlowOutput = string.Empty;
            if (PrintOrderFlowEveryHeartbeat && orderFlowSnapshot != null)
            {
                orderFlowOutput = " OFBias=" + orderFlowSnapshot.OrderFlowBias
                    + " Delta=" + FormatPrice(orderFlowSnapshot.BarDelta)
                    + " CVD=" + FormatPrice(orderFlowSnapshot.CumulativeDelta)
                    + " HighVol=" + orderFlowSnapshot.IsHighVolumeBar;
            }

            string confirmationOutput = string.Empty;
            if (PrintConfirmationEveryHeartbeat && confirmationSnapshot != null)
            {
                confirmationOutput = " Confirmation=" + confirmationSnapshot.ConfirmationType
                    + " ConfirmationState=" + confirmationSnapshot.ConfirmationState
                    + " Score=" + FormatPrice(confirmationSnapshot.ConfirmationScore);
            }

            // Heartbeat marker example: Journal=Enabled Journaled=True
            string journalOutput = " Journal=" + (EnableSignalObservationJournal ? "Enabled" : "Disabled")
                + " Journaled=" + journaled;

            string outcomeOutput = string.Empty;
            if (PrintOpenOutcomeCountEveryHeartbeat && hypotheticalOutcomeTracker != null)
            {
                outcomeOutput = " OpenOutcomes=" + hypotheticalOutcomeTracker.OpenOutcomeCount
                    + " ClosedOutcomes=" + hypotheticalOutcomeTracker.ClosedOutcomeCount
                    + " LastOutcome=" + (string.IsNullOrEmpty(hypotheticalOutcomeTracker.LastOutcome)
                        ? "None"
                        : hypotheticalOutcomeTracker.LastOutcome);
            }

            string performanceOutput = string.Empty;
            if (EnablePerformanceSummary && hypotheticalPerformanceTracker != null)
            {
                performanceOutput = " PerfTotal=" + hypotheticalPerformanceTracker.Summary.TotalClosedOutcomes
                    + " PerfWinRate=" + FormatPrice(hypotheticalPerformanceTracker.Summary.WinRate)
                    + " PerfAvgR=" + FormatPrice(hypotheticalPerformanceTracker.Summary.AverageR);
            }

            string replayValidationOutput = string.Empty;
            if (EnableReplayValidation && replayValidationTracker != null && replayValidationTracker.Session != null)
            {
                replayValidationOutput = " ReplayBars=" + replayValidationTracker.Session.TotalBarsProcessed
                    + " Reviewable=" + ReplayValidationReviewable();
            }

            string diagnosticsOutput = string.Empty;
            if (EnableStrategyDiagnostics)
            {
                diagnosticsOutput = " DiagGrade=" + CurrentDiagnosticGrade()
                    + " Sim101Eligible=" + CurrentSim101Eligible();
            }

            string filterOutput = string.Empty;
            if (EnableStrategyFilterLayer && strategyFilterEngine != null)
            {
                filterOutput = " FilterProfile=" + StrategyFilterProfile
                    + " FilteredCandidates=" + strategyFilterEngine.TotalCandidatesFiltered
                    + " AllowedCandidates=" + strategyFilterEngine.TotalCandidatesAllowed;
            }

            string ictOutput = " HTFBias=" + (latestHigherTimeframeBiasSnapshot == null ? "Unknown" : latestHigherTimeframeBiasSnapshot.BiasState.ToString())
                + " AMD=" + (latestMarketPhaseSnapshot == null ? "Unknown" : latestMarketPhaseSnapshot.PhaseState.ToString())
                + " Sweep=" + (latestLiquiditySweepSnapshot == null ? "Unknown" : latestLiquiditySweepSnapshot.SweepState.ToString())
                + " FVG=" + (latestFairValueGapSnapshot == null ? "Unknown" : latestFairValueGapSnapshot.FvgState.ToString())
                + " Displacement=" + (latestDisplacementMomentumSnapshot == null ? "None" : latestDisplacementMomentumSnapshot.MomentumState.ToString())
                + " OTE=" + (latestOteZoneSnapshot == null ? "Unknown" : latestOteZoneSnapshot.OteState.ToString())
                + " TargetQuality=" + (latestIctTargetQualitySnapshot == null ? "Unknown" : latestIctTargetQualitySnapshot.TargetQualityState.ToString());

            string originalOutput = " Roadmap=" + (latestValueRoadmapSnapshot == null ? "Unknown" : latestValueRoadmapSnapshot.RoadmapState)
                + " Acceptance=" + (latestValueAcceptanceSnapshot == null ? "Unknown" : latestValueAcceptanceSnapshot.AcceptanceState.ToString())
                + " OriginalSetup=" + latestOriginalStrategySetupType
                + " TargetPlan=" + (latestAdaptiveTargetPlan != null && latestAdaptiveTargetPlan.HasTargetPlan);

            Print(Name
                + " heartbeat: CurrentBar=" + CurrentBar
                + ictOutput
                + originalOutput
                + contextOutput
                + valueOutput
                + candidateOutput
                + orderFlowOutput
                + confirmationOutput
                + journalOutput
                + outcomeOutput
                + performanceOutput
                + replayValidationOutput
                + diagnosticsOutput
                + filterOutput
                + " State=" + decision.SignalState
                + " Decision=" + decision.DecisionStatus
                + " ExecutionDisabled=True");
        }

        private static string CandidateName(LongSetupCandidateSnapshot candidateSnapshot)
        {
            if (candidateSnapshot == null || !candidateSnapshot.HasCandidate)
            {
                return "None";
            }

            switch (candidateSnapshot.CandidateSetupType)
            {
                case LongSetupType.FailedBreakdownLong:
                    return "FAILED_BREAKDOWN_LONG";
                case LongSetupType.PullbackContinuationLong:
                    return "PULLBACK_CONTINUATION_LONG";
                case LongSetupType.ValueReclaimLong:
                    return "VALUE_RECLAIM_LONG";
                case LongSetupType.BreakoutPullbackLong:
                    return "BREAKOUT_PULLBACK_LONG";
                case LongSetupType.DeviationRejectionLong:
                    return "DEVIATION_REJECTION_LONG";
                default:
                    return "None";
            }
        }

        private static string ShortReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return string.Empty;
            }

            return reason.Length <= 80 ? reason : reason.Substring(0, 80);
        }

        private static string FormatPrice(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.########") : "N/A";
        }

        [NinjaScriptProperty]
        [Display(Name = "Evaluation only mode", GroupName = "Safety", Order = 1)]
        public bool EvaluationOnlyMode { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Sim only", GroupName = "Safety", Order = 2)]
        public bool UseSimOnly { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow live trading", GroupName = "Safety", Order = 3)]
        public bool AllowLiveTrading { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity", GroupName = "Risk", Order = 10)]
        public int Quantity { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "Minimum reward/risk", GroupName = "Risk", Order = 11)]
        public double MinRewardRisk { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Failed breakdown long", GroupName = "Long Setups", Order = 20)]
        public bool EnableFailedBreakdownLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Pullback continuation long", GroupName = "Long Setups", Order = 21)]
        public bool EnablePullbackContinuationLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Value reclaim long", GroupName = "Long Setups", Order = 22)]
        public bool EnableValueReclaimLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Breakout pullback long", GroupName = "Long Setups", Order = 23)]
        public bool EnableBreakoutPullbackLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Deviation rejection long", GroupName = "Long Setups", Order = 24)]
        public bool EnableDeviationRejectionLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print debug", GroupName = "Diagnostics", Order = 30)]
        public bool PrintDebug { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print startup summary", GroupName = "Diagnostics", Order = 31)]
        public bool PrintStartupSummary { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print heartbeat", GroupName = "Diagnostics", Order = 32)]
        public bool PrintHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Debug heartbeat bars", GroupName = "Diagnostics", Order = 33)]
        public int DebugHeartbeatBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print every evaluation", GroupName = "Diagnostics", Order = 34)]
        public bool PrintEveryEvaluation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable context layer", GroupName = "Context", Order = 40)]
        public bool EnableContextLayer { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use approximate session VWAP", GroupName = "Context", Order = 41)]
        public bool UseApproximateSessionVwap { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Near VWAP ticks", GroupName = "Context", Order = 42)]
        public int NearVwapTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "VWAP band ticks", GroupName = "Context", Order = 43)]
        public int VwapBandTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print context every heartbeat", GroupName = "Context", Order = 44)]
        public bool PrintContextEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable value structure layer", GroupName = "Value Structure", Order = 50)]
        public bool EnableValueStructureLayer { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use approximate volume profile", GroupName = "Value Structure", Order = 51)]
        public bool UseApproximateVolumeProfile { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Value area percent", GroupName = "Value Structure", Order = 52)]
        public int ValueAreaPercent { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Near value ticks", GroupName = "Value Structure", Order = 53)]
        public int NearValueTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Opening range minutes", GroupName = "Value Structure", Order = 54)]
        public int OpeningRangeMinutes { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print value structure every heartbeat", GroupName = "Value Structure", Order = 55)]
        public bool PrintValueStructureEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable setup candidate detection", GroupName = "Candidate Detection", Order = 60)]
        public bool EnableSetupCandidateDetection { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print candidate every heartbeat", GroupName = "Candidate Detection", Order = 61)]
        public bool PrintCandidateEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Candidate lookback bars", GroupName = "Candidate Detection", Order = 62)]
        public int CandidateLookbackBars { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "Minimum candidate reward/risk", GroupName = "Candidate Detection", Order = 63)]
        public double MinCandidateRewardRisk { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable order-flow feature layer", GroupName = "Order Flow Features", Order = 70)]
        public bool EnableOrderFlowFeatureLayer { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use approximate order flow", GroupName = "Order Flow Features", Order = 71)]
        public bool UseApproximateOrderFlow { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Delta moving average period", GroupName = "Order Flow Features", Order = 72)]
        public int DeltaMovingAveragePeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Volume moving average period", GroupName = "Order Flow Features", Order = 73)]
        public int VolumeMovingAveragePeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "High volume multiplier", GroupName = "Order Flow Features", Order = 74)]
        public double HighVolumeMultiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print order flow every heartbeat", GroupName = "Order Flow Features", Order = 75)]
        public bool PrintOrderFlowEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable order-flow confirmation engine", GroupName = "Order Flow Confirmation", Order = 80)]
        public bool EnableOrderFlowConfirmationEngine { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print confirmation every heartbeat", GroupName = "Order Flow Confirmation", Order = 81)]
        public bool PrintConfirmationEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Minimum confirmation score", GroupName = "Order Flow Confirmation", Order = 82)]
        public int MinConfirmationScore { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Weak confirmation score", GroupName = "Order Flow Confirmation", Order = 83)]
        public int WeakConfirmationScore { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require confirmation before signal", GroupName = "Order Flow Confirmation", Order = 84)]
        public bool RequireConfirmationBeforeSignal { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable signal observation journal", GroupName = "Signal Journal", Order = 90)]
        public bool EnableSignalObservationJournal { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Journal only confirmed candidates", GroupName = "Signal Journal", Order = 91)]
        public bool JournalOnlyConfirmedCandidates { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Journal only when candidate exists", GroupName = "Signal Journal", Order = 92)]
        public bool JournalOnlyWhenCandidateExists { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Journal file name", GroupName = "Signal Journal", Order = 93)]
        public string JournalFileName { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print journal events", GroupName = "Signal Journal", Order = 94)]
        public bool PrintJournalEvents { get; set; }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Minimum journal confirmation score", GroupName = "Signal Journal", Order = 95)]
        public int MinimumJournalConfirmationScore { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Journal cooldown bars", GroupName = "Signal Journal", Order = 96)]
        public int JournalCooldownBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable hypothetical outcome tracking", GroupName = "Hypothetical Outcomes", Order = 100)]
        public bool EnableHypotheticalOutcomeTracking { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Track weak confirmations", GroupName = "Hypothetical Outcomes", Order = 101)]
        public bool TrackWeakConfirmations { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max bars to track outcome", GroupName = "Hypothetical Outcomes", Order = 102)]
        public int MaxBarsToTrackOutcome { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Conservative same-bar resolution", GroupName = "Hypothetical Outcomes", Order = 103)]
        public bool ConservativeSameBarResolution { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print outcome events", GroupName = "Hypothetical Outcomes", Order = 104)]
        public bool PrintOutcomeEvents { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print open outcome count every heartbeat", GroupName = "Hypothetical Outcomes", Order = 105)]
        public bool PrintOpenOutcomeCountEveryHeartbeat { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable performance summary", GroupName = "Performance Summary", Order = 110)]
        public bool EnablePerformanceSummary { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print performance summary", GroupName = "Performance Summary", Order = 111)]
        public bool PrintPerformanceSummary { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Performance summary every closed outcomes", GroupName = "Performance Summary", Order = 112)]
        public int PerformanceSummaryEveryClosedOutcomes { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print setup breakdown", GroupName = "Performance Summary", Order = 113)]
        public bool PrintSetupBreakdown { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Timeout result R", GroupName = "Performance Summary", Order = 114)]
        public double TimeoutResultR { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Invalidated result R", GroupName = "Performance Summary", Order = 115)]
        public double InvalidatedResultR { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Default target reward R", GroupName = "Performance Summary", Order = 116)]
        public double DefaultTargetRewardR { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable replay validation", GroupName = "Replay Validation", Order = 120)]
        public bool EnableReplayValidation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print replay validation summary", GroupName = "Replay Validation", Order = 121)]
        public bool PrintReplayValidationSummary { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Print replay validation every bars", GroupName = "Replay Validation", Order = 122)]
        public int PrintReplayValidationEveryBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum closed outcomes for review", GroupName = "Replay Validation", Order = 123)]
        public int MinimumClosedOutcomesForReview { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum bars for review", GroupName = "Replay Validation", Order = 124)]
        public int MinimumBarsForReview { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Replay session label", GroupName = "Replay Validation", Order = 125)]
        public string ReplaySessionLabel { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable strategy diagnostics", GroupName = "Strategy Diagnostics", Order = 130)]
        public bool EnableStrategyDiagnostics { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print strategy diagnostics", GroupName = "Strategy Diagnostics", Order = 131)]
        public bool PrintStrategyDiagnostics { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Diagnostics every closed outcomes", GroupName = "Strategy Diagnostics", Order = 132)]
        public int DiagnosticsEveryClosedOutcomes { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum closed outcomes for diagnostics", GroupName = "Strategy Diagnostics", Order = 133)]
        public int MinimumClosedOutcomesForDiagnostics { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum setup outcomes for decision", GroupName = "Strategy Diagnostics", Order = 134)]
        public int MinimumSetupOutcomesForDecision { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Minimum average R for Sim101", GroupName = "Strategy Diagnostics", Order = 135)]
        public double MinimumAverageRForSim101 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Minimum setup average R to keep", GroupName = "Strategy Diagnostics", Order = 136)]
        public double MinimumSetupAverageRToKeep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable strategy filter layer", GroupName = "Strategy Filter", Order = 140)]
        public bool EnableStrategyFilterLayer { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Strategy filter profile", GroupName = "Strategy Filter", Order = 141)]
        public StrategyFilterProfile StrategyFilterProfile { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print filtered candidates", GroupName = "Strategy Filter", Order = 142)]
        public bool PrintFilteredCandidates { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Print filter summary every bars", GroupName = "Strategy Filter", Order = 143)]
        public int PrintFilterSummaryEveryBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow breakout pullback long", GroupName = "Strategy Filter", Order = 144)]
        public bool V2AllowBreakoutPullbackLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow failed breakdown long", GroupName = "Strategy Filter", Order = 145)]
        public bool V2AllowFailedBreakdownLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow value reclaim long", GroupName = "Strategy Filter", Order = 146)]
        public bool V2AllowValueReclaimLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow deviation rejection long", GroupName = "Strategy Filter", Order = 147)]
        public bool V2AllowDeviationRejectionLong { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow pullback continuation long", GroupName = "Strategy Filter", Order = 148)]
        public bool V2AllowPullbackContinuationLong { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "V2 minimum confirmation score", GroupName = "Strategy Filter", Order = 149)]
        public int V2MinimumConfirmationScore { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "V2 minimum reward/risk", GroupName = "Strategy Filter", Order = 150)]
        public double V2MinimumRewardRisk { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 require confirmation observed", GroupName = "Strategy Filter", Order = 151)]
        public bool V2RequireConfirmationObserved { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 require buyer pressure", GroupName = "Strategy Filter", Order = 152)]
        public bool V2RequireBuyerPressure { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 reject strong seller pressure", GroupName = "Strategy Filter", Order = 153)]
        public bool V2RejectStrongSellerPressure { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 reject no confirmation", GroupName = "Strategy Filter", Order = 154)]
        public bool V2RejectNoConfirmation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 reject weak confirmation", GroupName = "Strategy Filter", Order = 155)]
        public bool V2RejectWeakConfirmation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 reject breakout above upper deviation", GroupName = "Strategy Filter", Order = 156)]
        public bool V2RejectBreakoutAboveUpperDeviation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow breakout only near or above VAH", GroupName = "Strategy Filter", Order = 157)]
        public bool V2AllowBreakoutOnlyNearOrAboveVAH { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow failed breakdown only below or near VAL", GroupName = "Strategy Filter", Order = 158)]
        public bool V2AllowFailedBreakdownOnlyBelowOrNearVAL { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 allow long only when context not strongly bearish for breakout", GroupName = "Strategy Filter", Order = 159)]
        public bool V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "V2 reject inside value breakout chase", GroupName = "Strategy Filter", Order = 160)]
        public bool V2RejectInsideValueBreakoutChase { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable higher timeframe bias filter", GroupName = "ICT Market Model", Order = 170)]
        public bool EnableHigherTimeframeBiasFilter { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "HTF fast period", GroupName = "ICT Market Model", Order = 171)]
        public int HtfFastPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "HTF slow period", GroupName = "ICT Market Model", Order = 172)]
        public int HtfSlowPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require HTF bias for longs", GroupName = "ICT Market Model", Order = 173)]
        public bool RequireHtfBiasForLongs { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow longs when HTF balanced", GroupName = "ICT Market Model", Order = 174)]
        public bool AllowLongsWhenHtfBalanced { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reject longs when strong bearish", GroupName = "ICT Market Model", Order = 175)]
        public bool RejectLongsWhenStrongBearish { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable AMD phase filter", GroupName = "ICT Market Model", Order = 176)]
        public bool EnableAmdPhaseFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require accumulation before manipulation", GroupName = "ICT Market Model", Order = 177)]
        public bool RequireAccumulationBeforeManipulation { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Accumulation lookback bars", GroupName = "ICT Market Model", Order = 178)]
        public int AccumulationLookbackBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max accumulation range ticks", GroupName = "ICT Market Model", Order = 179)]
        public int MaxAccumulationRangeTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Manipulation lookback bars", GroupName = "ICT Market Model", Order = 180)]
        public int ManipulationLookbackBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max bars from manipulation to entry", GroupName = "ICT Market Model", Order = 181)]
        public int MaxBarsFromManipulationToEntry { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require distribution after manipulation", GroupName = "ICT Market Model", Order = 182)]
        public bool RequireDistributionAfterManipulation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable liquidity sweep filter", GroupName = "ICT Market Model", Order = 183)]
        public bool EnableLiquiditySweepFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require sell-side sweep for longs", GroupName = "ICT Market Model", Order = 184)]
        public bool RequireSellSideSweepForLongs { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Sweep lookback bars", GroupName = "ICT Market Model", Order = 185)]
        public int SweepLookbackBars { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Sweep buffer ticks", GroupName = "ICT Market Model", Order = 186)]
        public int SweepBufferTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max bars after sweep", GroupName = "ICT Market Model", Order = 187)]
        public int MaxBarsAfterSweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require reclaim after sweep", GroupName = "ICT Market Model", Order = 188)]
        public bool RequireReclaimAfterSweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow VAL sweep as liquidity sweep", GroupName = "ICT Market Model", Order = 189)]
        public bool AllowVALSweepAsLiquiditySweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow range low sweep as liquidity sweep", GroupName = "ICT Market Model", Order = 190)]
        public bool AllowRangeLowSweepAsLiquiditySweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow swing low sweep as liquidity sweep", GroupName = "ICT Market Model", Order = 191)]
        public bool AllowSwingLowSweepAsLiquiditySweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow prior low sweep as liquidity sweep", GroupName = "ICT Market Model", Order = 192)]
        public bool AllowPriorLowSweepAsLiquiditySweep { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable fair value gap filter", GroupName = "ICT Market Model", Order = 193)]
        public bool EnableFairValueGapFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require bullish FVG for longs", GroupName = "ICT Market Model", Order = 194)]
        public bool RequireBullishFvgForLongs { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require FVG after sweep", GroupName = "ICT Market Model", Order = 195)]
        public bool RequireFvgAfterSweep { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Min FVG size ticks", GroupName = "ICT Market Model", Order = 196)]
        public int MinFvgSizeTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Track FVG retest", GroupName = "ICT Market Model", Order = 197)]
        public bool TrackFvgRetest { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require FVG retest for entry", GroupName = "ICT Market Model", Order = 198)]
        public bool RequireFvgRetestForEntry { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable displacement filter", GroupName = "ICT Market Model", Order = 199)]
        public bool EnableDisplacementFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require bullish displacement for longs", GroupName = "ICT Market Model", Order = 200)]
        public bool RequireBullishDisplacementForLongs { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Min displacement body ticks", GroupName = "ICT Market Model", Order = 201)]
        public int MinDisplacementBodyTicks { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, 1.0)]
        [Display(Name = "Min body to range ratio", GroupName = "ICT Market Model", Order = 202)]
        public double MinBodyToRangeRatio { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require close near high for displacement", GroupName = "ICT Market Model", Order = 203)]
        public bool RequireCloseNearHighForDisplacement { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, 1.0)]
        [Display(Name = "Close near high percent", GroupName = "ICT Market Model", Order = 204)]
        public double CloseNearHighPercent { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require positive delta for displacement", GroupName = "ICT Market Model", Order = 205)]
        public bool RequirePositiveDeltaForDisplacement { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable OTE filter", GroupName = "ICT Market Model", Order = 206)]
        public bool EnableOteFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require OTE for longs", GroupName = "ICT Market Model", Order = 207)]
        public bool RequireOteForLongs { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, 1.0)]
        [Display(Name = "OTE lower level", GroupName = "ICT Market Model", Order = 208)]
        public double OteLowerLevel { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, 1.0)]
        [Display(Name = "OTE mid level", GroupName = "ICT Market Model", Order = 209)]
        public double OteMidLevel { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, 1.0)]
        [Display(Name = "OTE upper level", GroupName = "ICT Market Model", Order = 210)]
        public double OteUpperLevel { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Allow discount but outside OTE", GroupName = "ICT Market Model", Order = 211)]
        public bool AllowDiscountButOutsideOte { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reject premium long entries", GroupName = "ICT Market Model", Order = 212)]
        public bool RejectPremiumLongEntries { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable ICT target quality filter", GroupName = "ICT Market Model", Order = 213)]
        public bool EnableIctTargetQualityFilter { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "Minimum target reward/risk", GroupName = "ICT Market Model", Order = 214)]
        public double MinimumTargetRewardRisk { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 20.0)]
        [Display(Name = "Preferred target reward/risk", GroupName = "ICT Market Model", Order = 215)]
        public double PreferredTargetRewardRisk { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum target room ticks", GroupName = "ICT Market Model", Order = 216)]
        public int MinimumTargetRoomTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Prefer external liquidity targets", GroupName = "ICT Market Model", Order = 217)]
        public bool PreferExternalLiquidityTargets { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use swing high as buy-side liquidity", GroupName = "ICT Market Model", Order = 218)]
        public bool UseSwingHighAsBuySideLiquidity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use session high as buy-side liquidity", GroupName = "ICT Market Model", Order = 219)]
        public bool UseSessionHighAsBuySideLiquidity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use VAH as target", GroupName = "ICT Market Model", Order = 220)]
        public bool UseVAHAsTarget { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use upper VWAP band as target", GroupName = "ICT Market Model", Order = 221)]
        public bool UseUpperVwapBandAsTarget { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reject poor target quality", GroupName = "ICT Market Model", Order = 222)]
        public bool RejectPoorTargetQuality { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable value acceptance layer", GroupName = "Original Strategy Alignment", Order = 230)]
        public bool EnableValueAcceptanceLayer { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Acceptance bars required", GroupName = "Original Strategy Alignment", Order = 231)]
        public int AcceptanceBarsRequired { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Rejection bars required", GroupName = "Original Strategy Alignment", Order = 232)]
        public int RejectionBarsRequired { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Near value edge ticks", GroupName = "Original Strategy Alignment", Order = 233)]
        public int NearValueEdgeTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable original strategy alignment", GroupName = "Original Strategy Alignment", Order = 234)]
        public bool EnableOriginalStrategyAlignment { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require RTH session only", GroupName = "Original Strategy Alignment", Order = 235)]
        public bool RequireRthSessionOnly { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require clear value roadmap", GroupName = "Original Strategy Alignment", Order = 236)]
        public bool RequireClearValueRoadmap { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require value acceptance", GroupName = "Original Strategy Alignment", Order = 237)]
        public bool RequireValueAcceptance { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require original setup type", GroupName = "Original Strategy Alignment", Order = 238)]
        public bool RequireOriginalSetupType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Require logical value target", GroupName = "Original Strategy Alignment", Order = 239)]
        public bool RequireLogicalValueTarget { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minimum logical target room ticks", GroupName = "Original Strategy Alignment", Order = 240)]
        public int MinimumLogicalTargetRoomTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Minimum original confirmation score", GroupName = "Original Strategy Alignment", Order = 241)]
        public int MinOriginalConfirmationScore { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Print original strategy events", GroupName = "Original Strategy Alignment", Order = 242)]
        public bool PrintOriginalStrategyEvents { get; set; }
    }
}
