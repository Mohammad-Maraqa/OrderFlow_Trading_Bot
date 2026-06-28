# NT-4D Replay Validation Checklist

Use this checklist for every NinjaTrader Market Replay or historical chart validation session. NT-4D is observation-only and non-executable; it collects evidence, not trades.

## Run a Session

1. Open a NinjaTrader 8 chart or Market Replay connection.
2. Use the same instrument, timeframe, and trading hours template for comparable sessions.
3. Recommended starting point: ES or MES, 1-minute to 5-minute bars, regular trading hours unless the test plan says otherwise.
4. Add `LongOnlyOrderFlowAgentStrategy`.
5. Confirm `EvaluationOnlyMode = true`, `AllowLiveTrading = false`, `UseSimOnly = true`, and `EnableReplayValidation = true`.
6. Set `ReplaySessionLabel` to a date/session label when useful.
7. Enable the strategy and let the full planned session run.
8. Disable or reload the strategy at the end so the best-effort final `REPLAY_VALIDATION_SUMMARY=` can print.

## Minimum Sample

- Test at least 20 sessions before judging the strategy.
- Wait for at least 50 to 100 closed hypothetical outcomes before drawing conclusions.
- Treat sessions below `MinimumBarsForReview` or `MinimumClosedOutcomesForReview` as not reviewable.

## Output to Collect

Save the NinjaScript Output lines for every tested session:

- `PERFORMANCE_SUMMARY=`
- `SETUP_STATS=`
- `REPLAY_VALIDATION_PROGRESS=`
- `REPLAY_VALIDATION_SUMMARY=`
- `STRATEGY_DIAGNOSTICS=`
- `SETUP_DIAGNOSTIC=`
- `FILTERED_CANDIDATE=`
- `FILTER_SUMMARY=`

Also record instrument, date, timeframe, trading hours template, replay or historical mode, market condition, and any changed strategy settings.

For NT-4F, run comparable sessions with the `Baseline` profile and the `DiagnosticV2` profile. The baseline profile shows original behavior; DiagnosticV2 tests the cleaner V2 candidate stream recommended by diagnostics.

For NT-4G, add a comparable `IctAmdLiquidityV1` run. This profile tests the ICT-supported market model: higher-timeframe bias, accumulation, sell-side liquidity sweep, reclaim, displacement, fair value gap, OTE/discount entry, target quality, and order flow remains the final confirmation.

For NT-4G Revised, prioritize `OriginalValueRoadmapV1`. The original strategy is not ICT; it is a value roadmap: higher timeframe VWAP context, RTH TPO/value, composite value area, developing VWAP/value/deviation bands, return pullback, breakout pullback, continuation, order-flow confirmation, and adaptive target planning. Order flow is the last confirmation, and targets are logical/adaptive rather than fixed RR.

## Avoid Cherry-Picking

- Log every tested session.
- Do not skip bad sessions.
- Record the market condition before reviewing the results.
- Record the date and time of the replay session.
- Do not discard sessions because the stats look poor.
- Do not let one unusually lucky session dominate the conclusion.

## Sim101 Readiness Gate

Replay validation does not prove profitability. A later NT-5A Sim101 paper-execution phase should only be considered if the replay evidence shows:

- positive average R over enough samples
- acceptable max losing streak if available
- setup-level consistency across more than one setup or session
- no single lucky session dominates total R
- enough sample size across at least 20 sessions and 50 to 100 closed hypothetical outcomes
- NT-4E strategy diagnostics are acceptable and do not report `Sim101Eligible=False`

NT-4E strategy diagnostics are observation-only and non-executable. They recommend which setups to disable, tighten, or keep testing, but they do not automatically change setup rules. Negative diagnostics mean strategy rules should be refined before execution.

NT-4F strategy filters are observation-only and non-executable. They suppress low-quality candidates from journal/outcome/performance tracking so a cleaner V2 strategy can be compared against baseline. Filters do not execute trades and do not prove profitability.

NT-4G is observation-only and non-executable. It does not use Sim101 and does not place orders. Sim101 should only be considered after IctAmdLiquidityV1 has stable positive diagnostics across enough sessions.

OriginalValueRoadmapV1 must also remain observation-only and non-executable. Sim101 should only be considered after original-roadmap replay validation has stable positive diagnostics across enough sessions.

Do not enable orders from NT-4D, NT-4E, or NT-4F. They must not produce `LONG_VALID`, `LongValid`, `SignalConfirmed`, `ExecutionReady`, `OrderReady`, or any order submission. Future Sim101 should only be considered if replay validation, diagnostics, and DiagnosticV2 filter comparisons are acceptable.
