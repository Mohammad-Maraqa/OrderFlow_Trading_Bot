# NinjaTrader 8 long-only agent

This folder contains the compile-oriented NinjaScript source for the long-only order-flow agent. The strategy is evaluation-only: it evaluates context, value/session structure, long setup candidates, approximate order-flow features, approximate confirmation classification, signal observation journaling, NT-4B hypothetical outcome tracking, NT-4C performance summary reporting, NT-4D replay validation, NT-4E strategy diagnostics, NT-4F strategy filter profiles, and the NT-4G ICT-supported market model while keeping execution disabled.

It has no order execution path.

## Repository layout

```text
NinjaTrader/
|-- Strategies/
|   `-- LongOnlyOrderFlowAgentStrategy.cs
|-- Models/
|   |-- OrderFlowFeatureSnapshot.cs
|   |-- LongSetupType.cs
|   |-- ContextFeatureSnapshot.cs
|   |-- SessionStructureSnapshot.cs
|   |-- LongSetupCandidateSnapshot.cs
|   |-- OrderFlowConfirmationSnapshot.cs
|   |-- SignalObservationRecord.cs
|   |-- HypotheticalSignalOutcome.cs
|   |-- HypotheticalOutcomeState.cs
|   |-- HypotheticalPerformanceSummary.cs
|   |-- SetupPerformanceStats.cs
|   |-- ReplayValidationSession.cs
|   |-- ReplayValidationSummary.cs
|   |-- StrategyDiagnosticSummary.cs
|   |-- SetupDiagnosticResult.cs
|   |-- StrategyFilterResult.cs
|   |-- StrategyFilterProfile.cs
|   |-- HigherTimeframeBiasSnapshot.cs
|   |-- MarketPhaseSnapshot.cs
|   |-- LiquiditySweepSnapshot.cs
|   |-- FairValueGapSnapshot.cs
|   |-- DisplacementMomentumSnapshot.cs
|   |-- OteZoneSnapshot.cs
|   `-- IctTargetQualitySnapshot.cs
|-- Core/
|   |-- LongOnlyOrderFlowEvaluator.cs
|   |-- NinjaTraderSafetyGuards.cs
|   |-- MarketContextEvaluator.cs
|   |-- SessionStructureEvaluator.cs
|   |-- LongSetupCandidateEvaluator.cs
|   |-- OrderFlowFeatureEvaluator.cs
|   |-- OrderFlowConfirmationEvaluator.cs
|   |-- SignalObservationJournalWriter.cs
|   |-- HypotheticalOutcomeTracker.cs
|   |-- HypotheticalPerformanceTracker.cs
|   |-- ReplayValidationTracker.cs
|   |-- StrategyDiagnosticsEngine.cs
|   |-- StrategyFilterEngine.cs
|   |-- HigherTimeframeBiasEvaluator.cs
|   |-- AmdMarketPhaseEvaluator.cs
|   |-- LiquiditySweepEvaluator.cs
|   |-- FairValueGapEvaluator.cs
|   |-- DisplacementMomentumEvaluator.cs
|   |-- OteZoneEvaluator.cs
|   `-- IctTargetQualityEvaluator.cs
|-- REPLAY_VALIDATION_CHECKLIST.md
|-- INSTALL_IN_NINJATRADER.md
`-- COMPILATION_CHECKLIST.md
```

## Safety state

- `EvaluationOnlyMode` defaults to `true`.
- `UseSimOnly` defaults to `true`.
- `AllowLiveTrading` defaults to `false`.
- The execution guard always returns false.
- `OnBarUpdate` evaluates, logs, and journals observations only.
- Hypothetical outcome tracking is observation-only and does not execute trades.
- Performance summary reporting is observation-only and does not execute trades.
- Replay validation is observation-only, non-executable, and does not execute trades.
- Strategy diagnostics are observation-only, non-executable, and do not automatically change setup rules.
- Strategy filters are observation-only, non-executable, and only suppress journal/outcome tracking candidates.
- The ICT-supported market model is observation-only, non-executable, and only adds higher-quality candidate gates.
- No orders should appear in Sim101, live accounts, Orders, or Executions.

## NT-4A signal observation journal

NT-4A writes structured observation records when meaningful long candidate/confirmation events appear.

Journal records include:

- instrument, bar, timestamp, and price
- context and VWAP location
- approximate POC/VAH/VAL and value state
- candidate setup type/state/reason
- approximate order-flow bias, delta, CVD, and high-volume flag
- confirmation type/state/score/reason
- `ExecutionDisabled`
- `EvaluationOnlyMode`
- final non-executable decision state

The default file name is:

```text
orderflow_signal_observations.jsonl
```

If file writing fails, the strategy prints a fallback line to NinjaScript Output with:

```text
SIGNAL_OBSERVATION_JSON=
```

The journal is observation-only. A journal record does not mean the strategy is profitable, does not mean a trade was executed, and does not submit orders.

## NT-4B hypothetical outcome tracking

NT-4B adds hypothetical outcome tracking for journaled long setup observations. It asks: if a journaled confirmed candidate had been treated as a long signal, would the estimated target, stop, timeout, or invalidation have happened?

This does not execute trades, does not use Sim101, does not place broker orders, and does not create any executable signal state. It only writes or prints observation records.

Defaults:

- `EnableHypotheticalOutcomeTracking = true`
- `TrackWeakConfirmations = false`
- `MaxBarsToTrackOutcome = 50`
- `ConservativeSameBarResolution = true`
- `PrintOutcomeEvents = true`
- `PrintOpenOutcomeCountEveryHeartbeat = true`

When both target and stop are touched in the same bar, the default same-bar behavior is conservative: the tracker assumes `StopHit` first. This is intentional because bar data cannot prove the intrabar sequence.

Outcome events are written to the same JSONL observation file with `OUTCOME_OPENED` and `OUTCOME_CLOSED` event types. If file writing fails, the strategy prints fallback lines:

```text
HYPOTHETICAL_OUTCOME_OPENED=
HYPOTHETICAL_OUTCOME_CLOSED=
```

These results are approximate because entries, stops, and targets are still candidate estimates. NT-4B starts producing evidence for review, but it is not proof of profitability. Later phases should add summary statistics and replay validation reporting.

## NT-4C performance summary

NT-4C adds performance summary reporting for closed hypothetical outcomes. It answers how many hypothetical signals were tracked, how many hit target/stop/timeout, win rate, average R, total R, and which setup types are currently best or worst.

These stats are hypothetical and based on candidate estimates, not real fills. They do not execute, do not submit orders, and are not proof of profitability. Their purpose is to identify which setup types deserve further testing.

Defaults:

- `EnablePerformanceSummary = true`
- `PrintPerformanceSummary = true`
- `PerformanceSummaryEveryClosedOutcomes = 25`
- `PrintSetupBreakdown = true`
- `TimeoutResultR = 0.0`
- `InvalidatedResultR = 0.0`
- `DefaultTargetRewardR = 2.0`

Every `PerformanceSummaryEveryClosedOutcomes` closed outcomes, the strategy prints:

```text
PERFORMANCE_SUMMARY=Total=100 TargetHits=42 StopHits=48 Timeouts=10 WinRate=42 AvgR=0.14 TotalR=14 BestSetup=FAILED_BREAKDOWN_LONG WorstSetup=BREAKOUT_PULLBACK_LONG
```

If `PrintSetupBreakdown=true`, it also prints:

```text
SETUP_STATS=Setup=FAILED_BREAKDOWN_LONG Total=30 WinRate=53.3 AvgR=0.42 TotalR=12.6
```

## NT-4D replay validation

NT-4D adds a replay validation workflow and structured Output lines for exportable review. It tracks replay session metadata, bars processed, journaled candidates, confirmation counts, closed hypothetical outcomes, performance summary values, and whether the session is reviewable under minimum sample thresholds.

Defaults:

- `EnableReplayValidation = true`
- `PrintReplayValidationSummary = true`
- `PrintReplayValidationEveryBars = 500`
- `MinimumClosedOutcomesForReview = 50`
- `MinimumBarsForReview = 500`
- `ReplaySessionLabel =`

Every `PrintReplayValidationEveryBars` bars, the strategy prints:

```text
REPLAY_VALIDATION_PROGRESS=SessionId=... Instrument=... Bars=... JournaledCandidates=... Confirmed=... Weak=... NoConfirmation=... ClosedOutcomes=... WinRate=... AvgR=... Reviewable=True
```

On termination/disable when NinjaTrader provides the hook, it prints a best-effort final summary:

```text
REPLAY_VALIDATION_SUMMARY=SessionId=... Instrument=... Bars=... Candidates=... Confirmed=... Weak=... NoConfirmation=... ClosedOutcomes=... TargetHits=... StopHits=... Timeouts=... WinRate=... AvgR=... TotalR=... BestSetup=... WorstSetup=... Reviewable=False Warnings=...
```

If the termination hook is unreliable, disable/reload the strategy and review the periodic `REPLAY_VALIDATION_PROGRESS=` lines. NT-4D does not execute, does not prove profitability, and does not authorize Sim101. Later NT-5A may add Sim101 paper execution only after replay validation is acceptable.

## NT-4E strategy diagnostics

NT-4E adds strategy diagnostics and setup-level recommendations. Diagnostics help decide whether replay evidence is strong enough to even consider Sim101, and they identify weak setup types that should be disabled, tightened, or kept under review in the next replay test.

Diagnostics do not execute trades, do not automatically change strategy rules, and do not enable or disable setup properties. Negative diagnostics mean the strategy rules should be refined before execution.

Defaults:

- `EnableStrategyDiagnostics = true`
- `PrintStrategyDiagnostics = true`
- `DiagnosticsEveryClosedOutcomes = 100`
- `MinimumClosedOutcomesForDiagnostics = 100`
- `MinimumSetupOutcomesForDecision = 20`
- `MinimumAverageRForSim101 = 0.05`
- `MinimumSetupAverageRToKeep = 0.0`

Every `DiagnosticsEveryClosedOutcomes` closed hypothetical outcomes, the strategy prints:

```text
STRATEGY_DIAGNOSTICS=Total=1415 WinRate=14.91 AvgR=-0.293 TotalR=-415 Grade=NegativeExpectancy EligibleForSim101=False PrimaryProblem=Poor signal quality / too many weak candidates RecommendedAction=Do not proceed to Sim101. Disable or tighten worst setups first.
```

For each setup with stats, it also prints:

```text
SETUP_DIAGNOSTIC=Setup=PULLBACK_CONTINUATION_LONG Total=... WinRate=... AvgR=... TotalR=... Action=Disable Reason=Worst setup and negative expectancy.
```

Sim101 should only be enabled in a later approved phase after replay validation and diagnostics are acceptable. A diagnostic line such as `Sim101Eligible=False` or `EligibleForSim101=False` is a stop sign for execution work.

## NT-4F strategy filter profiles

NT-4F adds an observation-only strategy filter layer used to test a cleaner V2 strategy against the baseline. Filters run after candidate detection and confirmation, but before signal journaling and hypothetical outcome tracking. A filtered candidate is logged and suppressed from the journal/outcome/performance pipeline; it does not become executable.

Use the `Baseline` profile to compare original behavior. Use the `DiagnosticV2` profile to test the diagnostics-based filtered version. `StrictReplayValidation` applies the V2 idea more tightly, while `Custom` lets the individual V2 settings drive the test.

Defaults:

- `EnableStrategyFilterLayer = true`
- `StrategyFilterProfile = DiagnosticV2`
- `PrintFilteredCandidates = true`
- `PrintFilterSummaryEveryBars = 500`
- `V2AllowBreakoutPullbackLong = true`
- `V2AllowFailedBreakdownLong = true`
- `V2AllowValueReclaimLong = false`
- `V2AllowDeviationRejectionLong = false`
- `V2AllowPullbackContinuationLong = false`
- `V2MinimumConfirmationScore = 85`
- `V2MinimumRewardRisk = 2.0`
- `V2RequireConfirmationObserved = true`
- `V2RejectStrongSellerPressure = true`

When a candidate is suppressed, the strategy prints:

```text
FILTERED_CANDIDATE=Candidate=VALUE_RECLAIM_LONG Reason=Setup disabled by DiagnosticV2 Profile=DiagnosticV2 ConfirmationState=ConfirmationObserved Score=85 Context=ExtendedBearish Location=BelowLowerDeviation ValueState=InsideValue RR=2
```

Every `PrintFilterSummaryEveryBars` bars, it prints:

```text
FILTER_SUMMARY=Profile=DiagnosticV2 Seen=100 Allowed=25 Filtered=75 SetupDisabled=50 Confirmation=12 Score=8 Location=4 OrderFlow=1
```

Filters do not execute trades, do not prove profitability, and do not change setup enable properties automatically. If `DiagnosticV2` improves replay stats, only then should a future Sim101 phase be considered.

## NT-4G Original Strategy Alignment

NT-4G Revised pauses the ICT expansion as the primary framework and realigns the bot to the original value-roadmap strategy. The original strategy is not ICT/SMC/pattern-based. Order flow is the last confirmation step, not the strategy itself.

The primary roadmap is value based:

- higher timeframe VWAP context
- RTH TPO/value roadmap
- composite value area levels
- developing value area, VWAP, and deviation bands
- return pullback, breakout pullback, and continuation setup language
- order-flow confirmation as the final confirmation
- adaptive target planning at logical value/VWAP/CVA/deviation levels

`OriginalValueRoadmapV1` is the recommended profile for the main strategy test. It rejects candidates with no clear value roadmap, no value acceptance/rejection, no original setup mapping, weak order-flow confirmation, no logical target, or target room that is too small.

Targets are logical/adaptive, not fixed RR. The strategy prints:

```text
ORIGINAL_STRATEGY_FILTERED=
ORIGINAL_STRATEGY_CANDIDATE=
ADAPTIVE_TARGET_PLAN=
```

This remains observation-only and non-executable.

## NT-4G ICT-Supported Market Model

NT-4G adds the user's ICT-supported market model as an observation-only quality gate. Higher-timeframe bias gives direction. Accumulation/manipulation/distribution gives phase. A sell-side liquidity sweep provides the setup fuel. Reclaim, bullish displacement, and a bullish fair value gap confirm imbalance. OTE provides entry location. Target quality requires a meaningful buy-side liquidity objective. Order flow remains the final confirmation layer.

The new `IctAmdLiquidityV1` profile requires:

- HTF bias allows longs
- accumulation/manipulation/distribution is valid or developing
- sell-side liquidity was swept and reclaimed
- bullish displacement appeared after the sweep
- bullish fair value gap formed after displacement
- entry is in OTE or discount
- order-flow confirmation is observed with score at least 85
- target quality is acceptable or better
- reward/risk meets the target-quality threshold

When the ICT gate passes, the strategy prints:

```text
QUALITY_GATE_PASSED=Candidate=... Profile=IctAmdLiquidityV1 HTFBias=Bullish AMD=Manipulation Sweep=SellSideSweepAndReclaim FVG=BullishFvg Displacement=StrongBullishDisplacement OTE=InOteZone TargetQuality=Good Score=85 RR=2.5
```

When it rejects a candidate, `FILTERED_CANDIDATE=` includes the ICT quality-gate reason. `FILTER_SUMMARY=` includes `HtfRejected`, `AmdRejected`, `SweepRejected`, `FvgRejected`, `DisplacementRejected`, `OteRejected`, `TargetQualityRejected`, and `QualityGatePassed`.

This is not Sim101, does not execute trades, and does not prove profitability. Sim101 should only be considered after `IctAmdLiquidityV1` shows stable positive diagnostics across enough sessions.

## Runtime logging

Heartbeat output includes journal status:

```text
Journal=Enabled Journaled=True OpenOutcomes=1 ClosedOutcomes=0 LastOutcome=None PerfTotal=0 PerfWinRate=0 PerfAvgR=0 ReplayBars=500 Reviewable=False DiagGrade=NotRun Sim101Eligible=False HTFBias=Bullish AMD=Manipulation Sweep=SellSideSweepAndReclaim FVG=BullishFvg Displacement=StrongBullishDisplacement OTE=InOteZone TargetQuality=Good FilterProfile=DiagnosticV2 FilteredCandidates=10 AllowedCandidates=2
```

Example heartbeat:

```text
LongOnlyOrderFlowAgentStrategy heartbeat: CurrentBar=500 Price=5825.25 VWAP=5819.75 Context=Bullish Location=AboveVwap POC=5822 VAH=5830 VAL=5812 ValueState=InsideValue Candidate=PULLBACK_CONTINUATION_LONG CandidateState=WaitingForConfirmation RR=2.1 OFBias=BuyerPressure Delta=1200 CVD=8500 HighVol=False Confirmation=BuyerPressureConfirmation ConfirmationState=ConfirmationObserved Score=75 Journal=Enabled Journaled=True OpenOutcomes=3 ClosedOutcomes=100 LastOutcome=TargetHit PerfTotal=100 PerfWinRate=42 PerfAvgR=0.14 ReplayBars=500 Reviewable=True DiagGrade=NeedsReview Sim101Eligible=False FilterProfile=DiagnosticV2 FilteredCandidates=420 AllowedCandidates=85 State=DataMissing Decision=DataMissing ExecutionDisabled=True
```

## Manual compilation

NinjaTrader assemblies are unavailable in this repository environment. Python tests verify layout, namespaces, defaults, and source safety, but do not prove NinjaScript compilation.

Follow [INSTALL_IN_NINJATRADER.md](INSTALL_IN_NINJATRADER.md), then complete [COMPILATION_CHECKLIST.md](COMPILATION_CHECKLIST.md) inside NinjaTrader 8.
