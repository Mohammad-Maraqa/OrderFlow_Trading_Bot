# Phase 3A IBKR Read-Only Connection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an optional, disabled-by-default IBKR paper-account read adapter with no order-capable API.

**Architecture:** `ibkr/config.py` and `ibkr/safety.py` enforce connection policy; `ibkr/adapter.py` uses a narrow injected protocol; `ibkr/backend.py` lazily wraps optional `ib_insync`; `ibkr/fakes.py` supplies deterministic tests. CLI commands compose this adapter only for explicitly enabled read operations.

**Tech Stack:** Python 3.11+, Pydantic Settings, optional ib_insync, Typer, pytest

---

### Task 1: Configuration and structural safety

**Files:**
- Create: `src/orderflow_ibkr_agent/ibkr/config.py`
- Create: `src/orderflow_ibkr_agent/ibkr/errors.py`
- Create: `src/orderflow_ibkr_agent/ibkr/safety.py`
- Modify: `tests/test_ibkr_safety.py`

- [ ] Write tests for disabled/read-only/paper defaults, safe and live ports, unsafe overrides, and forbidden method detection.
- [ ] Run focused tests and confirm missing-model failures.
- [ ] Implement config, explicit exceptions, and pre-connection guard.
- [ ] Run focused tests and confirm they pass.

### Task 2: Read-only adapter and fake client

**Files:**
- Create: `src/orderflow_ibkr_agent/ibkr/models.py`
- Create: `src/orderflow_ibkr_agent/ibkr/adapter.py`
- Create: `src/orderflow_ibkr_agent/ibkr/backend.py`
- Create: `src/orderflow_ibkr_agent/ibkr/fakes.py`
- Modify: `src/orderflow_ibkr_agent/ibkr/contracts.py`
- Delete: `src/orderflow_ibkr_agent/ibkr/client.py`
- Test: `tests/test_ibkr_read_only_adapter.py`

- [ ] Write fake-backed tests for disabled connect, success/failure, paper account enforcement, disconnect, summaries, positions, snapshots, missing optional dependency, and forbidden public methods.
- [ ] Run focused tests and confirm missing-adapter failures.
- [ ] Implement normalized read models, protocol, fake, adapter, and lazy backend.
- [ ] Run focused tests and confirm they pass.

### Task 3: Move offline simulation out of IBKR

**Files:**
- Create: `src/orderflow_ibkr_agent/paper/simulator.py`
- Delete: `src/orderflow_ibkr_agent/ibkr/paper_executor.py`
- Modify: `src/orderflow_ibkr_agent/cli.py`
- Modify: `tests/test_ibkr_safety.py`

- [ ] Update regression imports to require the simulator from the offline paper package.
- [ ] Move the unchanged fake simulator and confirm existing commands still pass.
- [ ] Scan the IBKR package for order-shaped public methods and paper-execution code.

### Task 4: Read-only CLI

**Files:**
- Modify: `src/orderflow_ibkr_agent/cli.py`
- Create: `tests/test_ibkr_cli.py`

- [ ] Write tests for disabled status plus mocked status, positions, stock snapshot, and explicit futures expiry failure.
- [ ] Run focused tests and confirm commands are absent.
- [ ] Implement the three commands with masked accounts and guaranteed disconnect.
- [ ] Run focused tests and confirm they pass.

### Task 5: Packaging, environment, documentation, and verification

**Files:**
- Modify: `pyproject.toml`
- Modify: `.env.example`
- Modify: `README.md`

- [ ] Add optional `ibkr` dependency extra and safe environment examples.
- [ ] Document TWS/Gateway paper ports, TWS API Read-Only mode, forbidden live ports, and commands.
- [ ] Run the complete suite and all existing offline CLI regressions.
- [ ] Run a structural source scan proving no forbidden broker-order method is defined.
- [ ] Build the base wheel without installing the optional IBKR dependency.
