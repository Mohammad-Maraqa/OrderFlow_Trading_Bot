# Phase 2A Market Input Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a robust JSON market-input contract and CLI path that validates external snapshots, evaluates them through Phase 1, and journals every resulting decision.

**Architecture:** External Pydantic schemas live in `data/input_models.py`; `data/json_loader.py` owns file/JSON errors; `data/input_adapter.py` performs the only translation into Phase 1 models. The existing decision engine remains broker-independent and receives no source-specific parsing logic.

**Tech Stack:** Python 3.11+, Pydantic 2, Typer, Rich, pytest

---

### Task 1: External input contract

**Files:**
- Create: `src/orderflow_ibkr_agent/data/input_models.py`
- Create: `tests/test_input_models.py`

- [ ] Write model tests for valid construction, forbidden short labels, non-positive prices, and invalid long geometry.
- [ ] Run `python -m pytest tests/test_input_models.py -v` and confirm import failure because the models do not exist.
- [ ] Implement the enums and four nested snapshots plus `MarketDecisionInput` with cross-field validation.
- [ ] Run the focused tests and confirm they pass.

### Task 2: JSON loader and engine adapter

**Files:**
- Create: `src/orderflow_ibkr_agent/data/json_loader.py`
- Create: `src/orderflow_ibkr_agent/data/input_adapter.py`
- Create: `tests/test_json_input.py`

- [ ] Write tests for valid loading, malformed JSON, missing fields, missing files, waiting confirmation, and setup mismatch protection.
- [ ] Run the focused tests and confirm missing loader/adapter failures.
- [ ] Implement `MarketInputError`, `load_market_decision_input`, deterministic model translation, and evaluation.
- [ ] Run the focused tests and confirm they pass.

### Task 3: Samples and CLI

**Files:**
- Create: `samples/valid_failed_breakdown_long.json`
- Create: `samples/valid_pullback_continuation_long.json`
- Create: `samples/invalid_short_attempt.json`
- Create: `samples/invalid_bad_risk_reward.json`
- Create: `samples/invalid_missing_confirmation.json`
- Modify: `src/orderflow_ibkr_agent/cli.py`
- Create: `tests/test_evaluate_json_cli.py`

- [ ] Write CLI tests for valid evaluation, journal append, invalid short input, invalid risk result, and missing confirmation result.
- [ ] Run the focused tests and confirm `evaluate-json` is absent.
- [ ] Add sample fixtures and the CLI command with readable errors and `--journal-path`.
- [ ] Run the focused tests and confirm they pass.

### Task 4: Documentation and acceptance verification

**Files:**
- Modify: `README.md`

- [ ] Document the nested schema, valid/invalid examples, CLI invocation, journal behavior, and IBKR prohibition.
- [ ] Run `python -m pytest -q` and confirm all old and new tests pass.
- [ ] Run all existing CLI commands and the five-file `evaluate-json` matrix with expected success/rejection statuses.
- [ ] Build the wheel with `python -m pip wheel --no-deps . -w dist`.
