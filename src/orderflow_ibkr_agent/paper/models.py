from datetime import datetime
from enum import StrEnum
from typing import Self

from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator

from orderflow_ibkr_agent.models import TradeSetupType


class StrictPaperModel(BaseModel):
    model_config = ConfigDict(extra="forbid", str_strip_whitespace=True)


class PaperOrderSide(StrEnum):
    BUY = "BUY"
    SELL_TO_CLOSE = "SELL_TO_CLOSE"


class PaperOrderType(StrEnum):
    MARKET = "MARKET"
    LIMIT = "LIMIT"
    STOP = "STOP"
    TARGET = "TARGET"


class PaperOrderStatus(StrEnum):
    PENDING = "PENDING"
    FILLED = "FILLED"
    CANCELLED = "CANCELLED"
    REJECTED = "REJECTED"


class PaperPositionStatus(StrEnum):
    OPEN = "OPEN"
    CLOSED = "CLOSED"


class PaperCloseReason(StrEnum):
    TARGET_HIT = "TARGET_HIT"
    STOP_HIT = "STOP_HIT"
    MANUAL_EXIT = "MANUAL_EXIT"
    TIMEOUT = "TIMEOUT"
    INVALIDATED = "INVALIDATED"


class PaperEventType(StrEnum):
    OPENED = "OPENED"
    UPDATED = "UPDATED"
    CLOSED = "CLOSED"


def _timezone_aware(value: datetime) -> datetime:
    if value.tzinfo is None or value.utcoffset() is None:
        raise ValueError("timestamp must include a timezone")
    return value


class PaperOrder(StrictPaperModel):
    order_id: str = Field(min_length=1)
    symbol: str = Field(min_length=1)
    side: PaperOrderSide
    order_type: PaperOrderType
    price: float = Field(gt=0)
    quantity: int = Field(gt=0)
    status: PaperOrderStatus
    created_at: datetime
    filled_at: datetime | None = None

    _validate_created_at = field_validator("created_at")(_timezone_aware)
    _validate_filled_at = field_validator("filled_at")(
        lambda value: None if value is None else _timezone_aware(value)
    )

    @model_validator(mode="after")
    def validate_fill_state(self) -> Self:
        if self.status is PaperOrderStatus.FILLED and self.filled_at is None:
            raise ValueError("FILLED order requires filled_at")
        if self.status is not PaperOrderStatus.FILLED and self.filled_at is not None:
            raise ValueError("only a FILLED order may have filled_at")
        return self


class PaperPosition(StrictPaperModel):
    position_id: str = Field(min_length=1)
    symbol: str = Field(min_length=1)
    quantity: int = Field(gt=0)
    entry_price: float = Field(gt=0)
    stop_price: float = Field(gt=0)
    target_price: float = Field(gt=0)
    point_value: float = Field(gt=0)
    status: PaperPositionStatus
    opened_at: datetime
    closed_at: datetime | None = None
    close_price: float | None = Field(default=None, gt=0)
    close_reason: PaperCloseReason | None = None
    realized_pnl: float | None = None
    r_multiple: float | None = None

    _validate_opened_at = field_validator("opened_at")(_timezone_aware)
    _validate_closed_at = field_validator("closed_at")(
        lambda value: None if value is None else _timezone_aware(value)
    )

    @model_validator(mode="after")
    def validate_position_state(self) -> Self:
        if not self.stop_price < self.entry_price < self.target_price:
            raise ValueError("long paper position requires stop < entry < target")
        results = (
            self.closed_at,
            self.close_price,
            self.close_reason,
            self.realized_pnl,
            self.r_multiple,
        )
        if self.status is PaperPositionStatus.CLOSED and any(value is None for value in results):
            raise ValueError("closed position requires complete close accounting")
        if self.status is PaperPositionStatus.OPEN and any(value is not None for value in results):
            raise ValueError("open position cannot contain close accounting")
        return self


class PaperTradeRecord(StrictPaperModel):
    trade_id: str = Field(min_length=1)
    decision_id: str = Field(min_length=1)
    setup_type: TradeSetupType
    symbol: str = Field(min_length=1)
    entry_price: float = Field(gt=0)
    stop_price: float = Field(gt=0)
    target_price: float = Field(gt=0)
    quantity: int = Field(gt=0)
    point_value: float = Field(gt=0)
    risk_per_unit: float = Field(gt=0)
    reward_per_unit: float = Field(gt=0)
    planned_r_multiple: float = Field(gt=0)
    status: PaperPositionStatus
    close_price: float | None = Field(default=None, gt=0)
    close_reason: PaperCloseReason | None = None
    realized_pnl: float | None = None
    realized_r_multiple: float | None = None
    decision_reason: str = Field(min_length=1)
    created_at: datetime
    closed_at: datetime | None = None

    _validate_created_at = field_validator("created_at")(_timezone_aware)
    _validate_closed_at = field_validator("closed_at")(
        lambda value: None if value is None else _timezone_aware(value)
    )

    @model_validator(mode="after")
    def validate_trade_state(self) -> Self:
        if not self.stop_price < self.entry_price < self.target_price:
            raise ValueError("long paper trade requires stop < entry < target")
        results = (
            self.close_price,
            self.close_reason,
            self.realized_pnl,
            self.realized_r_multiple,
            self.closed_at,
        )
        if self.status is PaperPositionStatus.CLOSED and any(value is None for value in results):
            raise ValueError("closed trade requires complete close accounting")
        if self.status is PaperPositionStatus.OPEN and any(value is not None for value in results):
            raise ValueError("open trade cannot contain close accounting")
        return self


class PaperTradeEvent(StrictPaperModel):
    event_id: str = Field(min_length=1)
    event_type: PaperEventType
    trade_id: str = Field(min_length=1)
    symbol: str = Field(min_length=1)
    timestamp: datetime
    position_status: PaperPositionStatus
    last_price: float | None = Field(default=None, gt=0)
    close_reason: PaperCloseReason | None = None
    realized_pnl: float | None = None
    realized_r_multiple: float | None = None
    order: PaperOrder | None = None

    _validate_timestamp = field_validator("timestamp")(_timezone_aware)
