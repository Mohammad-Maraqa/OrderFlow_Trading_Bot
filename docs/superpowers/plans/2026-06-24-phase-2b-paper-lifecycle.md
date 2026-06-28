# Phase 2B Offline Paper Lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create and persist a long-only internal paper position from a valid decision, update it with fake prices, close on stop/target, and audit the complete lifecycle.

**Architecture:** Strict models live in `paper/models.py`; `paper/persistence.py` owns atomic JSON state and JSONL events; `paper/lifecycle.py` owns validation and state transitions. CLI commands compose the existing JSON loader/decision adapter with this offline lifecycle and never touch the IBKR package.

**Tech Stack:** Python 3.11+, Pydantic 2, Typer, pytest, JSON/JSONL

---

### Task 1: Paper domain models

**Files:**
- Create: `src/orderflow_ibkr_agent/paper/models.py`
- Create: `tests/test_paper_models.py`

- [ ] Write tests for allowed order sides/statuses, long geometry, and complete open/closed states.
- [ ] Run the focused tests and confirm the missing module failure.
- [ ] Implement strict enums and Pydantic models with cross-field validation.
- [ ] Run the focused tests and confirm they pass.

### Task 2: Lifecycle, state, and event journal

**Files:**
- Create: `src/orderflow_ibkr_agent/paper/persistence.py`
- Create: `src/orderflow_ibkr_agent/paper/lifecycle.py`
- Create: `tests/test_paper_lifecycle.py`

- [ ] Write tests for valid open, rejected decision, short bypass attempt, bad geometry, duplicate symbol, non-trigger update, target close, stop close, PnL/R, persistence, and events.
- [ ] Run the focused tests and confirm missing implementation failures.
- [ ] Implement atomic state persistence, append-only journal events, and long-only lifecycle transitions.
- [ ] Run the focused tests and confirm they pass.

### Task 3: Persistent CLI workflow

**Files:**
- Modify: `src/orderflow_ibkr_agent/cli.py`
- Create: `tests/test_paper_cli.py`

- [ ] Write CLI tests that open from valid JSON, refuse a non-valid decision, list the position, update below target, close at target, and reload closed state.
- [ ] Run focused tests and confirm the commands are absent.
- [ ] Implement `paper-open-from-json`, `paper-update-price`, and `paper-positions` with overridable local paths.
- [ ] Run focused tests and confirm they pass.

### Task 4: Documentation and acceptance verification

**Files:**
- Modify: `README.md`
- Modify: `.gitignore`

- [ ] Document target and stop lifecycle examples and the offline safety boundary.
- [ ] Ignore runtime state and paper journal files while retaining their parent directories.
- [ ] Run the complete test suite and all existing commands.
- [ ] Run target-hit and stop-hit CLI flows in isolated local state.
- [ ] Verify no Phase 2B module imports or calls IBKR.
- [ ] Build a fresh wheel.
