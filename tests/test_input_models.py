from copy import deepcopy

import pytest
from pydantic import ValidationError

from orderflow_ibkr_agent.data.input_models import (
    MarketDecisionInput,
    TapeSpeed,
    ValueMarketState,
)
from orderflow_ibkr_agent.models import TradeSetupType


def valid_input_payload() -> dict:
    return {
        "instrument": {
            "symbol": "ES",
            "exchange": "CME",
            "asset_type": "futures",
            "tick_size": 0.25,
            "point_value": 50.0,
            "session": "RTH",
            "timestamp": "2026-06-24T14:30:00Z",
        },
        "value_context": {
            "previous_rth_vah": 6015.0,
            "previous_rth_val": 6000.0,
            "previous_rth_poc": 6008.0,
            "current_rth_vah": 6012.0,
            "current_rth_val": 6000.0,
            "current_rth_poc": 6006.0,
            "current_vwap": 6007.0,
            "vwap_upper_1": 6014.0,
            "vwap_upper_2": 6021.0,
            "vwap_lower_1": 6000.0,
            "vwap_lower_2": 5993.0,
            "composite_value_high": 6020.0,
            "composite_value_low": 5999.5,
            "context_bias": "bearish",
            "market_state": "rejection_lower",
        },
        "order_flow": {
            "aggressive_buy_volume": 850.0,
            "aggressive_sell_volume": 1400.0,
            "delta": -550.0,
            "cumulative_delta": -2200.0,
            "session_volume": 190000.0,
            "volume_spike_detected": True,
            "seller_absorption_detected": True,
            "buyer_absorption_detected": False,
            "trapped_sellers_detected": True,
            "trapped_buyers_detected": False,
            "passive_buy_liquidity_detected": True,
            "passive_sell_liquidity_detected": False,
            "liquidity_wall_price": 5999.5,
            "large_participant_level": 6000.0,
            "tape_speed": "fast",
            "orderflow_confidence": "high",
        },
        "setup_candidate": {
            "setup_type": "FAILED_BREAKDOWN_LONG",
            "entry_price": 6000.25,
            "stop_price": 5997.75,
            "target_price": 6006.25,
            "location_type": "PREVIOUS_RTH_VAL",
            "confirmation_type": "SELLER_ABSORPTION_RECLAIM",
            "reason": "Sellers were trapped below value and price reclaimed VAL.",
        },
        "metadata": {"source": "test", "min_reward_risk_ratio": 2.0},
    }


def test_valid_input_constructs_strict_nested_models() -> None:
    data = MarketDecisionInput.model_validate(valid_input_payload())
    assert data.instrument.symbol == "ES"
    assert data.value_context.market_state is ValueMarketState.REJECTION_LOWER
    assert data.order_flow.tape_speed is TapeSpeed.FAST
    assert data.setup_candidate.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG


@pytest.mark.parametrize(
    "setup",
    ["SHORT", "SELL_RESISTANCE", "FAILED_BREAKOUT_SHORT", "BEARISH_CONTINUATION_SHORT"],
)
def test_short_side_setup_values_are_rejected(setup: str) -> None:
    payload = valid_input_payload()
    payload["setup_candidate"]["setup_type"] = setup
    with pytest.raises(ValidationError, match="setup_type"):
        MarketDecisionInput.model_validate(payload)


@pytest.mark.parametrize(
    ("section", "field"),
    [
        ("instrument", "tick_size"),
        ("value_context", "current_vwap"),
        ("setup_candidate", "entry_price"),
    ],
)
def test_non_positive_market_prices_are_rejected(section: str, field: str) -> None:
    payload = valid_input_payload()
    payload[section][field] = 0
    with pytest.raises(ValidationError):
        MarketDecisionInput.model_validate(payload)


@pytest.mark.parametrize(
    ("stop", "entry", "target"),
    [(6000.25, 6000.25, 6006.25), (5997.75, 6000.25, 6000.25), (6001, 6000, 5999)],
)
def test_bad_long_risk_geometry_is_rejected(stop: float, entry: float, target: float) -> None:
    payload = valid_input_payload()
    payload["setup_candidate"].update(
        {"stop_price": stop, "entry_price": entry, "target_price": target}
    )
    with pytest.raises(ValidationError, match="stop < entry < target"):
        MarketDecisionInput.model_validate(payload)


def test_missing_required_orderflow_field_is_rejected() -> None:
    payload = deepcopy(valid_input_payload())
    del payload["order_flow"]["orderflow_confidence"]
    with pytest.raises(ValidationError, match="orderflow_confidence"):
        MarketDecisionInput.model_validate(payload)


def test_unknown_fields_are_rejected() -> None:
    payload = valid_input_payload()
    payload["instrument"]["side"] = "SHORT"
    with pytest.raises(ValidationError, match="Extra inputs are not permitted"):
        MarketDecisionInput.model_validate(payload)
