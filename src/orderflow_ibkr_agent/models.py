from datetime import datetime
from enum import StrEnum
from typing import Self

from pydantic import BaseModel, Field, model_validator


class SessionType(StrEnum):
    RTH = "RTH"
    ETH = "ETH"


class MarketState(StrEnum):
    BULLISH = "bullish"
    BEARISH = "bearish"
    BALANCED = "balanced"
    IMBALANCED_UP = "imbalanced_up"
    IMBALANCED_DOWN = "imbalanced_down"
    UNCERTAIN = "uncertain"


class TradeSetupType(StrEnum):
    FAILED_BREAKDOWN_LONG = "FAILED_BREAKDOWN_LONG"
    PULLBACK_CONTINUATION_LONG = "PULLBACK_CONTINUATION_LONG"
    VALUE_RECLAIM_LONG = "VALUE_RECLAIM_LONG"
    BREAKOUT_PULLBACK_LONG = "BREAKOUT_PULLBACK_LONG"
    DEVIATION_REJECTION_LONG = "DEVIATION_REJECTION_LONG"


class DecisionStatus(StrEnum):
    LONG_VALID = "LONG_VALID"
    NO_TRADE = "NO_TRADE"
    WAITING_FOR_CONFIRMATION = "WAITING_FOR_CONFIRMATION"
    INVALID_CONTEXT = "INVALID_CONTEXT"
    INVALID_LOCATION = "INVALID_LOCATION"
    INVALID_ORDERFLOW = "INVALID_ORDERFLOW"
    INVALID_RISK_REWARD = "INVALID_RISK_REWARD"
    ENTRY_TOO_LATE = "ENTRY_TOO_LATE"
    DATA_MISSING = "DATA_MISSING"


class DataQuality(BaseModel):
    value_area_complete: bool = True
    vwap_complete: bool = True
    order_flow_complete: bool = True
    risk_plan_complete: bool = True
    stale_data: bool = False
    notes: list[str] = Field(default_factory=list)

    @property
    def is_usable(self) -> bool:
        return (
            self.value_area_complete
            and self.vwap_complete
            and self.order_flow_complete
            and self.risk_plan_complete
            and not self.stale_data
        )


class ValueAreaSnapshot(BaseModel):
    previous_vah: float
    previous_val: float
    previous_poc: float
    current_vah: float
    current_val: float
    current_poc: float
    composite_value_high: float
    composite_value_low: float


class VWAPSnapshot(BaseModel):
    vwap: float
    vwap_upper_1: float
    vwap_upper_2: float
    vwap_lower_1: float
    vwap_lower_2: float


class MarketSnapshot(BaseModel):
    symbol: str = Field(min_length=1)
    timestamp: datetime
    last_price: float = Field(gt=0)
    session_type: SessionType
    previous_vah: float
    previous_val: float
    previous_poc: float
    current_vah: float
    current_val: float
    current_poc: float
    vwap: float
    vwap_upper_1: float
    vwap_upper_2: float
    vwap_lower_1: float
    vwap_lower_2: float
    composite_value_high: float
    composite_value_low: float
    market_state: MarketState
    data_quality: DataQuality = Field(default_factory=DataQuality)

    @property
    def value_area(self) -> ValueAreaSnapshot:
        return ValueAreaSnapshot.model_validate(self.model_dump())

    @property
    def vwap_snapshot(self) -> VWAPSnapshot:
        return VWAPSnapshot.model_validate(self.model_dump())


class OrderFlowSnapshot(BaseModel):
    aggressive_buy_volume: float = Field(ge=0)
    aggressive_sell_volume: float = Field(ge=0)
    delta: float
    cumulative_delta: float
    seller_absorption_detected: bool
    buyer_absorption_detected: bool
    passive_buy_liquidity: float = Field(ge=0)
    passive_sell_liquidity: float = Field(ge=0)
    liquidity_wall_nearby: bool
    iceberg_detected: bool
    stop_run_detected: bool
    tape_speed: float = Field(ge=0)
    volume_outlier_score: float = Field(ge=0)
    price_reclaimed_level: bool
    price_accepted_above_level: bool
    price_accepted_below_level: bool


class RiskPlan(BaseModel):
    proposed_entry: float = Field(gt=0)
    proposed_stop: float = Field(gt=0)
    proposed_target: float = Field(gt=0)
    max_risk_points: float = Field(gt=0)
    min_reward_risk_ratio: float = Field(default=2.0, gt=0)
    position_size: int | None = Field(default=None, gt=0)

    @model_validator(mode="after")
    def enforce_long_geometry(self) -> Self:
        if not self.proposed_stop < self.proposed_entry < self.proposed_target:
            raise ValueError("long trade requires stop < entry < target")
        return self


class TradeDecision(BaseModel):
    symbol: str
    timestamp: datetime
    market_state: MarketState
    setup_type: TradeSetupType | None
    decision: DecisionStatus
    confidence_score: float = Field(ge=0, le=1)
    reasons: list[str] = Field(min_length=1)
    warnings: list[str] = Field(default_factory=list)
    entry_price: float | None = None
    stop_price: float | None = None
    target_price: float | None = None
    risk_points: float | None = None
    reward_points: float | None = None
    risk_reward_ratio: float | None = None
    invalidation_level: float | None = None
    data_quality: DataQuality = Field(default_factory=DataQuality)

    @model_validator(mode="after")
    def require_complete_valid_long(self) -> Self:
        if self.decision is DecisionStatus.LONG_VALID:
            required = (
                self.setup_type,
                self.entry_price,
                self.stop_price,
                self.target_price,
                self.risk_points,
                self.reward_points,
                self.risk_reward_ratio,
                self.invalidation_level,
            )
            if any(value is None for value in required):
                raise ValueError("LONG_VALID requires a setup and complete risk details")
        return self

