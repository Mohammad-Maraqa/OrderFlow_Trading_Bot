# Long-Only Order-Flow Agent

A deterministic long-only order-flow project. It applies Context + Location + Confirmation (CLC), classifies one of five allowed long setups, validates risk/reward, and emits auditable decisions.

## Platform direction

- **NinjaTrader 8 is the primary forward platform** for future chart and order-flow integration. The new [NinjaTrader skeleton](NinjaTrader/README.md) is evaluation-only and cannot trade.
- **Python remains the strategy reference** for specifications, schemas, deterministic tests, journaling, and offline paper-lifecycle behavior. The Python project and tests are being preserved.
- **IBKR is experimental/deprecated as the primary direction.** Its optional read-only adapter remains in the repository for research compatibility; no IBKR code or tests are removed yet.

The full rationale and responsibility split are in [docs/PLATFORM_DIRECTION.md](docs/PLATFORM_DIRECTION.md).

### NT-1A compilation preparation

NT-1A organizes the NinjaScript source for manual NinjaTrader compilation and adds explicit evaluation-only safety guards. No NinjaTrader execution exists yet. Follow [the NinjaTrader installation guide](NinjaTrader/INSTALL_IN_NINJATRADER.md) and complete the [compilation checklist](NinjaTrader/COMPILATION_CHECKLIST.md) inside NinjaTrader 8.

Python tests verify source layout, safe defaults, long-only types, and forbidden API absence. They do not replace the manual NinjaTrader compile and chart-loading check.

## Install

Python 3.11 or newer is required.

```powershell
py -3.11 -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -e ".[dev]"
```

Copy `.env.example` to `.env` only when you need custom settings. Live orders are structurally fixed to `false`.

## Test

```powershell
python -m pytest -v
```

## CLI

```powershell
orderflow-agent evaluate-sample
orderflow-agent evaluate-json samples/valid_failed_breakdown_long.json
orderflow-agent validate-long-only
orderflow-agent paper-sim-sample
orderflow-agent paper-open-from-json samples/valid_failed_breakdown_long.json
orderflow-agent paper-update-price --symbol ES --price 6003.0
orderflow-agent paper-positions
```

`evaluate-json` validates the input, runs the existing deterministic engine, prints a decision summary, and appends the result to `journal/decisions.jsonl`. Choose a different journal with:

```powershell
orderflow-agent evaluate-json samples/valid_pullback_continuation_long.json `
  --journal-path output/decisions.jsonl
```

No `evaluate-json` path connects to IBKR or submits an order.

## Offline paper lifecycle

Phase 2B can turn a `LONG_VALID` decision into an internal fake-fill position. This is local lifecycle testing, not broker paper trading: it performs no IBKR imports, connections, API calls, or order submissions.

By default, current state is stored in `data/paper_positions.json` and append-only events are written to `logs/paper_trades.jsonl`. Every open and price update is journaled; a stop or target trigger adds a close event containing realized PnL and R.

For futures, realized currency PnL uses the input instrument's point value:

```text
(close - entry) * quantity * point_value
```

### Target-hit example

Start with no open `ES` paper position, then run:

```powershell
orderflow-agent paper-open-from-json samples/valid_failed_breakdown_long.json
orderflow-agent paper-update-price --symbol ES --price 6003.0
orderflow-agent paper-positions
orderflow-agent paper-update-price --symbol ES --price 6006.25
orderflow-agent paper-positions
```

The intermediate update leaves the position `OPEN`. At `6006.25`, the sample closes as `TARGET_HIT` with `+$300.00` and `+2.40R` for one ES contract.

### Stop-hit example

After the first trade is closed, open the sample again and update to its stop:

```powershell
orderflow-agent paper-open-from-json samples/valid_failed_breakdown_long.json
orderflow-agent paper-update-price --symbol ES --price 5997.75
orderflow-agent paper-positions
```

The trade closes as `STOP_HIT` with `-$125.00` and `-1.00R`. Duplicate open positions for the same symbol are rejected by default.

To isolate a run, use `--positions-path` and `--paper-journal-path` on each paper command.

## Market input schema

Every JSON document has four required objects and optional metadata:

- `instrument`: symbol, exchange, asset type, tick size, point value, RTH/ETH session, and timezone-aware timestamp
- `value_context`: previous/current RTH value areas, VWAP bands, composite value, directional bias, and market state
- `order_flow`: aggressive volume, delta, session volume, absorption/trapped-trader/liquidity flags, tape speed, and confidence
- `setup_candidate`: one allowed long setup, entry, stop, target, location, confirmation, and reason
- `metadata`: optional source tags plus optional `min_reward_risk_ratio` and `max_risk_points`

Unknown fields and missing required fields are rejected. All market prices must be positive; volumes cannot be negative; value/VWAP levels must be ordered; timestamps require a timezone; and every candidate must satisfy `stop < entry < target`.

The only accepted setup values are:

```text
FAILED_BREAKDOWN_LONG
PULLBACK_CONTINUATION_LONG
VALUE_RECLAIM_LONG
BREAKOUT_PULLBACK_LONG
DEVIATION_REJECTION_LONG
```

Any other value—including `SHORT`, `SELL_RESISTANCE`, `FAILED_BREAKOUT_SHORT`, and `BEARISH_CONTINUATION_SHORT`—fails model validation before reaching the decision engine.

### Input examples

Valid long candidates:

```powershell
orderflow-agent evaluate-json samples/valid_failed_breakdown_long.json
orderflow-agent evaluate-json samples/valid_pullback_continuation_long.json
```

Deterministic rejection/waiting examples:

```powershell
# Valid structure, rejected by the risk engine
orderflow-agent evaluate-json samples/invalid_bad_risk_reward.json

# Valid structure, waits because order-flow confirmation is absent
orderflow-agent evaluate-json samples/invalid_missing_confirmation.json

# Invalid structure, rejected immediately by the long-only model guard
orderflow-agent evaluate-json samples/invalid_short_attempt.json
```

## Implemented

- Pydantic market, order-flow, risk, data-quality, setup, and decision models
- deterministic CLC validation and five long-only setup types
- missing-data, context, location, confirmation, late-entry, and reward:risk rejection paths
- sample failed-breakdown snapshot and offline paper-order simulation
- JSONL decision journal
- strict external `MarketDecisionInput` schema and JSON loader
- deterministic adapter from external snapshots to the Phase 1 engine
- representative valid, rejected, waiting, and forbidden sample files
- strict paper order, position, trade-record, and event models
- atomic local paper-position persistence and append-only lifecycle journaling
- stop/target fake execution with point-value PnL and realized R accounting
- optional IBKR client/contract interfaces that fail closed
- evaluation-only NinjaTrader 8 models, long-only evaluator, and NinjaScript Strategy shell

## Intentionally deferred

- IBKR/TWS/Gateway connections and account calls
- real-time or historical broker data
- IBKR paper and live order submission
- broker fills, bracket management, dashboards, alerts, ML, and AI overrides
- NinjaTrader native data wiring, Market Replay certification, Sim101, and live execution

A later phase may implement IBKR paper-order execution behind a separate safety boundary, but broker orders and live execution remain disabled.

## Experimental/deprecated: IBKR paper read-only connection

The retained experimental/deprecated IBKR adapter can optionally connect to TWS or IB Gateway to read connection status, account summary, positions, and one-shot market snapshots. It implements no broker orders—paper or live—and exposes no place, submit, modify, cancel, bracket, or transmit operation.

The base installation does not require an IBKR library. Install the optional backend only on a machine where you intend to run TWS or Gateway:

```powershell
python -m pip install -e ".[ibkr]"
```

In TWS or IB Gateway:

1. Enable socket/API clients.
2. Keep the API **Read-Only** setting enabled.
3. Use a paper account.
4. Confirm the configured socket port.

Paper defaults are:

- TWS paper: `7497`
- IB Gateway paper: `4002`

Do not use live TWS port `7496` or live Gateway port `4001` in this phase. They are rejected by the default safety guard.

IBKR access is disabled unless explicitly enabled:

```dotenv
ORDERFLOW_IBKR_ENABLED=true
ORDERFLOW_IBKR_HOST=127.0.0.1
ORDERFLOW_IBKR_PORT=7497
ORDERFLOW_IBKR_CLIENT_ID=101
ORDERFLOW_IBKR_READONLY=true
ORDERFLOW_IBKR_REQUIRE_PAPER=true
ORDERFLOW_IBKR_CONNECTION_TIMEOUT_SECONDS=10
ORDERFLOW_IBKR_MARKET_DATA_TYPE=delayed
```

Read-only commands:

```powershell
orderflow-agent ibkr-status
orderflow-agent ibkr-positions
orderflow-agent ibkr-snapshot --symbol AAPL --security-type STK --exchange SMART --currency USD
orderflow-agent ibkr-snapshot --symbol ES --security-type FUT --exchange CME --currency USD --expiry 202609
```

Futures expiry is always required and is never guessed. With `ORDERFLOW_IBKR_ENABLED=false`, the commands print a safe disabled message and do not import the optional backend or attempt a connection.
