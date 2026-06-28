# Phase 3A IBKR Read-Only Connection Design

## Boundary

Phase 3A adds an optional observation-only adapter. The adapter exposes exactly six public operations: `connect`, `disconnect`, `is_connected`, `get_connection_status`, `get_account_summary`, `get_positions`, and `request_market_snapshot`. It exposes no broker order operation.

The legacy Phase 1 `IBKRClient` placeholder and its submission-shaped methods are removed. The offline simulator moves out of the `ibkr` package into the existing `paper` package so the IBKR namespace is structurally read-only.

## Optional backend

`IBKRReadOnlyAdapter` depends on a small internal read-client protocol rather than directly on `ib_insync`. Tests inject `FakeIBKRReadOnlyClient`. When no client is injected, connection lazily imports the optional `ib_insync` backend; importing the application, running the decision engine, and using offline paper trades never requires that dependency.

The optional backend wraps only connection, account summary, positions, and snapshot market data. It does not wrap or expose order methods.

## Safety

`IBKRConfig` is disabled, read-only, and paper-required by default. The pre-connection guard requires explicit enablement, `readonly=true`, and a paper-safe port (`7497` for paper TWS or `4002` for paper Gateway). Live ports `7496` and `4001` are rejected unless `allow_unsafe_live_port=true` is explicitly set; even then, the post-connection account check remains required by default.

After connecting, the adapter reads the account identifier. When paper-account enforcement is active, identifiers must use the IBKR paper-style `DU` prefix. Failure disconnects immediately and raises `IBKRSafetyError`.

The guard also inspects the adapter's public class surface for forbidden order method names. This is defense in depth; those methods are absent by design.

## CLI

`ibkr-status`, `ibkr-positions`, and `ibkr-snapshot` load `ORDERFLOW_IBKR_*` settings for each invocation. Disabled mode returns a clear safe message without importing `ib_insync` or attempting a socket connection. Enabled commands always run the guard, connect, perform one read-only operation, and disconnect in `finally`.

Account IDs are masked in CLI output. Futures snapshots require an explicit expiry and never guess one.

