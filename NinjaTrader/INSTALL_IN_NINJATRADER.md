# Install in NinjaTrader 8

NT-4G Revised is evaluation-only. It writes observation records, hypothetical outcome records, performance summary output, replay validation output, strategy diagnostics output, strategy filter output, original value-roadmap output, and optional ICT quality-gate output, not orders.

## 1. Back up custom NinjaScript

Before copying files, back up your NinjaTrader 8 `bin/Custom` directory or export your existing custom scripts.

## 2. Copy the source files

The usual Windows user directory is:

```text
Documents\NinjaTrader 8\bin\Custom
```

Copy files as follows:

```text
Repository                         NinjaTrader 8 user directory
NinjaTrader/Strategies/*.cs        -> bin/Custom/Strategies/
NinjaTrader/Models/*.cs            -> bin/Custom/OrderFlowAgent/Models/
NinjaTrader/Core/*.cs              -> bin/Custom/OrderFlowAgent/Core/
```

Create the `OrderFlowAgent/Models` and `OrderFlowAgent/Core` folders if they do not exist.

## 3. Compile

Open NinjaTrader 8, open the NinjaScript Editor, and compile. Python tests cannot prove NinjaTrader compilation because they do not load NinjaTrader assemblies.

## 4. Strategy settings

Confirm these before enabling:

- `EvaluationOnlyMode = true`
- `UseSimOnly = true`
- `AllowLiveTrading = false`
- `EnableSignalObservationJournal = true`
- `JournalOnlyConfirmedCandidates = false`
- `JournalOnlyWhenCandidateExists = true`
- `JournalFileName = orderflow_signal_observations.jsonl`
- `JournalCooldownBars = 5`
- `EnableHypotheticalOutcomeTracking = true`
- `TrackWeakConfirmations = false`
- `MaxBarsToTrackOutcome = 50`
- `ConservativeSameBarResolution = true`
- `EnablePerformanceSummary = true`
- `PrintPerformanceSummary = true`
- `PerformanceSummaryEveryClosedOutcomes = 25`
- `PrintSetupBreakdown = true`
- `EnableReplayValidation = true`
- `PrintReplayValidationSummary = true`
- `PrintReplayValidationEveryBars = 500`
- `MinimumClosedOutcomesForReview = 50`
- `MinimumBarsForReview = 500`
- `EnableStrategyDiagnostics = true`
- `PrintStrategyDiagnostics = true`
- `DiagnosticsEveryClosedOutcomes = 100`
- `MinimumClosedOutcomesForDiagnostics = 100`
- `MinimumSetupOutcomesForDecision = 20`
- `EnableStrategyFilterLayer = true`
- `StrategyFilterProfile = DiagnosticV2`
- `PrintFilteredCandidates = true`
- `PrintFilterSummaryEveryBars = 500`
- `V2MinimumConfirmationScore = 85`
- `EnableHigherTimeframeBiasFilter = true`
- `EnableAmdPhaseFilter = true`
- `EnableLiquiditySweepFilter = true`
- `EnableFairValueGapFilter = true`
- `EnableDisplacementFilter = true`
- `EnableOteFilter = true`
- `EnableIctTargetQualityFilter = true`
- `EnableOriginalStrategyAlignment = true`
- `RequireRthSessionOnly = true`
- `RequireClearValueRoadmap = true`
- `RequireValueAcceptance = true`
- `RequireOriginalSetupType = true`
- `RequireLogicalValueTarget = true`

## 5. Expected Output window output

Startup output should include:

```text
EnableSignalObservationJournal=True
JournalOnlyConfirmedCandidates=False
JournalOnlyWhenCandidateExists=True
JournalFileName=orderflow_signal_observations.jsonl
JournalCooldownBars=5
Signal journal active=True
EnableHypotheticalOutcomeTracking=True
TrackWeakConfirmations=False
MaxBarsToTrackOutcome=50
ConservativeSameBarResolution=True
Outcome tracking active=True
EnablePerformanceSummary=True
PerformanceSummaryEveryClosedOutcomes=25
PrintSetupBreakdown=True
TimeoutResultR=0
InvalidatedResultR=0
DefaultTargetRewardR=2
Performance summary active=True
EnableReplayValidation=True
PrintReplayValidationSummary=True
PrintReplayValidationEveryBars=500
MinimumClosedOutcomesForReview=50
MinimumBarsForReview=500
Replay validation active=True
EnableStrategyDiagnostics=True
PrintStrategyDiagnostics=True
DiagnosticsEveryClosedOutcomes=100
MinimumClosedOutcomesForDiagnostics=100
MinimumSetupOutcomesForDecision=20
MinimumAverageRForSim101=0.05
Strategy diagnostics active=True
EnableStrategyFilterLayer=True
StrategyFilterProfile=DiagnosticV2
PrintFilteredCandidates=True
PrintFilterSummaryEveryBars=500
V2MinimumConfirmationScore=85
V2RequireConfirmationObserved=True
V2RejectStrongSellerPressure=True
Strategy filter layer active=True
EnableHigherTimeframeBiasFilter=True
EnableAmdPhaseFilter=True
EnableLiquiditySweepFilter=True
EnableFairValueGapFilter=True
EnableDisplacementFilter=True
EnableOteFilter=True
EnableIctTargetQualityFilter=True
IctAmdLiquidityV1 available
EnableOriginalStrategyAlignment=True
RequireRthSessionOnly=True
RequireClearValueRoadmap=True
RequireValueAcceptance=True
RequireOriginalSetupType=True
RequireLogicalValueTarget=True
MinimumLogicalTargetRoomTicks=20
MinOriginalConfirmationScore=85
OriginalValueRoadmapV1 available
Execution enabled: false
NO_EXECUTION_ENABLED=true
```

Heartbeat output should include:

```text
Journal=Enabled Journaled=True OpenOutcomes=1 PerfTotal=0 PerfWinRate=0 PerfAvgR=0 ReplayBars=500 Reviewable=False DiagGrade=NotRun Sim101Eligible=False Roadmap=BreakoutPullbackToUpperValue Acceptance=AcceptedAboveValue OriginalSetup=BreakoutPullbackFromValue TargetPlan=True FilterProfile=OriginalValueRoadmapV1 FilteredCandidates=10 AllowedCandidates=2
```

If file writing fails, the strategy should print:

```text
SIGNAL_OBSERVATION_JSON=
```

Hypothetical outcome tracking may also write `OUTCOME_OPENED` and `OUTCOME_CLOSED` records or print:

```text
HYPOTHETICAL_OUTCOME_OPENED=
HYPOTHETICAL_OUTCOME_CLOSED=
```

This is observation-only and does not execute trades. If both a target and stop are touched inside the same bar, the default same-bar behavior is conservative and assumes the stop happened first. These records are not proof of profitability.

After enough closed hypothetical outcomes, performance summary output may appear:

```text
PERFORMANCE_SUMMARY=
SETUP_STATS=
```

Replay validation output should appear as:

```text
REPLAY_VALIDATION_PROGRESS=
REPLAY_VALIDATION_SUMMARY=
```

Strategy diagnostics output should appear as:

```text
STRATEGY_DIAGNOSTICS=
SETUP_DIAGNOSTIC=
```

Strategy filter output should appear as:

```text
FILTERED_CANDIDATE=
FILTER_SUMMARY=
QUALITY_GATE_PASSED=
ORIGINAL_STRATEGY_FILTERED=
ORIGINAL_STRATEGY_CANDIDATE=
ADAPTIVE_TARGET_PLAN=
```

These stats are hypothetical, based on candidate estimates and not real fills. They are useful for identifying setup types for further testing, but they are not proof of profitability. The original strategy is not ICT; order flow is the last confirmation after the higher timeframe VWAP/RTH value roadmap, composite value area, developing value/VWAP/deviation context, return pullback, breakout pullback, continuation, and adaptive target plan. Execution remains disabled. Later NT-5A may add Sim101 paper execution only after OriginalValueRoadmapV1 has stable positive diagnostics across enough sessions.

If any order appears, disable/remove the strategy immediately. NT-4G has no intended execution behavior.
