import re
from pathlib import Path


ROOT = Path(__file__).parents[1]
NT_ROOT = ROOT / "NinjaTrader"
STRATEGY = NT_ROOT / "Strategies" / "LongOnlyOrderFlowAgentStrategy.cs"
MODELS = {
    "OrderFlowFeatureSnapshot.cs",
    "OrderFlowSignalState.cs",
    "LongSetupType.cs",
    "LongDecisionResult.cs",
    "MarketContextState.cs",
    "PriceLocationState.cs",
    "ContextFeatureSnapshot.cs",
    "ValueAreaState.cs",
    "SessionStructureSnapshot.cs",
    "LongSetupCandidateState.cs",
    "LongSetupCandidateSnapshot.cs",
    "OrderFlowBiasState.cs",
    "OrderFlowPressureState.cs",
    "OrderFlowConfirmationState.cs",
    "OrderFlowConfirmationType.cs",
    "OrderFlowConfirmationSnapshot.cs",
    "SignalObservationRecord.cs",
    "HypotheticalSignalOutcome.cs",
    "HypotheticalOutcomeState.cs",
    "HypotheticalPerformanceSummary.cs",
    "SetupPerformanceStats.cs",
    "ReplayValidationSession.cs",
    "ReplayValidationSummary.cs",
    "StrategyDiagnosticSummary.cs",
    "SetupDiagnosticResult.cs",
    "StrategyFilterResult.cs",
    "StrategyFilterProfile.cs",
    "HigherTimeframeBiasState.cs",
    "HigherTimeframeBiasSnapshot.cs",
    "MarketPhaseState.cs",
    "MarketPhaseSnapshot.cs",
    "LiquiditySweepState.cs",
    "LiquiditySweepSnapshot.cs",
    "FairValueGapState.cs",
    "FairValueGapSnapshot.cs",
    "DisplacementMomentumState.cs",
    "DisplacementMomentumSnapshot.cs",
    "OteZoneState.cs",
    "OteZoneSnapshot.cs",
    "IctTargetQualityState.cs",
    "IctTargetQualitySnapshot.cs",
    "OriginalStrategySetupType.cs",
    "ValueRoadmapSnapshot.cs",
    "ValueAcceptanceState.cs",
    "ValueAcceptanceSnapshot.cs",
    "AdaptiveTargetPlan.cs",
}
CORE = {
    "LongOnlyOrderFlowEvaluator.cs",
    "NinjaTraderSafetyGuards.cs",
    "MarketContextEvaluator.cs",
    "SessionStructureEvaluator.cs",
    "LongSetupCandidateEvaluator.cs",
    "OrderFlowFeatureEvaluator.cs",
    "OrderFlowConfirmationEvaluator.cs",
    "SignalObservationJournalWriter.cs",
    "HypotheticalOutcomeTracker.cs",
    "HypotheticalPerformanceTracker.cs",
    "ReplayValidationTracker.cs",
    "StrategyDiagnosticsEngine.cs",
    "StrategyFilterEngine.cs",
    "HigherTimeframeBiasEvaluator.cs",
    "AmdMarketPhaseEvaluator.cs",
    "LiquiditySweepEvaluator.cs",
    "FairValueGapEvaluator.cs",
    "DisplacementMomentumEvaluator.cs",
    "OteZoneEvaluator.cs",
    "IctTargetQualityEvaluator.cs",
    "ValueRoadmapEvaluator.cs",
    "ValueAcceptanceEvaluator.cs",
    "AdaptiveTargetPlanner.cs",
}
ALLOWED_SETUPS = {
    "FailedBreakdownLong",
    "PullbackContinuationLong",
    "ValueReclaimLong",
    "BreakoutPullbackLong",
    "DeviationRejectionLong",
}
FORBIDDEN_EXECUTION_APIS = {
    "EnterLong",
    "EnterShort",
    "ExitLong",
    "ExitShort",
    "SubmitOrder",
    "SubmitOrderUnmanaged",
    "SetStopLoss",
    "SetProfitTarget",
    "AtmStrategyCreate",
    "ChangeOrder",
    "CancelOrder",
    "CreateOrder",
}
REQUIRED_DEFAULTS = {
    "EvaluationOnlyMode": "true",
    "UseSimOnly": "true",
    "AllowLiveTrading": "false",
    "Quantity": "1",
    "MinRewardRisk": "2.0",
    "EnableFailedBreakdownLong": "true",
    "EnablePullbackContinuationLong": "true",
    "EnableValueReclaimLong": "true",
    "EnableBreakoutPullbackLong": "true",
    "EnableDeviationRejectionLong": "true",
    "PrintDebug": "true",
    "PrintStartupSummary": "true",
    "PrintHeartbeat": "true",
    "DebugHeartbeatBars": "50",
    "PrintEveryEvaluation": "false",
    "EnableContextLayer": "true",
    "UseApproximateSessionVwap": "true",
    "NearVwapTicks": "8",
    "VwapBandTicks": "40",
    "PrintContextEveryHeartbeat": "true",
    "EnableValueStructureLayer": "true",
    "UseApproximateVolumeProfile": "true",
    "ValueAreaPercent": "70",
    "NearValueTicks": "8",
    "OpeningRangeMinutes": "30",
    "PrintValueStructureEveryHeartbeat": "true",
    "EnableSetupCandidateDetection": "true",
    "PrintCandidateEveryHeartbeat": "true",
    "CandidateLookbackBars": "20",
    "MinCandidateRewardRisk": "2.0",
    "EnableOrderFlowFeatureLayer": "true",
    "UseApproximateOrderFlow": "true",
    "DeltaMovingAveragePeriod": "20",
    "VolumeMovingAveragePeriod": "20",
    "HighVolumeMultiplier": "1.5",
    "PrintOrderFlowEveryHeartbeat": "true",
    "EnableOrderFlowConfirmationEngine": "true",
    "PrintConfirmationEveryHeartbeat": "true",
    "MinConfirmationScore": "70",
    "WeakConfirmationScore": "50",
    "RequireConfirmationBeforeSignal": "true",
    "EnableSignalObservationJournal": "true",
    "JournalOnlyConfirmedCandidates": "false",
    "JournalOnlyWhenCandidateExists": "true",
    "JournalFileName": '"orderflow_signal_observations.jsonl"',
    "PrintJournalEvents": "true",
    "MinimumJournalConfirmationScore": "0",
    "JournalCooldownBars": "5",
    "EnableHypotheticalOutcomeTracking": "true",
    "TrackWeakConfirmations": "false",
    "MaxBarsToTrackOutcome": "50",
    "ConservativeSameBarResolution": "true",
    "PrintOutcomeEvents": "true",
    "PrintOpenOutcomeCountEveryHeartbeat": "true",
    "EnablePerformanceSummary": "true",
    "PrintPerformanceSummary": "true",
    "PerformanceSummaryEveryClosedOutcomes": "25",
    "PrintSetupBreakdown": "true",
    "TimeoutResultR": "0.0",
    "InvalidatedResultR": "0.0",
    "DefaultTargetRewardR": "2.0",
    "EnableReplayValidation": "true",
    "PrintReplayValidationSummary": "true",
    "PrintReplayValidationEveryBars": "500",
    "MinimumClosedOutcomesForReview": "50",
    "MinimumBarsForReview": "500",
    "ReplaySessionLabel": '""',
    "EnableStrategyDiagnostics": "true",
    "PrintStrategyDiagnostics": "true",
    "DiagnosticsEveryClosedOutcomes": "100",
    "MinimumClosedOutcomesForDiagnostics": "100",
    "MinimumSetupOutcomesForDecision": "20",
    "MinimumAverageRForSim101": "0.05",
    "MinimumSetupAverageRToKeep": "0.0",
    "EnableStrategyFilterLayer": "true",
    "StrategyFilterProfile": "StrategyFilterProfile.DiagnosticV2",
    "PrintFilteredCandidates": "true",
    "PrintFilterSummaryEveryBars": "500",
    "V2AllowBreakoutPullbackLong": "true",
    "V2AllowFailedBreakdownLong": "true",
    "V2AllowValueReclaimLong": "false",
    "V2AllowDeviationRejectionLong": "false",
    "V2AllowPullbackContinuationLong": "false",
    "V2MinimumConfirmationScore": "85",
    "V2MinimumRewardRisk": "2.0",
    "V2RequireConfirmationObserved": "true",
    "V2RequireBuyerPressure": "false",
    "V2RejectStrongSellerPressure": "true",
    "V2RejectNoConfirmation": "true",
    "V2RejectWeakConfirmation": "true",
    "V2RejectBreakoutAboveUpperDeviation": "true",
    "V2AllowBreakoutOnlyNearOrAboveVAH": "true",
    "V2AllowFailedBreakdownOnlyBelowOrNearVAL": "true",
    "V2AllowLongOnlyWhenContextNotStronglyBearishForBreakout": "true",
    "V2RejectInsideValueBreakoutChase": "true",
    "EnableHigherTimeframeBiasFilter": "true",
    "HtfFastPeriod": "50",
    "HtfSlowPeriod": "200",
    "RequireHtfBiasForLongs": "true",
    "AllowLongsWhenHtfBalanced": "false",
    "RejectLongsWhenStrongBearish": "true",
    "EnableAmdPhaseFilter": "true",
    "RequireAccumulationBeforeManipulation": "true",
    "AccumulationLookbackBars": "30",
    "MaxAccumulationRangeTicks": "80",
    "ManipulationLookbackBars": "20",
    "MaxBarsFromManipulationToEntry": "20",
    "RequireDistributionAfterManipulation": "true",
    "EnableLiquiditySweepFilter": "true",
    "RequireSellSideSweepForLongs": "true",
    "SweepLookbackBars": "30",
    "SweepBufferTicks": "2",
    "MaxBarsAfterSweep": "15",
    "RequireReclaimAfterSweep": "true",
    "AllowVALSweepAsLiquiditySweep": "true",
    "AllowRangeLowSweepAsLiquiditySweep": "true",
    "AllowSwingLowSweepAsLiquiditySweep": "true",
    "AllowPriorLowSweepAsLiquiditySweep": "true",
    "EnableFairValueGapFilter": "true",
    "RequireBullishFvgForLongs": "true",
    "RequireFvgAfterSweep": "true",
    "MinFvgSizeTicks": "4",
    "TrackFvgRetest": "true",
    "RequireFvgRetestForEntry": "false",
    "EnableDisplacementFilter": "true",
    "RequireBullishDisplacementForLongs": "true",
    "MinDisplacementBodyTicks": "8",
    "MinBodyToRangeRatio": "0.55",
    "RequireCloseNearHighForDisplacement": "true",
    "CloseNearHighPercent": "0.30",
    "RequirePositiveDeltaForDisplacement": "true",
    "EnableOteFilter": "true",
    "RequireOteForLongs": "true",
    "OteLowerLevel": "0.61",
    "OteMidLevel": "0.70",
    "OteUpperLevel": "0.79",
    "AllowDiscountButOutsideOte": "false",
    "RejectPremiumLongEntries": "true",
    "EnableIctTargetQualityFilter": "true",
    "MinimumTargetRewardRisk": "2.0",
    "PreferredTargetRewardRisk": "2.5",
    "MinimumTargetRoomTicks": "20",
    "PreferExternalLiquidityTargets": "true",
    "UseSwingHighAsBuySideLiquidity": "true",
    "UseSessionHighAsBuySideLiquidity": "true",
    "UseVAHAsTarget": "true",
    "UseUpperVwapBandAsTarget": "true",
    "RejectPoorTargetQuality": "true",
    "EnableValueAcceptanceLayer": "true",
    "AcceptanceBarsRequired": "3",
    "RejectionBarsRequired": "1",
    "NearValueEdgeTicks": "8",
    "EnableOriginalStrategyAlignment": "true",
    "RequireRthSessionOnly": "true",
    "RequireClearValueRoadmap": "true",
    "RequireValueAcceptance": "true",
    "RequireOriginalSetupType": "true",
    "RequireLogicalValueTarget": "true",
    "MinimumLogicalTargetRoomTicks": "20",
    "MinOriginalConfirmationScore": "85",
    "PrintOriginalStrategyEvents": "true",
}


def csharp_sources() -> dict[Path, str]:
    return {
        path: path.read_text(encoding="utf-8")
        for path in NT_ROOT.rglob("*.cs")
    }


def test_nt1a_requested_source_layout_exists_without_old_duplicates() -> None:
    assert STRATEGY.is_file()
    assert MODELS == {path.name for path in (NT_ROOT / "Models").glob("*.cs")}
    assert CORE == {path.name for path in (NT_ROOT / "Core").glob("*.cs")}
    assert not list((NT_ROOT / "bin").rglob("*.cs"))


def test_strategy_uses_ninjatrader_namespace_and_lifecycle() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    assert "namespace NinjaTrader.NinjaScript.Strategies" in source
    assert "class LongOnlyOrderFlowAgentStrategy : Strategy" in source
    assert "protected override void OnStateChange()" in source
    assert "protected override void OnBarUpdate()" in source
    assert "LongOnlyOrderFlowEvaluator" in source


def test_strategy_has_all_required_safe_defaults() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for property_name, expected in REQUIRED_DEFAULTS.items():
        assert re.search(
            rf"\b{property_name}\s*=\s*{re.escape(expected)}\s*;", source
        ), property_name
        assert re.search(rf"public\s+\w+\s+{property_name}\s*\{{", source), property_name


def test_strategy_prints_evaluation_state_without_execution_branch() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "EVALUATION_ONLY",
        "NO_EXECUTION_ENABLED",
        "SignalState",
        "DecisionStatus",
        "Reason",
        "EntryPrice",
        "StopPrice",
        "TargetPrice",
    ):
        assert text in source
    assert "Print(" in source


def test_strategy_has_nt1c_throttled_runtime_logging_controls() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for property_name in (
        "PrintStartupSummary",
        "PrintHeartbeat",
        "DebugHeartbeatBars",
        "PrintEveryEvaluation",
    ):
        assert re.search(rf"public\s+\w+\s+{property_name}\s*\{{", source), property_name
    assert "PrintStartupSummary = true;" in source
    assert "PrintHeartbeat = true;" in source
    assert "DebugHeartbeatBars = 50;" in source
    assert "PrintEveryEvaluation = false;" in source
    assert "heartbeat:" in source
    assert "PrintEveryEvaluation" in source and "PrintHeartbeat" in source


def test_nt2a_context_model_files_and_fields_exist() -> None:
    snapshot = (NT_ROOT / "Models" / "ContextFeatureSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "CurrentPrice",
        "SessionHigh",
        "SessionLow",
        "Vwap",
        "UpperVwapBand",
        "LowerVwapBand",
        "DistanceFromVwapPoints",
        "DistanceFromVwapTicks",
        "DistanceFromVwapPercent",
        "ContextState",
        "LocationState",
        "HasValidContext",
        "Reason",
    ):
        assert re.search(rf"\b{field}\s*\{{", snapshot), field


def test_nt2a_context_enums_contain_expected_states() -> None:
    context_state = (NT_ROOT / "Models" / "MarketContextState.cs").read_text(
        encoding="utf-8"
    )
    location_state = (NT_ROOT / "Models" / "PriceLocationState.cs").read_text(
        encoding="utf-8"
    )
    for state in (
        "Unknown",
        "Bullish",
        "Bearish",
        "Balanced",
        "ExtendedBullish",
        "ExtendedBearish",
    ):
        assert re.search(rf"\b{state}\b", context_state), state
    for state in (
        "Unknown",
        "AboveVwap",
        "BelowVwap",
        "NearVwap",
        "AboveUpperDeviation",
        "BelowLowerDeviation",
        "NearSessionHigh",
        "NearSessionLow",
        "InsideSessionRange",
    ):
        assert re.search(rf"\b{state}\b", location_state), state


def test_nt2a_market_context_evaluator_is_deterministic_and_non_executing() -> None:
    source = (NT_ROOT / "Core" / "MarketContextEvaluator.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class MarketContextEvaluator",
        "Evaluate",
        "MarketContextState.Balanced",
        "MarketContextState.Bullish",
        "MarketContextState.Bearish",
        "MarketContextState.ExtendedBullish",
        "MarketContextState.ExtendedBearish",
        "PriceLocationState.NearVwap",
        "PriceLocationState.AboveUpperDeviation",
        "PriceLocationState.BelowLowerDeviation",
    ):
        assert text in source
    assert "LongDecisionStatus.LongValid" not in source


def test_nt2a_strategy_wires_approximate_context_without_enabling_trades() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "MarketContextEvaluator",
        "ContextFeatureSnapshot",
        "UpdateSessionContext",
        "BuildContextSnapshot",
        "TypicalPrice",
        "Bars.IsFirstBarOfSession",
        "EnableContextLayer",
        "UseApproximateSessionVwap",
        "NearVwapTicks",
        "VwapBandTicks",
        "PrintContextEveryHeartbeat",
        "Context layer active",
        "Order-flow confirmation not implemented",
        "Price=",
        "VWAP=",
        "Context=",
        "Location=",
    ):
        assert text in source
    assert "LongConfirmationPresent = true" not in source
    assert "DecisionStatus = LongDecisionStatus.LongValid" not in source


def test_nt2b_value_structure_model_files_and_fields_exist() -> None:
    snapshot = (NT_ROOT / "Models" / "SessionStructureSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "CurrentPrice",
        "SessionHigh",
        "SessionLow",
        "PriorSessionHigh",
        "PriorSessionLow",
        "OpeningRangeHigh",
        "OpeningRangeLow",
        "ApproxPoc",
        "ApproxVah",
        "ApproxVal",
        "DistanceFromPocTicks",
        "DistanceFromVahTicks",
        "DistanceFromValTicks",
        "ValueState",
        "HasValidValueStructure",
        "Reason",
    ):
        assert re.search(rf"\b{field}\s*\{{", snapshot), field


def test_nt2b_value_area_enum_contains_expected_states() -> None:
    source = (NT_ROOT / "Models" / "ValueAreaState.cs").read_text(encoding="utf-8")
    for state in (
        "Unknown",
        "InsideValue",
        "AboveValue",
        "BelowValue",
        "NearVAH",
        "NearVAL",
        "NearPOC",
        "AtSessionHigh",
        "AtSessionLow",
        "AbovePriorHigh",
        "BelowPriorLow",
        "InsidePriorRange",
    ):
        assert re.search(rf"\b{state}\b", source), state


def test_nt2b_session_structure_evaluator_is_deterministic_and_non_executing() -> None:
    source = (NT_ROOT / "Core" / "SessionStructureEvaluator.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class SessionStructureEvaluator",
        "Evaluate",
        "ValueAreaState.NearPOC",
        "ValueAreaState.NearVAH",
        "ValueAreaState.NearVAL",
        "ValueAreaState.AboveValue",
        "ValueAreaState.BelowValue",
        "ValueAreaState.InsideValue",
        "PriorSessionHigh",
        "PriorSessionLow",
    ):
        assert text in source
    assert "LongDecisionStatus.LongValid" not in source


def test_nt2b_strategy_wires_approximate_value_structure_without_enabling_trades() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "SessionStructureEvaluator",
        "SessionStructureSnapshot",
        "Dictionary<double, double>",
        "UpdateSessionStructure",
        "UpdateApproximateVolumeProfile",
        "BuildSessionStructureSnapshot",
        "CalculateApproximateValueArea",
        "EnableValueStructureLayer",
        "UseApproximateVolumeProfile",
        "ValueAreaPercent",
        "NearValueTicks",
        "OpeningRangeMinutes",
        "PrintValueStructureEveryHeartbeat",
        "Value structure layer active",
        "Approximate volume profile active",
        "POC=",
        "VAH=",
        "VAL=",
        "ValueState=",
    ):
        assert text in source
    assert "LongConfirmationPresent = true" not in source
    assert "DecisionStatus = LongDecisionStatus.LongValid" not in source


def test_nt2c_candidate_model_files_and_fields_exist() -> None:
    snapshot = (NT_ROOT / "Models" / "LongSetupCandidateSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "CandidateSetupType",
        "CandidateState",
        "HasCandidate",
        "RequiresOrderFlowConfirmation",
        "Reason",
        "CandidateEntryPrice",
        "CandidateStopPrice",
        "CandidateTargetPrice",
        "CandidateRewardRisk",
        "ContextState",
        "LocationState",
        "ValueState",
    ):
        assert re.search(rf"\b{field}\s*\{{", snapshot), field


def test_nt2c_candidate_state_enum_contains_expected_states() -> None:
    source = (NT_ROOT / "Models" / "LongSetupCandidateState.cs").read_text(
        encoding="utf-8"
    )
    for state in (
        "None",
        "CandidateDetected",
        "WaitingForConfirmation",
        "InvalidContext",
        "InvalidLocation",
        "Disabled",
    ):
        assert re.search(rf"\b{state}\b", source), state


def test_nt2c_candidate_evaluator_priority_and_confirmation_are_safe() -> None:
    source = (NT_ROOT / "Core" / "LongSetupCandidateEvaluator.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class LongSetupCandidateEvaluator",
        "FAILED_BREAKDOWN_LONG",
        "VALUE_RECLAIM_LONG",
        "DEVIATION_REJECTION_LONG",
        "BREAKOUT_PULLBACK_LONG",
        "PULLBACK_CONTINUATION_LONG",
        "RequiresOrderFlowConfirmation = true",
        "LongSetupCandidateState.WaitingForConfirmation",
        "Candidate priority",
        "EvaluateFailedBreakdownLong",
        "EvaluateValueReclaimLong",
        "EvaluateDeviationRejectionLong",
        "EvaluateBreakoutPullbackLong",
        "EvaluatePullbackContinuationLong",
    ):
        assert text in source
    assert "LongDecisionStatus.LongValid" not in source
    assert "ExecutionReady" not in source
    assert "OrderReady" not in source


def test_nt2c_strategy_wires_candidates_without_confirming_or_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "LongSetupCandidateEvaluator",
        "LongSetupCandidateSnapshot",
        "BuildLongSetupCandidateSnapshot",
        "UpdateRecentCandidateState",
        "PreviousValueState",
        "PreviousContextState",
        "PreviousLocationState",
        "PreviousClosePrice",
        "BarsSinceBelowValue",
        "BarsSinceAboveValue",
        "EnableSetupCandidateDetection",
        "PrintCandidateEveryHeartbeat",
        "CandidateLookbackBars",
        "MinCandidateRewardRisk",
        "Setup candidate detection active",
        "Candidate=",
        "CandidateState=",
        " RR=",
    ):
        assert text in source
    forbidden = (
        "LongConfirmationPresent = true",
        "DecisionStatus = LongDecisionStatus.LongValid",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
    )
    for text in forbidden:
        assert text not in source


def test_nt3a_order_flow_snapshot_has_approximate_feature_fields() -> None:
    source = (NT_ROOT / "Models" / "OrderFlowFeatureSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "BarVolume",
        "ApproxBuyVolume",
        "ApproxSellVolume",
        "BarDelta",
        "CumulativeDelta",
        "DeltaMovingAverage",
        "VolumeMovingAverage",
        "DeltaStrength",
        "VolumeRatio",
        "IsHighVolumeBar",
        "IsPositiveDelta",
        "IsNegativeDelta",
        "IsCvdRising",
        "IsCvdFalling",
        "HasApproxOrderFlow",
        "UsesApproximation",
        "Source",
        "Reason",
    ):
        assert re.search(rf"\b{field}\s*\{{", source), field


def test_nt3a_order_flow_state_enums_exist() -> None:
    bias = (NT_ROOT / "Models" / "OrderFlowBiasState.cs").read_text(
        encoding="utf-8"
    )
    pressure = (NT_ROOT / "Models" / "OrderFlowPressureState.cs").read_text(
        encoding="utf-8"
    )
    for state in (
        "Unknown",
        "BuyerPressure",
        "SellerPressure",
        "Balanced",
        "StrongBuyerPressure",
        "StrongSellerPressure",
    ):
        assert re.search(rf"\b{state}\b", bias), state
    for state in (
        "Unknown",
        "PositiveDelta",
        "NegativeDelta",
        "HighVolumePositiveDelta",
        "HighVolumeNegativeDelta",
        "LowVolume",
        "ExhaustionCandidate",
        "AbsorptionCandidatePlaceholder",
    ):
        assert re.search(rf"\b{state}\b", pressure), state


def test_nt3a_order_flow_feature_evaluator_is_approximate_and_non_confirming() -> None:
    source = (NT_ROOT / "Core" / "OrderFlowFeatureEvaluator.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class OrderFlowFeatureEvaluator",
        "ApproxBuyVolume",
        "ApproxSellVolume",
        "BarDelta",
        "CumulativeDelta",
        "OrderFlowBiasState.BuyerPressure",
        "OrderFlowBiasState.SellerPressure",
        "OrderFlowBiasState.StrongBuyerPressure",
        "OrderFlowBiasState.StrongSellerPressure",
        "OrderFlowPressureState.HighVolumePositiveDelta",
        "OrderFlowPressureState.HighVolumeNegativeDelta",
        "UsesApproximation = true",
        "approximate",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt3a_strategy_wires_order_flow_features_without_confirming_or_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "OrderFlowFeatureEvaluator",
        "BuildApproxOrderFlowFeatureSnapshot",
        "EnableOrderFlowFeatureLayer",
        "UseApproximateOrderFlow",
        "DeltaMovingAveragePeriod",
        "VolumeMovingAveragePeriod",
        "HighVolumeMultiplier",
        "PrintOrderFlowEveryHeartbeat",
        "Order-flow feature layer active",
        "Approximate order-flow active",
        "True bid/ask volumetric data not wired yet",
        "Confirmation engine not implemented",
        "OFBias=",
        "Delta=",
        "CVD=",
        "HighVol=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt3b_confirmation_model_files_and_fields_exist() -> None:
    snapshot = (NT_ROOT / "Models" / "OrderFlowConfirmationSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "ConfirmationState",
        "ConfirmationType",
        "HasConfirmation",
        "RequiresExecutionDisabled",
        "UsesApproximateOrderFlow",
        "CandidateSetupType",
        "Reason",
        "ConfirmationScore",
        "BarDelta",
        "CumulativeDelta",
        "OrderFlowBias",
        "IsHighVolumeBar",
    ):
        assert re.search(rf"\b{field}\s*\{{", snapshot), field


def test_nt3b_confirmation_enums_contain_expected_states() -> None:
    state = (NT_ROOT / "Models" / "OrderFlowConfirmationState.cs").read_text(
        encoding="utf-8"
    )
    confirmation_type = (
        NT_ROOT / "Models" / "OrderFlowConfirmationType.cs"
    ).read_text(encoding="utf-8")
    for item in (
        "None",
        "WaitingForConfirmation",
        "ConfirmationObserved",
        "WeakConfirmation",
        "NoConfirmation",
        "InvalidCandidate",
        "Disabled",
        "ApproximationOnly",
    ):
        assert re.search(rf"\b{item}\b", state), item
    for item in (
        "None",
        "BuyerPressureConfirmation",
        "SellerExhaustionCandidate",
        "SellerAbsorptionCandidate",
        "CvdReclaimCandidate",
        "DeltaShiftCandidate",
        "HighVolumeReversalCandidate",
        "ApproximationOnly",
    ):
        assert re.search(rf"\b{item}\b", confirmation_type), item


def test_nt3b_confirmation_evaluator_is_approximate_and_non_executable() -> None:
    source = (NT_ROOT / "Core" / "OrderFlowConfirmationEvaluator.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class OrderFlowConfirmationEvaluator",
        "BuyerPressureConfirmation",
        "SellerExhaustionCandidate",
        "SellerAbsorptionCandidate",
        "CvdReclaimCandidate",
        "DeltaShiftCandidate",
        "HighVolumeReversalCandidate",
        "ConfirmationObserved",
        "WeakConfirmation",
        "UsesApproximateOrderFlow = true",
        "RequiresExecutionDisabled = true",
        "approximate",
        "minimumConfirmationScore",
        "weakConfirmationScore",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt3b_strategy_wires_confirmation_without_confirming_or_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "OrderFlowConfirmationEvaluator",
        "OrderFlowConfirmationSnapshot",
        "BuildOrderFlowConfirmationSnapshot",
        "EnableOrderFlowConfirmationEngine",
        "PrintConfirmationEveryHeartbeat",
        "MinConfirmationScore",
        "WeakConfirmationScore",
        "RequireConfirmationBeforeSignal",
        "Confirmation engine active",
        "Approximate confirmation only",
        "True volumetric confirmation not wired yet",
        "Confirmation=",
        "ConfirmationState=",
        " Score=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4a_signal_observation_record_contains_required_fields() -> None:
    source = (NT_ROOT / "Models" / "SignalObservationRecord.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "RecordId",
        "Timestamp",
        "Instrument",
        "CurrentBar",
        "Price",
        "ContextState",
        "LocationState",
        "Vwap",
        "Poc",
        "Vah",
        "Val",
        "ValueState",
        "CandidateSetupType",
        "CandidateState",
        "CandidateRewardRisk",
        "CandidateReason",
        "OrderFlowBias",
        "BarDelta",
        "CumulativeDelta",
        "IsHighVolumeBar",
        "ConfirmationType",
        "ConfirmationState",
        "ConfirmationScore",
        "ConfirmationReason",
        "ExecutionDisabled",
        "EvaluationOnlyMode",
        "DecisionState",
        "Notes",
    ):
        assert re.search(rf"\b{field}\s*\{{", source), field


def test_nt4a_signal_observation_writer_is_jsonl_safe_and_non_executing() -> None:
    source = (NT_ROOT / "Core" / "SignalObservationJournalWriter.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class SignalObservationJournalWriter",
        "orderflow_signal_observations.jsonl",
        "SIGNAL_OBSERVATION_JSON=",
        "AppendAllText",
        "try",
        "catch",
        "ToJsonLine",
        "Write",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongValid",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4a_strategy_wires_observation_journal_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "SignalObservationJournalWriter",
        "SignalObservationRecord",
        "MaybeWriteSignalObservation",
        "BuildSignalObservationRecord",
        "EnableSignalObservationJournal",
        "JournalOnlyConfirmedCandidates",
        "JournalOnlyWhenCandidateExists",
        "JournalFileName",
        "PrintJournalEvents",
        "MinimumJournalConfirmationScore",
        "JournalCooldownBars",
        "Signal journal active",
        "Journaled=",
        "Journal=Enabled",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4b_hypothetical_outcome_model_contains_required_fields() -> None:
    source = (NT_ROOT / "Models" / "HypotheticalSignalOutcome.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "RecordId",
        "Instrument",
        "SignalTime",
        "SignalBar",
        "CandidateSetupType",
        "ConfirmationState",
        "ConfirmationType",
        "EntryPrice",
        "StopPrice",
        "TargetPrice",
        "RewardRisk",
        "BarsOpen",
        "MaxBarsToTrack",
        "MaxFavorableExcursion",
        "MaxAdverseExcursion",
        "MaxFavorableR",
        "MaxAdverseR",
        "OutcomeState",
        "OutcomeTime",
        "OutcomeBar",
        "OutcomePrice",
        "OutcomeReason",
        "IsClosed",
        "ExecutionDisabled",
        "EvaluationOnlyMode",
    ):
        assert re.search(rf"\b{field}\s*\{{", source), field


def test_nt4b_hypothetical_outcome_state_enum_contains_expected_states() -> None:
    source = (NT_ROOT / "Models" / "HypotheticalOutcomeState.cs").read_text(
        encoding="utf-8"
    )
    for state in (
        "Open",
        "TargetHit",
        "StopHit",
        "Timeout",
        "Invalidated",
        "Canceled",
        "Unknown",
    ):
        assert re.search(rf"\b{state}\b", source), state


def test_nt4b_hypothetical_outcome_tracker_is_observation_only() -> None:
    source = (NT_ROOT / "Core" / "HypotheticalOutcomeTracker.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class HypotheticalOutcomeTracker",
        "TryOpenFromObservation",
        "UpdateOpenOutcomes",
        "HYPOTHETICAL_OUTCOME_OPENED=",
        "HYPOTHETICAL_OUTCOME_CLOSED=",
        "ConfirmationObserved",
        "WeakConfirmation",
        "ConservativeSameBarResolution",
        "StopHit",
        "TargetHit",
        "Timeout",
        "MaxFavorableExcursion",
        "MaxAdverseExcursion",
        "MaxFavorableR",
        "MaxAdverseR",
        "observation-only",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4b_strategy_wires_outcome_tracking_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "HypotheticalOutcomeTracker",
        "EnableHypotheticalOutcomeTracking",
        "TrackWeakConfirmations",
        "MaxBarsToTrackOutcome",
        "ConservativeSameBarResolution",
        "PrintOutcomeEvents",
        "PrintOpenOutcomeCountEveryHeartbeat",
        "MaybeTrackHypotheticalOutcome",
        "UpdateHypotheticalOutcomes",
        "Outcome tracking active",
        "OpenOutcomes=",
        "ClosedOutcomes=",
        "LastOutcome=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4b_docs_explain_observation_only_outcomes_and_same_bar_conservatism() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "INSTALL_IN_NINJATRADER.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4b",
        "hypothetical outcome tracking",
        "observation-only",
        "does not execute",
        "target",
        "stop",
        "timeout",
        "same-bar",
        "conservative",
        "not proof of profitability",
    ):
        assert text in combined


def test_nt4c_performance_summary_model_contains_required_fields() -> None:
    source = (NT_ROOT / "Models" / "HypotheticalPerformanceSummary.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "TotalClosedOutcomes",
        "TargetHits",
        "StopHits",
        "Timeouts",
        "Invalidated",
        "OpenOutcomes",
        "WinRate",
        "StopRate",
        "TimeoutRate",
        "AverageR",
        "TotalR",
        "BestR",
        "WorstR",
        "AverageMaxFavorableR",
        "AverageMaxAdverseR",
        "BestSetupType",
        "WorstSetupType",
        "LastUpdated",
        "SummaryReason",
    ):
        assert re.search(rf"\b{field}\s*\{{", source), field


def test_nt4c_setup_performance_stats_model_contains_required_fields() -> None:
    source = (NT_ROOT / "Models" / "SetupPerformanceStats.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "SetupType",
        "Total",
        "TargetHits",
        "StopHits",
        "Timeouts",
        "WinRate",
        "AverageR",
        "TotalR",
        "AverageMfeR",
        "AverageMaeR",
        "BestR",
        "WorstR",
    ):
        assert re.search(rf"\b{field}\s*\{{", source), field


def test_nt4c_performance_tracker_is_observation_only_and_summarizes_outcomes() -> None:
    source = (NT_ROOT / "Core" / "HypotheticalPerformanceTracker.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class HypotheticalPerformanceTracker",
        "HypotheticalPerformanceSummary",
        "SetupPerformanceStats",
        "RecordClosedOutcome",
        "RecordClosedOutcomes",
        "PERFORMANCE_SUMMARY=",
        "SETUP_STATS=",
        "TargetHit",
        "StopHit",
        "Timeout",
        "Invalidated",
        "DefaultTargetRewardR",
        "TimeoutResultR",
        "InvalidatedResultR",
        "BestSetupType",
        "WorstSetupType",
        "observation-only",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4c_strategy_wires_performance_summary_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "HypotheticalPerformanceTracker",
        "EnablePerformanceSummary",
        "PrintPerformanceSummary",
        "PerformanceSummaryEveryClosedOutcomes",
        "PrintSetupBreakdown",
        "TimeoutResultR",
        "InvalidatedResultR",
        "DefaultTargetRewardR",
        "UpdatePerformanceSummary",
        "Performance summary active",
        "PerfTotal=",
        "PerfWinRate=",
        "PerfAvgR=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4c_docs_explain_hypothetical_stats_are_non_executable() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "INSTALL_IN_NINJATRADER.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4c",
        "performance summary",
        "hypothetical",
        "not real fills",
        "does not execute",
        "not proof of profitability",
        "setup types",
        "further testing",
        "nt-4d",
    ):
        assert text in combined


def test_nt4d_replay_validation_model_files_and_fields_exist() -> None:
    session = (NT_ROOT / "Models" / "ReplayValidationSession.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "SessionId",
        "Instrument",
        "StrategyName",
        "StartedAt",
        "EndedAt",
        "DataMode",
        "BarType",
        "Timeframe",
        "TradingHoursTemplate",
        "EvaluationOnlyMode",
        "ExecutionDisabled",
        "UsesApproximateOrderFlow",
        "UsesApproximateVolumeProfile",
        "StartingBar",
        "EndingBar",
        "TotalBarsProcessed",
        "Notes",
    ):
        assert re.search(rf"\b{field}\s*\{{", session), field

    summary = (NT_ROOT / "Models" / "ReplayValidationSummary.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "SessionId",
        "Instrument",
        "StartedAt",
        "EndedAt",
        "TotalBarsProcessed",
        "JournaledCandidates",
        "ConfirmedCandidates",
        "WeakConfirmations",
        "NoConfirmations",
        "OpenOutcomes",
        "ClosedOutcomes",
        "TargetHits",
        "StopHits",
        "Timeouts",
        "WinRate",
        "AverageR",
        "TotalR",
        "BestSetupType",
        "WorstSetupType",
        "IsReviewable",
        "ReviewWarnings",
        "SummaryReason",
    ):
        assert re.search(rf"\b{field}\s*\{{", summary), field


def test_nt4d_replay_validation_tracker_is_observation_only_and_prints_outputs() -> None:
    source = (NT_ROOT / "Core" / "ReplayValidationTracker.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class ReplayValidationTracker",
        "ReplayValidationSession",
        "ReplayValidationSummary",
        "REPLAY_VALIDATION_PROGRESS=",
        "REPLAY_VALIDATION_SUMMARY=",
        "StartSession",
        "RecordBar",
        "BuildSummary",
        "PrintProgress",
        "PrintFinalSummary",
        "MinimumClosedOutcomesForReview",
        "MinimumBarsForReview",
        "IsReviewable",
        "JournaledCandidates",
        "ConfirmedCandidates",
        "WeakConfirmations",
        "NoConfirmations",
        "observation-only",
        "non-executable",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4d_strategy_wires_replay_validation_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "ReplayValidationTracker",
        "EnableReplayValidation",
        "PrintReplayValidationSummary",
        "PrintReplayValidationEveryBars",
        "MinimumClosedOutcomesForReview",
        "MinimumBarsForReview",
        "ReplaySessionLabel",
        "Replay validation active",
        "MaybePrintReplayValidationProgress",
        "PrintReplayValidationFinalSummary",
        "ReplayBars=",
        "Reviewable=",
        "REPLAY_VALIDATION_PROGRESS=",
        "REPLAY_VALIDATION_SUMMARY=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4d_docs_and_checklist_explain_replay_validation_safety() -> None:
    checklist = NT_ROOT / "REPLAY_VALIDATION_CHECKLIST.md"
    assert checklist.is_file()
    checklist_text = checklist.read_text(encoding="utf-8").lower()
    for text in (
        "20 sessions",
        "50 to 100 closed hypothetical outcomes",
        "performance_summary",
        "setup_stats",
        "replay_validation_progress",
        "replay_validation_summary",
        "cherry-picking",
        "do not skip bad sessions",
        "positive average r",
        "sim101",
    ):
        assert text in checklist_text

    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "INSTALL_IN_NINJATRADER.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4d",
        "replay validation",
        "observation-only",
        "non-executable",
        "does not execute",
        "does not prove profitability",
        "replay_validation_progress",
        "replay_validation_summary",
        "execution remains disabled",
    ):
        assert text in combined


def test_nt4e_strategy_diagnostic_model_files_and_fields_exist() -> None:
    summary = (NT_ROOT / "Models" / "StrategyDiagnosticSummary.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "Timestamp",
        "SessionId",
        "TotalClosedOutcomes",
        "WinRate",
        "AverageR",
        "TotalR",
        "BestSetupType",
        "WorstSetupType",
        "IsEligibleForSim101",
        "EligibilityReason",
        "OverallGrade",
        "PrimaryProblem",
        "RecommendedAction",
        "Warnings",
    ):
        assert re.search(rf"\b{field}\s*\{{", summary), field

    setup = (NT_ROOT / "Models" / "SetupDiagnosticResult.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "SetupType",
        "Total",
        "WinRate",
        "AverageR",
        "TotalR",
        "ShouldDisable",
        "ShouldTighten",
        "ShouldKeepTesting",
        "DiagnosticReason",
        "RecommendedAction",
    ):
        assert re.search(rf"\b{field}\s*\{{", setup), field


def test_nt4e_diagnostics_engine_is_observation_only_and_recommends_actions() -> None:
    source = (NT_ROOT / "Core" / "StrategyDiagnosticsEngine.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class StrategyDiagnosticsEngine",
        "StrategyDiagnosticSummary",
        "SetupDiagnosticResult",
        "STRATEGY_DIAGNOSTICS=",
        "SETUP_DIAGNOSTIC=",
        "Evaluate",
        "EvaluateSetups",
        "PrintDiagnostics",
        "InsufficientSample",
        "NegativeExpectancy",
        "IsEligibleForSim101 = false",
        "MinimumClosedOutcomesForDiagnostics",
        "MinimumSetupOutcomesForDecision",
        "MinimumAverageRForSim101",
        "MinimumSetupAverageRToKeep",
        "Do not proceed to Sim101",
        "observation-only",
        "non-executable",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4e_strategy_wires_diagnostics_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "StrategyDiagnosticsEngine",
        "EnableStrategyDiagnostics",
        "PrintStrategyDiagnostics",
        "DiagnosticsEveryClosedOutcomes",
        "MinimumClosedOutcomesForDiagnostics",
        "MinimumSetupOutcomesForDecision",
        "MinimumAverageRForSim101",
        "MinimumSetupAverageRToKeep",
        "Strategy diagnostics active",
        "UpdateStrategyDiagnostics",
        "DiagGrade=",
        "Sim101Eligible=",
        "STRATEGY_DIAGNOSTICS=",
        "SETUP_DIAGNOSTIC=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4e_docs_explain_diagnostics_are_non_executable() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "REPLAY_VALIDATION_CHECKLIST.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4e",
        "strategy diagnostics",
        "observation-only",
        "non-executable",
        "does not execute",
        "does not automatically change",
        "sim101eligible=false",
        "negative diagnostics",
        "before execution",
        "strategy_diagnostics",
        "setup_diagnostic",
    ):
        assert text in combined


def test_nt4f_strategy_filter_model_files_and_fields_exist() -> None:
    result = (NT_ROOT / "Models" / "StrategyFilterResult.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "IsAllowed",
        "IsFiltered",
        "FilterReason",
        "CandidateSetupType",
        "FilterProfile",
        "ConfirmationScore",
        "ConfirmationState",
        "ContextState",
        "LocationState",
        "ValueState",
        "CandidateRewardRisk",
        "ExecutionDisabled",
        "EvaluationOnlyMode",
    ):
        assert re.search(rf"\b{field}\s*\{{", result), field

    profile = (NT_ROOT / "Models" / "StrategyFilterProfile.cs").read_text(
        encoding="utf-8"
    )
    for item in (
        "Baseline",
        "DiagnosticV2",
        "StrictReplayValidation",
        "Custom",
    ):
        assert re.search(rf"\b{item}\b", profile), item


def test_nt4f_strategy_filter_engine_is_observation_only_and_filters_candidates() -> None:
    source = (NT_ROOT / "Core" / "StrategyFilterEngine.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "class StrategyFilterEngine",
        "StrategyFilterResult",
        "StrategyFilterProfile",
        "FILTERED_CANDIDATE=",
        "FILTER_SUMMARY=",
        "Evaluate",
        "PrintFilteredCandidate",
        "PrintFilterSummary",
        "TotalCandidatesSeen",
        "TotalCandidatesAllowed",
        "TotalCandidatesFiltered",
        "FilteredBySetupDisabled",
        "FilteredByConfirmation",
        "FilteredByScore",
        "FilteredByLocation",
        "FilteredByOrderFlowPressure",
        "AllowedByProfile",
        "DiagnosticV2",
        "StrictReplayValidation",
        "observation-only",
        "non-executable",
    ):
        assert text in source
    for forbidden in (
        "EnterLong",
        "SubmitOrder",
        "LongConfirmationPresent = true",
        "LongDecisionStatus.LongValid",
        "SignalConfirmed",
        "ConfirmedLong",
        "ExecutionReady",
        "OrderReady",
    ):
        assert forbidden not in source


def test_nt4f_strategy_wires_filter_layer_before_journal_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "StrategyFilterEngine",
        "StrategyFilterResult",
        "EnableStrategyFilterLayer",
        "StrategyFilterProfile",
        "PrintFilteredCandidates",
        "PrintFilterSummaryEveryBars",
        "V2AllowValueReclaimLong",
        "V2AllowDeviationRejectionLong",
        "V2AllowPullbackContinuationLong",
        "V2MinimumConfirmationScore",
        "V2RequireConfirmationObserved",
        "V2RejectStrongSellerPressure",
        "Strategy filter layer active",
        "ApplyStrategyFilter",
        "FILTERED_CANDIDATE=",
        "FILTER_SUMMARY=",
        "FilterProfile=",
        "FilteredCandidates=",
        "AllowedCandidates=",
    ):
        assert text in source
    assert source.index("ApplyStrategyFilter") < source.index("MaybeWriteSignalObservation")
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4f_docs_explain_filter_layer_is_non_executable() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "REPLAY_VALIDATION_CHECKLIST.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4f",
        "strategy filter",
        "diagnosticv2",
        "baseline profile",
        "observation-only",
        "non-executable",
        "does not execute",
        "does not prove profitability",
        "filtered_candidate",
        "filter_summary",
        "cleaner v2",
        "future sim101",
    ):
        assert text in combined


def test_nt4g_ict_market_model_files_and_states_exist() -> None:
    expected_states = {
        "HigherTimeframeBiasState.cs": (
            "Unknown",
            "StrongBullish",
            "Bullish",
            "Balanced",
            "Bearish",
            "StrongBearish",
        ),
        "MarketPhaseState.cs": (
            "Unknown",
            "Accumulation",
            "Manipulation",
            "Distribution",
            "Expansion",
            "Chop",
        ),
        "LiquiditySweepState.cs": (
            "None",
            "SellSideSweep",
            "BuySideSweep",
            "SellSideSweepAndReclaim",
            "BuySideSweepAndReject",
            "Unknown",
        ),
        "FairValueGapState.cs": (
            "None",
            "BullishFvg",
            "BearishFvg",
            "BullishFvgRetest",
            "BearishFvgRetest",
            "Filled",
            "Unknown",
        ),
        "DisplacementMomentumState.cs": (
            "None",
            "Weak",
            "BullishDisplacement",
            "BearishDisplacement",
            "StrongBullishDisplacement",
            "StrongBearishDisplacement",
        ),
        "OteZoneState.cs": (
            "Unknown",
            "NotInOte",
            "InDiscount",
            "InOteZone",
            "InPremium",
            "Invalid",
        ),
        "IctTargetQualityState.cs": (
            "Unknown",
            "Poor",
            "Acceptable",
            "Good",
            "Excellent",
        ),
    }
    for file_name, states in expected_states.items():
        source = (NT_ROOT / "Models" / file_name).read_text(encoding="utf-8")
        for state in states:
            assert re.search(rf"\b{state}\b", source), f"{file_name}:{state}"

    expected_snapshots = {
        "HigherTimeframeBiasSnapshot.cs": ("BiasState", "FastEma", "SlowEma", "SlowEmaSlope", "AllowsLongs"),
        "MarketPhaseSnapshot.cs": ("PhaseState", "RangeHigh", "RangeLow", "HasAccumulation", "HasManipulation"),
        "LiquiditySweepSnapshot.cs": ("SweepState", "SweptLevel", "HasSellSideSweep", "HasReclaim"),
        "FairValueGapSnapshot.cs": ("FvgState", "FvgHigh", "FvgLow", "FvgMidpoint", "HasBullishFvg"),
        "DisplacementMomentumSnapshot.cs": ("MomentumState", "BodyTicks", "BodyToRangeRatio", "HasBullishDisplacement"),
        "OteZoneSnapshot.cs": ("OteState", "DealingRangeLow", "DealingRangeHigh", "IsInOteZone"),
        "IctTargetQualitySnapshot.cs": ("TargetQualityState", "TargetPrice", "TargetRoomTicks", "HasExternalLiquidityTarget"),
    }
    for file_name, fields in expected_snapshots.items():
        source = (NT_ROOT / "Models" / file_name).read_text(encoding="utf-8")
        for field in fields:
            assert re.search(rf"\b{field}\s*\{{", source), f"{file_name}:{field}"


def test_nt4g_ict_evaluators_are_observation_only() -> None:
    for file_name in (
        "HigherTimeframeBiasEvaluator.cs",
        "AmdMarketPhaseEvaluator.cs",
        "LiquiditySweepEvaluator.cs",
        "FairValueGapEvaluator.cs",
        "DisplacementMomentumEvaluator.cs",
        "OteZoneEvaluator.cs",
        "IctTargetQualityEvaluator.cs",
    ):
        source = (NT_ROOT / "Core" / file_name).read_text(encoding="utf-8")
        assert "Evaluate" in source, file_name
        assert "observation-only" in source, file_name
        for forbidden in (
            "EnterLong",
            "SubmitOrder",
            "LongConfirmationPresent = true",
            "LongDecisionStatus.LongValid",
            "SignalConfirmed",
            "ConfirmedLong",
            "ExecutionReady",
            "OrderReady",
        ):
            assert forbidden not in source, f"{file_name}:{forbidden}"


def test_nt4g_filter_profile_and_engine_include_ict_quality_gate() -> None:
    profile = (NT_ROOT / "Models" / "StrategyFilterProfile.cs").read_text(
        encoding="utf-8"
    )
    assert "IctAmdLiquidityV1" in profile

    source = (NT_ROOT / "Core" / "StrategyFilterEngine.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "IctAmdLiquidityV1",
        "HigherTimeframeBiasSnapshot",
        "MarketPhaseSnapshot",
        "LiquiditySweepSnapshot",
        "FairValueGapSnapshot",
        "DisplacementMomentumSnapshot",
        "OteZoneSnapshot",
        "IctTargetQualitySnapshot",
        "QUALITY_GATE_PASSED=",
        "HtfRejected",
        "AmdRejected",
        "SweepRejected",
        "FvgRejected",
        "DisplacementRejected",
        "OteRejected",
        "TargetQualityRejected",
        "QualityGatePassed",
        "HTF bias does not allow longs",
        "No sell-side liquidity sweep",
        "No bullish FVG after displacement",
        "Entry is not in OTE/discount",
        "Target quality is poor",
    ):
        assert text in source


def test_nt4g_strategy_wires_ict_model_without_executing() -> None:
    source = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "HigherTimeframeBiasEvaluator",
        "AmdMarketPhaseEvaluator",
        "LiquiditySweepEvaluator",
        "FairValueGapEvaluator",
        "DisplacementMomentumEvaluator",
        "OteZoneEvaluator",
        "IctTargetQualityEvaluator",
        "BuildHigherTimeframeBiasSnapshot",
        "BuildMarketPhaseSnapshot",
        "BuildLiquiditySweepSnapshot",
        "BuildFairValueGapSnapshot",
        "BuildDisplacementMomentumSnapshot",
        "BuildOteZoneSnapshot",
        "BuildIctTargetQualitySnapshot",
        "EnableHigherTimeframeBiasFilter",
        "EnableAmdPhaseFilter",
        "EnableLiquiditySweepFilter",
        "EnableFairValueGapFilter",
        "EnableDisplacementFilter",
        "EnableOteFilter",
        "EnableIctTargetQualityFilter",
        "IctAmdLiquidityV1 available",
        "HTFBias=",
        "AMD=",
        "Sweep=",
        "FVG=",
        "Displacement=",
        "OTE=",
        "TargetQuality=",
        "QUALITY_GATE_PASSED=",
    ):
        assert text in source
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in source


def test_nt4g_docs_explain_ict_model_is_observation_only() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "REPLAY_VALIDATION_CHECKLIST.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "nt-4g",
        "ict-supported market model",
        "higher-timeframe bias",
        "accumulation",
        "liquidity sweep",
        "displacement",
        "fair value gap",
        "ote",
        "target quality",
        "order flow remains the final confirmation",
        "observation-only",
        "non-executable",
        "stable positive diagnostics",
    ):
        assert text in combined


def test_nt4g_revised_original_strategy_models_and_evaluators_exist() -> None:
    setup = (NT_ROOT / "Models" / "OriginalStrategySetupType.cs").read_text(
        encoding="utf-8"
    )
    for item in (
        "None",
        "ReturnPullbackToValue",
        "BreakoutPullbackFromValue",
        "ValueContinuation",
        "RotationalReversalFromDeviation",
        "Invalid",
    ):
        assert re.search(rf"\b{item}\b", setup), item

    roadmap = (NT_ROOT / "Models" / "ValueRoadmapSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "RoadmapState",
        "PrimaryTradeIdea",
        "HasClearRoadmap",
        "IsRotationalDayCandidate",
        "IsTrendDayCandidate",
        "SessionVwap",
        "DevelopingValueHigh",
        "DevelopingValueLow",
        "UpperDeviation15",
        "UpperDeviation20",
        "LowerDeviation15",
        "LowerDeviation20",
        "NearestCvaHigh",
        "NearestCvaLow",
        "TargetLevel1",
        "TargetLevel2",
        "TargetLevelFinal",
        "TargetReason",
    ):
        assert re.search(rf"\b{field}\s*\{{", roadmap), field

    acceptance_state = (NT_ROOT / "Models" / "ValueAcceptanceState.cs").read_text(
        encoding="utf-8"
    )
    for item in (
        "Unknown",
        "RejectedAboveValue",
        "RejectedBelowValue",
        "AcceptedInsideValue",
        "AcceptedAboveValue",
        "AcceptedBelowValue",
        "RotationalInsideValue",
        "NoAcceptance",
    ):
        assert re.search(rf"\b{item}\b", acceptance_state), item

    acceptance = (NT_ROOT / "Models" / "ValueAcceptanceSnapshot.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "AcceptanceState",
        "HasAcceptance",
        "HasRejection",
        "IsLongSupportive",
        "IsShortSupportive",
        "BarsAccepted",
        "AcceptedLevel",
        "Reason",
    ):
        assert re.search(rf"\b{field}\s*\{{", acceptance), field

    target_plan = (NT_ROOT / "Models" / "AdaptiveTargetPlan.cs").read_text(
        encoding="utf-8"
    )
    for field in (
        "HasTargetPlan",
        "EntryPrice",
        "StopPrice",
        "Target1",
        "Target2",
        "FinalTarget",
        "Target1Reason",
        "Target2Reason",
        "FinalTargetReason",
        "MoveStopToBreakevenAfterTarget1",
        "IsLogicalTargetPlan",
        "EstimatedRewardRiskToFinal",
        "Reason",
    ):
        assert re.search(rf"\b{field}\s*\{{", target_plan), field

    for file_name in (
        "ValueRoadmapEvaluator.cs",
        "ValueAcceptanceEvaluator.cs",
        "AdaptiveTargetPlanner.cs",
    ):
        source = (NT_ROOT / "Core" / file_name).read_text(encoding="utf-8")
        assert "Evaluate" in source or "Build" in source
        assert "observation-only" in source
        for forbidden in (
            "EnterLong",
            "SubmitOrder",
            "LongConfirmationPresent = true",
            "LongDecisionStatus.LongValid",
            "SignalConfirmed",
            "ConfirmedLong",
            "ExecutionReady",
            "OrderReady",
        ):
            assert forbidden not in source


def test_nt4g_revised_filter_profile_engine_and_strategy_wiring() -> None:
    profile = (NT_ROOT / "Models" / "StrategyFilterProfile.cs").read_text(
        encoding="utf-8"
    )
    assert "OriginalValueRoadmapV1" in profile

    engine = (NT_ROOT / "Core" / "StrategyFilterEngine.cs").read_text(
        encoding="utf-8"
    )
    for text in (
        "OriginalValueRoadmapV1",
        "ORIGINAL_STRATEGY_FILTERED=",
        "ORIGINAL_STRATEGY_CANDIDATE=",
        "ADAPTIVE_TARGET_PLAN=",
        "NoRoadmapRejected",
        "AcceptanceRejected",
        "OriginalSetupRejected",
        "NoTargetPlanRejected",
        "ChasingRejected",
        "OriginalStrategyPassed",
        "Order flow is the final confirmation",
    ):
        assert text in engine

    strategy = STRATEGY.read_text(encoding="utf-8")
    for text in (
        "ValueRoadmapEvaluator",
        "ValueAcceptanceEvaluator",
        "AdaptiveTargetPlanner",
        "BuildValueRoadmapSnapshot",
        "BuildValueAcceptanceSnapshot",
        "BuildAdaptiveTargetPlan",
        "EnableOriginalStrategyAlignment",
        "RequireRthSessionOnly",
        "RequireClearValueRoadmap",
        "RequireValueAcceptance",
        "RequireOriginalSetupType",
        "RequireLogicalValueTarget",
        "MinimumLogicalTargetRoomTicks",
        "MinOriginalConfirmationScore",
        "PrintOriginalStrategyEvents",
        "OriginalValueRoadmapV1 available",
        "Roadmap=",
        "Acceptance=",
        "OriginalSetup=",
        "TargetPlan=",
        "ORIGINAL_STRATEGY_FILTERED=",
        "ORIGINAL_STRATEGY_CANDIDATE=",
        "ADAPTIVE_TARGET_PLAN=",
    ):
        assert text in strategy
    for forbidden in (
        "LongConfirmationPresent = true",
        "LongValid",
        "LONG_VALID",
        "ExecutionReady",
        "OrderReady",
        "SignalConfirmed",
        "ConfirmedLong",
    ):
        assert forbidden not in strategy


def test_nt4g_revised_docs_explain_original_value_roadmap_strategy() -> None:
    combined = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (
            NT_ROOT / "README.md",
            NT_ROOT / "COMPILATION_CHECKLIST.md",
            NT_ROOT / "REPLAY_VALIDATION_CHECKLIST.md",
            ROOT / "docs" / "PLATFORM_DIRECTION.md",
        )
    ).lower()
    for text in (
        "originalvalueroadmapv1",
        "order flow is the last confirmation",
        "not ict",
        "value roadmap",
        "higher timeframe vwap",
        "rth",
        "composite value area",
        "return pullback",
        "breakout pullback",
        "continuation",
        "adaptive target",
        "not fixed rr",
        "observation-only",
        "non-executable",
    ):
        assert text in combined


def test_long_setup_enum_contains_only_approved_long_setups() -> None:
    source = (NT_ROOT / "Models" / "LongSetupType.cs").read_text(encoding="utf-8")
    block = re.search(r"enum\s+LongSetupType\s*\{(?P<body>.*?)\}", source, re.DOTALL)
    assert block is not None
    members = {
        member.strip().rstrip(",")
        for member in block.group("body").splitlines()
        if member.strip()
    }
    assert members == ALLOWED_SETUPS


def test_safety_guard_exposes_required_fail_closed_methods() -> None:
    source = (NT_ROOT / "Core" / "NinjaTraderSafetyGuards.cs").read_text(
        encoding="utf-8"
    )
    for method in (
        "IsLongSetupAllowed",
        "IsShortSetupForbidden",
        "IsExecutionAllowed",
        "IsEvaluationOnly",
        "IsLiveTradingForbidden",
        "ValidateLongOnlyDecision",
    ):
        assert re.search(rf"\b{method}\s*\(", source), method
    assert '"SHORT"' in source
    assert '"SELL_TO_OPEN"' in source
    assert re.search(r"IsExecutionAllowed\s*\(.*?\).*?return\s+false\s*;", source, re.DOTALL)


def test_all_ninjatrader_source_contains_no_execution_api_calls() -> None:
    combined = "\n".join(csharp_sources().values())
    for api in FORBIDDEN_EXECUTION_APIS:
        assert re.search(rf"\b{api}\s*\(", combined) is None, api
    assert "Account.CreateOrder" not in combined


def test_install_and_compilation_docs_cover_manual_gate() -> None:
    install = (NT_ROOT / "INSTALL_IN_NINJATRADER.md").read_text(encoding="utf-8")
    checklist = (NT_ROOT / "COMPILATION_CHECKLIST.md").read_text(encoding="utf-8")
    readme = (NT_ROOT / "README.md").read_text(encoding="utf-8")
    for text in (
        "NinjaScript Editor",
        "Output window",
        "EvaluationOnlyMode",
        "AllowLiveTrading",
    ):
        assert text in install
    for text in (
        "no compile errors",
        "strategy appears",
        "EvaluationOnlyMode is true",
        "AllowLiveTrading is false",
        "no orders appear",
    ):
        assert text.lower() in checklist.lower()
    assert "manual" in readme.lower() and "compil" in readme.lower()


def test_python_and_ibkr_reference_trees_are_preserved() -> None:
    assert (ROOT / "src" / "orderflow_ibkr_agent" / "decision_engine.py").is_file()
    assert (ROOT / "src" / "orderflow_ibkr_agent" / "paper" / "lifecycle.py").is_file()
    assert (ROOT / "src" / "orderflow_ibkr_agent" / "ibkr" / "adapter.py").is_file()
    assert (ROOT / "tests" / "test_decision_engine.py").is_file()
    assert (ROOT / "tests" / "test_ibkr_read_only_adapter.py").is_file()


def test_project_docs_state_nt1a_is_source_only_and_evaluation_only() -> None:
    root_readme = (ROOT / "README.md").read_text(encoding="utf-8")
    platform_doc = (ROOT / "docs" / "PLATFORM_DIRECTION.md").read_text(
        encoding="utf-8"
    )
    combined = root_readme + "\n" + platform_doc
    assert "NT-1A" in combined
    assert "evaluation-only" in combined.lower()
    assert "manual NinjaTrader" in combined
    assert "Python tests" in combined
    assert "experimental/deprecated" in combined.lower()
