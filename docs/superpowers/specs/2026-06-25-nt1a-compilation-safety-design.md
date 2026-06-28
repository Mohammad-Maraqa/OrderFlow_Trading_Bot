# NT-1A Compilation and Safety Design

## Source organization

The repository uses a clean authoring layout:

```text
NinjaTrader/
├── Strategies/LongOnlyOrderFlowAgentStrategy.cs
├── Models/*.cs
└── Core/*.cs
```

For installation, the Strategy file is copied to the NinjaTrader 8 user `bin/Custom/Strategies` directory. Models and Core files are copied beneath `bin/Custom/OrderFlowAgent/Models` and `bin/Custom/OrderFlowAgent/Core`. NinjaTrader compiles C# recursively beneath `bin/Custom`.

The previous `NinjaTrader/bin/Custom/Strategies` repository mirror is removed so copying the new sources cannot create duplicate type definitions.

## Namespaces and compilation boundary

The Strategy uses the required `NinjaTrader.NinjaScript.Strategies` namespace and inherits from `Strategy`. Helper types use `NinjaTrader.NinjaScript.OrderFlowAgent`, imported by the Strategy. Helpers remain plain C# and contain no NinjaTrader order APIs.

This environment cannot compile against NinjaTrader assemblies. Static Python tests verify layout, namespaces, required lifecycle overrides, property defaults, long-only types, safety guards, and absence of execution API calls. The manual checklist is the authoritative platform compilation gate.

## Evaluation-only behavior

`OnStateChange` sets every required safe default. `OnBarUpdate` builds an explicitly incomplete feature snapshot from read-only bar information, evaluates it, and prints mode, signal state, status, reason, and optional planned prices. It never reaches an execution branch.

`NinjaTraderSafetyGuards.IsExecutionAllowed` always returns false in NT-1A. `ValidateLongOnlyDecision` permits only a valid enabled long setup with `stop < entry < target`. Text containing short-side intent is rejected for diagnostics/import boundaries.

## Deferred work

Native order-flow/volumetric feature extraction, Market Replay validation, Sim101 execution, and live execution remain separate future phases.

