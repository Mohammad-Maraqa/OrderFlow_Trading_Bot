from datetime import datetime
from enum import StrEnum
from typing import Any, Annotated, Self

from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator

from orderflow_ibkr_agent.models import TradeSetupType


PositiveFloat = Annotated[float, Field(gt=0)]
NonNegativeFloat = Annotated[float, Field(ge=0)]


class StrictInputModel(BaseModel):
    model_config = ConfigDict(extra="forbid", str_strip_whitespace=True)


class AssetType(StrEnum):
    FUTURES = "futures"
    STOCK = "stock"
    ETF = "etf"
    FOREX = "forex"
    CRYPTO = "crypto"


class InputSession(StrEnum):
    RTH = "RTH"
    ETH = "ETH"


class ContextBias(StrEnum):
    BULLISH = "bullish"
    BEARISH = "bearish"
    BALANCED = "balanced"
    UNCERTAIN = "uncertain"


class ValueMarketState(StrEnum):
    BALANCE = "balance"
    BULLISH_IMBALANCE = "bullish_imbalance"
    BEARISH_IMBALANCE = "bearish_imbalance"
    ACCEPTANCE_HIGHER = "acceptance_higher"
    ACCEPTANCE_LOWER = "acceptance_lower"
    REJECTION_HIGHER = "rejection_higher"
    REJECTION_LOWER = "rejection_lower"
    NO_TRADE = "no_trade"


class TapeSpeed(StrEnum):
    SLOW = "slow"
    NORMAL = "normal"
    FAST = "fast"


class OrderFlowConfidence(StrEnum):
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"


class InstrumentSnapshot(StrictInputModel):
    symbol: str = Field(min_length=1)
    exchange: str = Field(min_length=1)
    asset_type: AssetType
    tick_size: PositiveFloat
    point_value: PositiveFloat
    session: InputSession
    timestamp: datetime

    @field_validator("timestamp")
    @classmethod
    def timestamp_must_be_timezone_aware(cls, value: datetime) -> datetime:
        if value.tzinfo is None or value.utcoffset() is None:
            raise ValueError("timestamp must include a timezone")
        return value


class ValueContextSnapshot(StrictInputModel):
    previous_rth_vah: PositiveFloat
    previous_rth_val: PositiveFloat
    previous_rth_poc: PositiveFloat
    current_rth_vah: PositiveFloat
    current_rth_val: PositiveFloat
    current_rth_poc: PositiveFloat
    current_vwap: PositiveFloat
    vwap_upper_1: PositiveFloat
    vwap_upper_2: PositiveFloat
    vwap_lower_1: PositiveFloat
    vwap_lower_2: PositiveFloat
    composite_value_high: PositiveFloat
    composite_value_low: PositiveFloat
    context_bias: ContextBias
    market_state: ValueMarketState

    @model_validator(mode="after")
    def validate_profile_geometry(self) -> Self:
        if not self.previous_rth_val <= self.previous_rth_poc <= self.previous_rth_vah:
            raise ValueError("previous RTH levels require VAL <= POC <= VAH")
        if not self.current_rth_val <= self.current_rth_poc <= self.current_rth_vah:
            raise ValueError("current RTH levels require VAL <= POC <= VAH")
        if not (
            self.vwap_lower_2
            < self.vwap_lower_1
            < self.current_vwap
            < self.vwap_upper_1
            < self.vwap_upper_2
        ):
            raise ValueError("VWAP levels must be ordered lower_2 < lower_1 < VWAP < upper_1 < upper_2")
        if self.composite_value_low >= self.composite_value_high:
            raise ValueError("composite value low must be below composite value high")
        return self


class OrderFlowSnapshot(StrictInputModel):
    aggressive_buy_volume: NonNegativeFloat
    aggressive_sell_volume: NonNegativeFloat
    delta: float
    cumulative_delta: float
    session_volume: NonNegativeFloat
    volume_spike_detected: bool
    seller_absorption_detected: bool
    buyer_absorption_detected: bool
    trapped_sellers_detected: bool
    trapped_buyers_detected: bool
    passive_buy_liquidity_detected: bool
    passive_sell_liquidity_detected: bool
    liquidity_wall_price: PositiveFloat | None
    large_participant_level: PositiveFloat | None
    tape_speed: TapeSpeed
    orderflow_confidence: OrderFlowConfidence


class LongSetupCandidate(StrictInputModel):
    setup_type: TradeSetupType
    entry_price: PositiveFloat
    stop_price: PositiveFloat
    target_price: PositiveFloat
    location_type: str = Field(min_length=1)
    confirmation_type: str = Field(min_length=1)
    reason: str = Field(min_length=1)

    @model_validator(mode="after")
    def enforce_long_only_candidate(self) -> Self:
        if not self.stop_price < self.entry_price < self.target_price:
            raise ValueError("long setup requires stop < entry < target")
        intent = f"{self.location_type} {self.confirmation_type}".upper()
        forbidden = ("SHORT", "SELL_RESISTANCE", "BEARISH_CONTINUATION")
        if any(token in intent for token in forbidden):
            raise ValueError("short-side intent is forbidden")
        return self


class MarketDecisionInput(StrictInputModel):
    instrument: InstrumentSnapshot
    value_context: ValueContextSnapshot
    order_flow: OrderFlowSnapshot
    setup_candidate: LongSetupCandidate
    metadata: dict[str, Any] | None = None
