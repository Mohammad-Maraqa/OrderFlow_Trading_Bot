# NinjaTrader Platform Pivot Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Preserve Python as the tested strategy reference while adding a non-executing NinjaTrader 8 long-only NinjaScript skeleton and deprecating IBKR as the primary direction.

**Architecture:** Plain C# models and a pure evaluator are isolated from a thin NinjaScript `Strategy` shell. Python structural tests enforce the allowed setup surface and absence of NinjaTrader execution APIs. Documentation assigns clear platform ownership and installation steps.

**Tech Stack:** Python 3.11+/pytest reference suite, NinjaTrader 8 NinjaScript/C# skeleton

---

### Task 1: Structural safety tests

**Files:**
- Create: `tests/test_ninjatrader_skeleton.py`

- [ ] Write tests for required folder/files, approved long-only enums, execution-disabled marker, forbidden NinjaTrader order API absence, Python preservation, and IBKR deprecation labels.
- [ ] Run the focused tests and confirm failures because the NinjaTrader platform files do not exist.

### Task 2: NinjaScript models and evaluator

**Files:**
- Create: `NinjaTrader/bin/Custom/Strategies/OrderFlowModels.cs`
- Create: `NinjaTrader/bin/Custom/Strategies/LongOnlyDecisionEvaluator.cs`

- [ ] Add C# POCO/enums for normalized context, five long setups, decision status, candidate geometry, and decision audit output.
- [ ] Add a pure evaluator that fails closed on missing data, invalid context/location/confirmation, long geometry, and reward/risk.
- [ ] Run focused structural tests.

### Task 3: Evaluation-only NinjaScript strategy shell

**Files:**
- Create: `NinjaTrader/bin/Custom/Strategies/OrderFlowLongOnlyAgent.cs`

- [ ] Add NinjaTrader defaults and evaluation/logging properties.
- [ ] Keep `OnBarUpdate` explicitly observation-only and expose snapshot evaluation without any execution API.
- [ ] Run structural tests and inspect the source safety scan.

### Task 4: Platform documentation and IBKR status

**Files:**
- Create: `NinjaTrader/README.md`
- Create: `docs/PLATFORM_DIRECTION.md`
- Modify: `README.md`
- Modify: `src/orderflow_ibkr_agent/ibkr/__init__.py`

- [ ] Document Python, NinjaTrader, and IBKR roles plus NinjaTrader installation/compilation steps.
- [ ] Mark IBKR experimental/deprecated without deleting code or tests.
- [ ] Document deferred market-data wiring, playback validation, and all execution work.

### Task 5: Verification

- [ ] Run the full Python test suite.
- [ ] Run a structural scan for forbidden NinjaTrader order APIs.
- [ ] Confirm all Python and IBKR source files/tests remain present.
- [ ] Build the Python wheel.
