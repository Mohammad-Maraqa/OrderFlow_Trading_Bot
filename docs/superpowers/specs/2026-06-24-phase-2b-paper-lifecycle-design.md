# Phase 2B Offline Paper Lifecycle Design

## Scope and safety boundary

Phase 2B consumes only a validated `MarketDecisionInput` and its deterministic `TradeDecision`. It does not import an IBKR client, open a socket, place an order, or simulate broker-specific fills. A paper entry is an immediate fake fill at the decision entry price.

Only `LONG_VALID` decisions with an allowed `TradeSetupType`, `stop < entry < target`, and positive quantity may open a position. Entry orders are `BUY`; lifecycle exits are `SELL_TO_CLOSE`. No model accepts a short-side value.

## Models

`PaperOrder`, `PaperPosition`, and `PaperTradeRecord` use strict Pydantic validation and explicit enums. A closed position/record must include its close timestamp, price, reason, realized PnL, and realized R. An open position/record cannot contain close results.

`point_value` is persisted with positions and records as an internal accounting field sourced from `InstrumentSnapshot`. This is necessary to compute currency PnL for futures and other non-unit instruments:

```text
realized_pnl = (close_price - entry_price) * quantity * point_value
realized_r = (close_price - entry_price) / (entry_price - stop_price)
```

## Persistence and journal

`data/paper_positions.json` is an atomic state snapshot containing all positions and their corresponding trade records. `logs/paper_trades.jsonl` is an append-only event log. Opening creates an `OPENED` event with a fake filled entry order. Every price update creates an `UPDATED` event. A stop or target trigger then creates a `CLOSED` event with close reason, realized PnL, and realized R.

Missing state files represent an empty portfolio. Invalid/corrupt state fails closed with a readable persistence error. Duplicate open positions for a symbol are rejected unless the caller explicitly opts in.

## CLI flow

`paper-open-from-json` loads JSON, runs the existing engine, and opens only a `LONG_VALID` result. Other decisions are printed and do not create state. `paper-update-price` loads persisted state, applies the price to every open position for the symbol, persists any close, and journals the events. `paper-positions` lists only open positions.

All commands accept optional state/journal paths for isolated testing while defaulting to the required local paths.

