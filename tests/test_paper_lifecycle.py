import json
from datetime import datetime, timezone
from pathlib import Path

import pytest

from orderflow_ibkr_agent.data.input_adapter import evaluate_market_input
from orderflow_ibkr_agent.data.json_loader import load_market_decision_input
from orderflow_ibkr_agent.models import DecisionStatus, TradeSetupType
from orderflow_ibkr_agent.paper.lifecycle import (
    PaperTradeLifecycleManager,
    PaperTradeRejectedError,
)
from orderflow_ibkr_agent.paper.models import (
    PaperCloseReason,
    PaperPositionStatus,
)


SAMPLES = Path(__file__).parents[1] / "samples"
UPDATE_TIME = datetime(2026, 6, 24, 15, 0, tzinfo=timezone.utc)


def valid_input_and_decision():
    market_input = load_market_decision_input(SAMPLES / "valid_failed_breakdown_long.json")
    return market_input, evaluate_market_input(market_input)


@pytest.fixture
def manager(tmp_path: Path) -> PaperTradeLifecycleManager:
    return PaperTradeLifecycleManager(
        state_path=tmp_path / "paper_positions.json",
        journal_path=tmp_path / "paper_trades.jsonl",
    )


def test_long_valid_opens_persisted_paper_trade(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    record = manager.open_paper_trade_from_decision(decision, market_input)
    positions = manager.list_open_positions()
    assert record.status is PaperPositionStatus.OPEN
    assert record.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG
    assert len(positions) == 1
    assert positions[0].position_id == record.trade_id
    assert manager.state_path.exists()


def test_waiting_for_confirmation_decision_does_not_open(
    manager: PaperTradeLifecycleManager,
) -> None:
    market_input = load_market_decision_input(SAMPLES / "invalid_missing_confirmation.json")
    decision = evaluate_market_input(market_input)
    assert decision.decision is DecisionStatus.WAITING_FOR_CONFIRMATION
    with pytest.raises(PaperTradeRejectedError, match="LONG_VALID"):
        manager.open_paper_trade_from_decision(decision, market_input)
    assert manager.list_open_positions() == []


def test_invalid_risk_reward_decision_does_not_open(
    manager: PaperTradeLifecycleManager,
) -> None:
    market_input = load_market_decision_input(SAMPLES / "invalid_bad_risk_reward.json")
    decision = evaluate_market_input(market_input)
    assert decision.decision is DecisionStatus.INVALID_RISK_REWARD
    with pytest.raises(PaperTradeRejectedError, match="LONG_VALID"):
        manager.open_paper_trade_from_decision(decision, market_input)
    assert manager.list_open_positions() == []


def test_no_trade_decision_does_not_open(manager: PaperTradeLifecycleManager) -> None:
    market_input, valid_decision = valid_input_and_decision()
    no_trade = valid_decision.model_copy(update={"decision": DecisionStatus.NO_TRADE})
    with pytest.raises(PaperTradeRejectedError, match="LONG_VALID"):
        manager.open_paper_trade_from_decision(no_trade, market_input)
    assert manager.list_open_positions() == []


def test_short_setup_bypass_attempt_cannot_open(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    bypassed = decision.model_copy(update={"setup_type": "SHORT"})
    with pytest.raises(PaperTradeRejectedError, match="long-only"):
        manager.open_paper_trade_from_decision(bypassed, market_input)


def test_invalid_decision_geometry_cannot_open(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    bypassed = decision.model_copy(update={"stop_price": decision.entry_price})
    with pytest.raises(PaperTradeRejectedError, match="stop < entry < target"):
        manager.open_paper_trade_from_decision(bypassed, market_input)


def test_duplicate_open_symbol_is_rejected(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    manager.open_paper_trade_from_decision(decision, market_input)
    with pytest.raises(PaperTradeRejectedError, match="already open"):
        manager.open_paper_trade_from_decision(decision, market_input)


def test_non_trigger_price_keeps_position_open_and_journals_update(
    manager: PaperTradeLifecycleManager,
) -> None:
    market_input, decision = valid_input_and_decision()
    manager.open_paper_trade_from_decision(decision, market_input)
    updated = manager.update_open_position_with_price("ES", 6003.0, UPDATE_TIME)
    assert len(updated) == 1
    assert updated[0].status is PaperPositionStatus.OPEN
    events = manager.read_journal_events()
    assert [event["event_type"] for event in events] == ["OPENED", "UPDATED"]


def test_target_hit_closes_with_correct_pnl_and_r(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    manager.open_paper_trade_from_decision(decision, market_input)
    updated = manager.update_open_position_with_price("ES", 6006.25, UPDATE_TIME)
    position = updated[0]
    assert position.status is PaperPositionStatus.CLOSED
    assert position.close_reason is PaperCloseReason.TARGET_HIT
    assert position.realized_pnl == pytest.approx(300.0)
    assert position.r_multiple == pytest.approx(2.4)
    assert manager.list_open_positions() == []


def test_stop_hit_closes_with_minus_one_r(manager: PaperTradeLifecycleManager) -> None:
    market_input, decision = valid_input_and_decision()
    manager.open_paper_trade_from_decision(decision, market_input)
    position = manager.update_open_position_with_price("ES", 5997.75, UPDATE_TIME)[0]
    assert position.status is PaperPositionStatus.CLOSED
    assert position.close_reason is PaperCloseReason.STOP_HIT
    assert position.realized_pnl == pytest.approx(-125.0)
    assert position.r_multiple == pytest.approx(-1.0)


def test_closed_state_reloads_and_journal_contains_exit_order(
    manager: PaperTradeLifecycleManager,
) -> None:
    market_input, decision = valid_input_and_decision()
    manager.open_paper_trade_from_decision(decision, market_input)
    manager.update_open_position_with_price("ES", 6006.25, UPDATE_TIME)

    reloaded = PaperTradeLifecycleManager(manager.state_path, manager.journal_path)
    assert reloaded.list_open_positions() == []
    record = reloaded.list_trade_records()[0]
    assert record.status is PaperPositionStatus.CLOSED
    assert record.realized_pnl == pytest.approx(300.0)

    events = [json.loads(line) for line in manager.journal_path.read_text().splitlines()]
    assert [event["event_type"] for event in events] == ["OPENED", "UPDATED", "CLOSED"]
    assert events[0]["order"]["side"] == "BUY"
    assert events[-1]["order"]["side"] == "SELL_TO_CLOSE"
