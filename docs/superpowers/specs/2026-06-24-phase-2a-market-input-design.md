# Phase 2A Market Input Design

## Boundary

Phase 2A adds an external-source contract without replacing Phase 1's internal engine models. `MarketDecisionInput` represents normalized data from JSON, TradingView, CSV exports, or later feeds. A deterministic adapter translates that contract into the existing `MarketSnapshot`, internal `OrderFlowSnapshot`, and `RiskPlan` types.

This avoids coupling source-oriented field names to strategy implementation details and preserves the Phase 1 API.

## Validation

The external models use strict enums for session, asset type, context, market state, tape speed, confidence, and the existing five-value `TradeSetupType`. Prices, tick size, point value, and volumes are constrained at model construction. Cross-field validators enforce long geometry (`stop < entry < target`), value-area ordering, VWAP-band ordering, and composite-value ordering.

Any unrecognized setup—including every short-side label—is rejected because `setup_type` is the existing long-only enum. JSON syntax, missing files, and Pydantic validation errors are exposed through one readable `MarketInputError` boundary.

## Decision Flow

`evaluate-json` loads and validates a file, adapts it to engine inputs, and calls `DecisionEngine.evaluate`. The candidate's entry becomes the decision snapshot's current price; metadata may override `min_reward_risk_ratio` and `max_risk_points`, otherwise safe defaults are used.

Complete data with no detected absorption, defense, reclaim, or sufficient order-flow confidence remains structurally valid and returns `WAITING_FOR_CONFIRMATION`. Invalid JSON, missing fields, forbidden setup values, and invalid long geometry fail before the engine runs.

The adapter checks that a valid engine classification matches the declared candidate setup. A mismatch becomes `NO_TRADE`, preventing an upstream source from labeling one setup while supplying evidence for another.

## CLI and Journal

`orderflow-agent evaluate-json INPUT --journal-path PATH` prints symbol, decision, setup, confidence, prices, reasons, and warnings. Every successfully evaluated input—including rejected/no-trade decisions—is appended to JSONL. Validation failures do not create journal entries.

The default journal path is `journal/decisions.jsonl`. No code path imports an IBKR SDK, connects to TWS/Gateway, or submits an order.

## Testing

Tests cover valid loading, missing fields, short labels, invalid prices and geometry, unconfirmed order flow, CLI evaluation, JSONL output, all five supplied sample files, Phase 1 regression tests, CLI regressions, and wheel construction.

