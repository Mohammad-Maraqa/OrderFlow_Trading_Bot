from datetime import datetime, timezone

import pytest
from pydantic import ValidationError

from orderflow_ibkr_agent.models import TradeSetupType
from orderflow_ibkr_agent.paper.models import (
    PaperCloseReason,
    PaperOrder,
    PaperOrderSide,
    PaperOrderStatus,
    PaperOrderType,
    PaperPosition,
    PaperPositionStatus,
    PaperTradeRecord,
)


NOW = datetime(2026, 6, 24, 14, 30, tzinfo=timezone.utc)


def test_filled_buy_paper_order_is_valid() -> None:
    order = PaperOrder(
        order_id="order-1",
        symbol="ES",
        side=PaperOrderSide.BUY,
        order_type=PaperOrderType.LIMIT,
        price=6000.25,
        quantity=1,
        status=PaperOrderStatus.FILLED,
        created_at=NOW,
        filled_at=NOW,
    )
    assert order.side is PaperOrderSide.BUY


@pytest.mark.parametrize("side", ["SHORT", "SELL", "SELL_SHORT"])
def test_short_order_sides_are_rejected(side: str) -> None:
    with pytest.raises(ValidationError):
        PaperOrder(
            order_id="bad",
            symbol="ES",
            side=side,
            order_type="MARKET",
            price=6000,
            quantity=1,
            status="PENDING",
            created_at=NOW,
        )


def test_filled_order_requires_filled_timestamp() -> None:
    with pytest.raises(ValidationError, match="filled_at"):
        PaperOrder(
            order_id="bad",
            symbol="ES",
            side="BUY",
            order_type="LIMIT",
            price=6000,
            quantity=1,
            status="FILLED",
            created_at=NOW,
        )


def test_open_position_rejects_bad_long_geometry() -> None:
    with pytest.raises(ValidationError, match="stop < entry < target"):
        PaperPosition(
            position_id="position-1",
            symbol="ES",
            quantity=1,
            entry_price=6000,
            stop_price=6001,
            target_price=6005,
            point_value=50,
            status="OPEN",
            opened_at=NOW,
        )


def test_closed_position_requires_complete_close_accounting() -> None:
    with pytest.raises(ValidationError, match="closed position"):
        PaperPosition(
            position_id="position-1",
            symbol="ES",
            quantity=1,
            entry_price=6000,
            stop_price=5998,
            target_price=6004,
            point_value=50,
            status=PaperPositionStatus.CLOSED,
            opened_at=NOW,
            close_reason=PaperCloseReason.TARGET_HIT,
        )


def test_closed_trade_record_accepts_realized_results() -> None:
    record = PaperTradeRecord(
        trade_id="trade-1",
        decision_id="decision-1",
        setup_type=TradeSetupType.FAILED_BREAKDOWN_LONG,
        symbol="ES",
        entry_price=6000,
        stop_price=5998,
        target_price=6004,
        quantity=1,
        point_value=50,
        risk_per_unit=2,
        reward_per_unit=4,
        planned_r_multiple=2,
        status=PaperPositionStatus.CLOSED,
        close_price=6004,
        close_reason=PaperCloseReason.TARGET_HIT,
        realized_pnl=200,
        realized_r_multiple=2,
        decision_reason="CLC valid",
        created_at=NOW,
        closed_at=NOW,
    )
    assert record.realized_r_multiple == 2
