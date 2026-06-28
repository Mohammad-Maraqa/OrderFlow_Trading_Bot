import json
from pathlib import Path

import pytest

from orderflow_ibkr_agent.data.input_adapter import evaluate_market_input
from orderflow_ibkr_agent.data.json_loader import MarketInputError, load_market_decision_input
from orderflow_ibkr_agent.models import DecisionStatus, TradeSetupType
from test_input_models import valid_input_payload


def write_json(path: Path, payload: dict) -> Path:
    path.write_text(json.dumps(payload), encoding="utf-8")
    return path


def test_valid_json_file_loads_market_decision_input(tmp_path: Path) -> None:
    path = write_json(tmp_path / "input.json", valid_input_payload())
    data = load_market_decision_input(path)
    assert data.instrument.symbol == "ES"
    assert data.setup_candidate.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG


def test_missing_file_has_clear_loader_error(tmp_path: Path) -> None:
    with pytest.raises(MarketInputError, match="Unable to read JSON input"):
        load_market_decision_input(tmp_path / "missing.json")


def test_malformed_json_has_clear_loader_error(tmp_path: Path) -> None:
    path = tmp_path / "broken.json"
    path.write_text('{"instrument": ', encoding="utf-8")
    with pytest.raises(MarketInputError, match="Invalid JSON"):
        load_market_decision_input(path)


def test_missing_required_field_names_the_field(tmp_path: Path) -> None:
    payload = valid_input_payload()
    del payload["order_flow"]["orderflow_confidence"]
    path = write_json(tmp_path / "missing-field.json", payload)
    with pytest.raises(MarketInputError, match="order_flow.orderflow_confidence: Field required"):
        load_market_decision_input(path)


def test_valid_external_input_reaches_existing_decision_engine(tmp_path: Path) -> None:
    data = load_market_decision_input(write_json(tmp_path / "input.json", valid_input_payload()))
    decision = evaluate_market_input(data)
    assert decision.decision is DecisionStatus.LONG_VALID
    assert decision.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG
    assert decision.risk_reward_ratio == pytest.approx(2.4)


def test_complete_input_without_confirmation_waits(tmp_path: Path) -> None:
    payload = valid_input_payload()
    payload["value_context"].update(
        {"context_bias": "bullish", "market_state": "acceptance_higher"}
    )
    payload["order_flow"].update(
        {
            "volume_spike_detected": False,
            "seller_absorption_detected": False,
            "buyer_absorption_detected": False,
            "trapped_sellers_detected": False,
            "passive_buy_liquidity_detected": False,
            "orderflow_confidence": "low",
        }
    )
    payload["setup_candidate"].update(
        {
            "setup_type": "PULLBACK_CONTINUATION_LONG",
            "confirmation_type": "NONE",
            "reason": "Candidate is waiting for order-flow confirmation.",
        }
    )
    data = load_market_decision_input(write_json(tmp_path / "waiting.json", payload))
    decision = evaluate_market_input(data)
    assert decision.decision is DecisionStatus.WAITING_FOR_CONFIRMATION


def test_declared_setup_must_match_engine_classification(tmp_path: Path) -> None:
    payload = valid_input_payload()
    payload["setup_candidate"]["setup_type"] = "PULLBACK_CONTINUATION_LONG"
    data = load_market_decision_input(write_json(tmp_path / "mismatch.json", payload))
    decision = evaluate_market_input(data)
    assert decision.decision is DecisionStatus.NO_TRADE
    assert "does not match" in decision.reasons[-1]
