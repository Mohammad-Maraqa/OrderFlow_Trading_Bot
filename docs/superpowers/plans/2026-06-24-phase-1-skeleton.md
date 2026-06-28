# Phase 1 Skeleton Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a deterministic, auditable, long-only order-flow decision engine with a disabled-by-default, mockable IBKR placeholder layer.

**Architecture:** Pydantic models define validated market inputs and decisions. Small strategy evaluators implement Context + Location + Confirmation and risk checks, while a coordinating `DecisionEngine` applies a deterministic precedence order. IBKR types remain isolated and dependency-free; V1 only creates simulated paper orders and always rejects live submission.

**Tech Stack:** Python 3.11+, Pydantic 2, pydantic-settings, Typer, Rich, pytest

---

### Task 1: Package and validated domain models

**Files:**
- Create: `pyproject.toml`
- Create: `.env.example`
- Create: `src/orderflow_ibkr_agent/__init__.py`
- Create: `src/orderflow_ibkr_agent/config.py`
- Create: `src/orderflow_ibkr_agent/models.py`
- Test: `tests/test_long_only_guard.py`

- [ ] Write tests that reject forbidden setup values and invalid long risk geometry.
- [ ] Run `py -3.11 -m pytest tests/test_long_only_guard.py -v` and confirm collection/import fails because the package does not exist.
- [ ] Add enums, nested market snapshots, risk plans, data-quality flags, and trade decisions with long-only validation.
- [ ] Run the focused test and confirm it passes.

### Task 2: Deterministic CLC strategy evaluators

**Files:**
- Create: `src/orderflow_ibkr_agent/strategy/context_engine.py`
- Create: `src/orderflow_ibkr_agent/strategy/location_engine.py`
- Create: `src/orderflow_ibkr_agent/strategy/orderflow_engine.py`
- Create: `src/orderflow_ibkr_agent/strategy/setup_classifier.py`
- Create: `src/orderflow_ibkr_agent/strategy/risk_engine.py`
- Test: `tests/test_risk_engine.py`
- Test: `tests/test_setup_classifier.py`

- [ ] Write focused tests for setup classification, missing confirmation, valid/invalid reward:risk, and late entry detection.
- [ ] Run focused tests and confirm expected missing-implementation failures.
- [ ] Implement pure evaluators returning explicit results and reasons.
- [ ] Run focused tests and confirm they pass.

### Task 3: Decision coordinator and sample snapshots

**Files:**
- Create: `src/orderflow_ibkr_agent/decision_engine.py`
- Create: `src/orderflow_ibkr_agent/data/schemas.py`
- Create: `src/orderflow_ibkr_agent/data/sample_snapshots.py`
- Test: `tests/test_decision_engine.py`

- [ ] Write tests for a valid failed-breakdown long, missing order flow, invalid location, bad reward:risk, and complete audit fields.
- [ ] Run focused tests and confirm expected missing-implementation failures.
- [ ] Implement deterministic status precedence: missing data, context, location, confirmation, setup, risk, then `LONG_VALID`.
- [ ] Run focused tests and confirm they pass.

### Task 4: Safe optional IBKR placeholders and static paper simulation

**Files:**
- Create: `src/orderflow_ibkr_agent/ibkr/client.py`
- Create: `src/orderflow_ibkr_agent/ibkr/contracts.py`
- Create: `src/orderflow_ibkr_agent/ibkr/paper_executor.py`
- Test: `tests/test_ibkr_safety.py`

- [ ] Write tests proving the client defaults disconnected, live orders always fail, paper submission requires explicit enablement, and shorts cannot enter the executor.
- [ ] Run focused tests and confirm expected missing-implementation failures.
- [ ] Implement dependency-free contract descriptors, a mockable client interface, and simulated limit orders only.
- [ ] Run focused tests and confirm they pass.

### Task 5: Journal, CLI, documentation, and verification

**Files:**
- Create: `src/orderflow_ibkr_agent/journal/trade_journal.py`
- Create: `src/orderflow_ibkr_agent/cli.py`
- Create: `README.md`
- Create package `__init__.py` files
- Test: `tests/test_cli.py`

- [ ] Write CLI tests for `evaluate-sample`, `validate-long-only`, and offline `paper-sim-sample`.
- [ ] Run focused tests and confirm expected failures.
- [ ] Implement JSONL decision journaling and the three Typer commands without network access.
- [ ] Run `py -3.11 -m pytest -v` and confirm the full suite passes.
- [ ] Run each CLI command and inspect its output.
- [ ] Review configuration and IBKR code to confirm live execution is impossible in V1.
