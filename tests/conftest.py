from datetime import datetime, timezone

import pytest

from orderflow_ibkr_agent.models import (
    MarketSnapshot,
    MarketState,
    OrderFlowSnapshot,
    RiskPlan,
    SessionType,
)


@pytest.fixture
def failed_breakdown_snapshot() -> MarketSnapshot:
    return MarketSnapshot(
        symbol="ES",
        timestamp=datetime(2026, 6, 24, 14, 30, tzinfo=timezone.utc),
        last_price=5999.75,
        session_type=SessionType.RTH,
        previous_vah=6015.0,
        previous_val=6000.0,
        previous_poc=6008.0,
        current_vah=6012.0,
        current_val=6000.0,
        current_poc=6006.0,
        vwap=6007.0,
        vwap_upper_1=6014.0,
        vwap_upper_2=6021.0,
        vwap_lower_1=6000.0,
        vwap_lower_2=5993.0,
        composite_value_high=6020.0,
        composite_value_low=5999.5,
        market_state=MarketState.BEARISH,
    )


@pytest.fixture
def confirming_order_flow() -> OrderFlowSnapshot:
    return OrderFlowSnapshot(
        aggressive_buy_volume=850,
        aggressive_sell_volume=1400,
        delta=-550,
        cumulative_delta=-2200,
        seller_absorption_detected=True,
        buyer_absorption_detected=False,
        passive_buy_liquidity=1800,
        passive_sell_liquidity=700,
        liquidity_wall_nearby=True,
        iceberg_detected=True,
        stop_run_detected=True,
        tape_speed=1.6,
        volume_outlier_score=2.1,
        price_reclaimed_level=True,
        price_accepted_above_level=True,
        price_accepted_below_level=False,
    )


@pytest.fixture
def valid_risk_plan() -> RiskPlan:
    return RiskPlan(
        proposed_entry=6000.25,
        proposed_stop=5997.75,
        proposed_target=6006.25,
        max_risk_points=3.0,
        min_reward_risk_ratio=2.0,
        position_size=1,
    )

