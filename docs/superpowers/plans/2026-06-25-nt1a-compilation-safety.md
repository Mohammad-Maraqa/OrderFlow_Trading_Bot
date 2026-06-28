# NT-1A Compilation and Safety Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Organize and harden the evaluation-only NinjaTrader 8 skeleton for manual NinjaScript compilation without adding execution capability.

**Architecture:** The NinjaScript Strategy lives in the required Strategies namespace; plain helper models/evaluator/guards live in a dedicated helper namespace. Static Python tests enforce compile-oriented structure, safe defaults, long-only types, and zero order API calls; manual NinjaTrader compilation remains the final platform check.

**Tech Stack:** NinjaTrader 8 NinjaScript/C#, Python pytest static safety tests

---

### Task 1: NT-1A static contract tests

**Files:**
- Modify: `tests/test_ninjatrader_skeleton.py`

- [ ] Update tests for the requested Strategies/Models/Core layout and documentation files.
- [ ] Assert Strategy namespace/inheritance, lifecycle methods, all required defaults, and evaluation logging.
- [ ] Assert guard method names, long-only setup enum, short-intent rejection text, and absence of every forbidden execution API call.
- [ ] Run focused tests and confirm missing-layout/default failures.

### Task 2: Models, evaluator, and safety guards

**Files:**
- Create: `NinjaTrader/Models/OrderFlowFeatureSnapshot.cs`
- Create: `NinjaTrader/Models/OrderFlowSignalState.cs`
- Create: `NinjaTrader/Models/LongSetupType.cs`
- Create: `NinjaTrader/Models/LongDecisionResult.cs`
- Create: `NinjaTrader/Core/LongOnlyOrderFlowEvaluator.cs`
- Create: `NinjaTrader/Core/NinjaTraderSafetyGuards.cs`

- [ ] Implement one-purpose model files in the helper namespace.
- [ ] Implement deterministic fail-closed evaluation.
- [ ] Implement guard methods that make execution and live trading impossible in NT-1A.

### Task 3: NinjaScript Strategy shell

**Files:**
- Create: `NinjaTrader/Strategies/LongOnlyOrderFlowAgentStrategy.cs`
- Delete: `NinjaTrader/bin/Custom/Strategies/OrderFlowModels.cs`
- Delete: `NinjaTrader/bin/Custom/Strategies/LongOnlyDecisionEvaluator.cs`
- Delete: `NinjaTrader/bin/Custom/Strategies/OrderFlowLongOnlyAgent.cs`

- [ ] Add NinjaTrader lifecycle methods and required user properties/defaults.
- [ ] Evaluate an incomplete read-only snapshot and print safety/decision state.
- [ ] Keep all execution paths absent.
- [ ] Remove old duplicate source layout.

### Task 4: Manual installation and compilation docs

**Files:**
- Rewrite: `NinjaTrader/README.md`
- Create: `NinjaTrader/INSTALL_IN_NINJATRADER.md`
- Create: `NinjaTrader/COMPILATION_CHECKLIST.md`
- Modify: `README.md`
- Modify: `docs/PLATFORM_DIRECTION.md`

- [ ] Document exact copy destinations, editor/compile/chart/output workflow, expected messages, and error handling.
- [ ] Add the required manual checklist and explicit evaluation-only warnings.

### Task 5: Verification

- [ ] Run the complete Python suite and existing CLI smoke tests.
- [ ] Run an independent forbidden-API scan over every new C# file.
- [ ] Confirm Python and IBKR trees/tests are unchanged and present.
- [ ] Build the Python wheel.
