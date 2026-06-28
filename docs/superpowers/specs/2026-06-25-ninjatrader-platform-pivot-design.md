# NinjaTrader Platform Pivot Design

## Platform roles

Python remains the reference implementation for deterministic strategy rules, schema validation, sample data, regression tests, offline lifecycle behavior, and journal formats. It is not deleted or reduced.

NinjaTrader 8 becomes the forward integration target for chart, market-data, and order-flow work. The initial NinjaScript is evaluation-only: it can hold normalized snapshots, run long-only validation, and log decisions, but it cannot create, change, cancel, or transmit orders.

The existing IBKR read-only adapter remains in the repository for research compatibility. It is labeled experimental/deprecated and is no longer the primary platform direction. Its tests and source remain intact.

## NinjaScript structure

`NinjaTrader/bin/Custom/Strategies/` mirrors the NinjaTrader user directory. It contains:

- `OrderFlowModels.cs`: long-only setup, status, context, snapshot, and decision types.
- `LongOnlyDecisionEvaluator.cs`: a pure deterministic evaluator for data completeness, context, location, confirmation, geometry, and reward/risk.
- `OrderFlowLongOnlyAgent.cs`: a NinjaTrader `Strategy` shell that exposes evaluation settings and logs evaluation results. `OnBarUpdate` intentionally performs no trade action.

The setup enum contains only the five approved long setups. No short-side setup exists in the type system.

## Safety and testing

Python structural tests verify the NinjaTrader files exist, the approved long enums are present, the strategy is explicitly marked execution-disabled, and no NinjaTrader order-entry/exit/change/cancel API call appears in the C# source.

The existing Python suite remains part of every acceptance run. This phase does not require NinjaTrader to be installed and does not claim NinjaScript compilation in this environment; compilation/import instructions are documented for NinjaTrader 8.

