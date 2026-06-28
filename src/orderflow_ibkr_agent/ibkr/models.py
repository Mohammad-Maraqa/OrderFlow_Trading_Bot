from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class ReadOnlyModel(BaseModel):
    model_config = ConfigDict(extra="forbid")


class IBKRAccountValue(ReadOnlyModel):
    tag: str
    value: str
    currency: str = ""


class IBKRAccountSummary(ReadOnlyModel):
    account_id: str
    values: list[IBKRAccountValue] = Field(default_factory=list)


class IBKRPosition(ReadOnlyModel):
    account_id: str
    symbol: str
    security_type: str
    exchange: str = ""
    currency: str = ""
    quantity: float
    average_cost: float


class IBKRMarketSnapshot(ReadOnlyModel):
    symbol: str
    security_type: str
    bid: float | None = None
    ask: float | None = None
    last: float | None = None
    close: float | None = None
    timestamp: datetime


class IBKRConnectionStatus(ReadOnlyModel):
    enabled: bool
    connected: bool
    readonly: bool
    host: str
    port: int
    client_id: int
    safety_guard_passed: bool
    masked_account: str | None = None

