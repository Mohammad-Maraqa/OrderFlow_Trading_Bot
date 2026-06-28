# Platform Direction

## Decision

NinjaTrader 8 is now the primary platform direction for chart integration, order-flow data access, Market Replay validation, and -- only after separate approval -- execution design.

The Python project remains the strategy reference. It owns deterministic specification, strict input validation, sample fixtures, regression tests, audit/journal formats, and offline paper-lifecycle examples.

## Current milestone: NT-4G

NT-4G Revised realigns the bot to the original value-roadmap strategy before any ICT expansion. The original strategy is not ICT/SMC/pattern-based. Order flow is the last confirmation step. The main strategy profile is now `OriginalValueRoadmapV1`, which uses higher timeframe VWAP context, RTH value/TPO roadmap, composite value area levels, developing VWAP/value/deviation bands, return pullback, breakout pullback, continuation, order-flow confirmation, and adaptive target planning.

Journal records, hypothetical outcomes, performance stats, replay validation summaries, strategy diagnostics, filter summaries, original roadmap outputs, and ICT quality-gate outputs are for later validation and replay review. They are hypothetical, not real fills, not trades, and they do not prove profitability.

The strategy remains evaluation-only:

- no NinjaTrader execution APIs
- no Sim101 orders
- no live orders
- no executable signal state
- no `LongConfirmationPresent=true`
- hypothetical outcome tracking is observation-only and does not execute trades
- performance summary reporting is observation-only and does not execute trades
- replay validation is observation-only, non-executable, and does not execute trades
- strategy diagnostics are observation-only, non-executable, and do not automatically change setup rules
- strategy filters are observation-only, non-executable, and only suppress candidates from journal/outcome tracking
- the ICT-supported market model is observation-only, non-executable, and order flow remains the final confirmation
- OriginalValueRoadmapV1 is observation-only, non-executable, and targets are logical/adaptive value levels, not fixed RR

Same-bar target/stop handling is conservative by default. If both levels are touched in one bar, NT-4B assumes the stop happened first because bar data cannot prove sequence.

NT-4D prints `REPLAY_VALIDATION_PROGRESS=` and best-effort `REPLAY_VALIDATION_SUMMARY=` lines so multiple Market Replay or historical sessions can be reviewed consistently. NT-4E prints `STRATEGY_DIAGNOSTICS=` and `SETUP_DIAGNOSTIC=` lines so weak setup types and Sim101 readiness can be reviewed. NT-4F prints `FILTERED_CANDIDATE=` and `FILTER_SUMMARY=` lines so the Baseline profile can be compared with filtered profiles. NT-4G Revised prints `ORIGINAL_STRATEGY_FILTERED=`, `ORIGINAL_STRATEGY_CANDIDATE=`, and `ADAPTIVE_TARGET_PLAN=` so the original value roadmap can be reviewed. Positive stats still do not authorize execution, and `Sim101Eligible=False` means strategy rules should be refined before execution.

## Deferred work

- True NinjaTrader Volumetric Bars / Order Flow+ bid-ask data wiring.
- Native Order Flow+ VWAP/Volume Profile replacement or validation.
- Market Replay comparison against Python fixtures.
- NinjaTrader internal simulation lifecycle.
- Any broker, Sim101, or live execution capability.
- NT-5A Sim101 paper execution may be considered only after replay validation, diagnostics, and IctAmdLiquidityV1 has stable positive diagnostics across enough sessions.

Python tests provide source-safety evidence only; they cannot load NinjaTrader assemblies or certify compilation. The manual NinjaTrader installation and compilation checklist is required before moving to native feature work.
