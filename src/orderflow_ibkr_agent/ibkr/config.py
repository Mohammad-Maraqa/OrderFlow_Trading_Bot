from enum import StrEnum

from pydantic import AliasChoices, Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class MarketDataType(StrEnum):
    LIVE = "live"
    FROZEN = "frozen"
    DELAYED = "delayed"
    DELAYED_FROZEN = "delayed_frozen"


class IBKRConfig(BaseSettings):
    """Environment-backed configuration for observation-only IBKR access."""

    model_config = SettingsConfigDict(
        env_prefix="ORDERFLOW_IBKR_",
        env_file=".env",
        extra="ignore",
        populate_by_name=True,
    )

    host: str = Field(default="127.0.0.1", min_length=1)
    port: int = Field(default=7497, ge=1, le=65535)
    client_id: int = Field(default=101, ge=0)
    readonly: bool = True
    require_paper_account: bool = Field(
        default=True,
        validation_alias=AliasChoices(
            "require_paper_account",
            "ORDERFLOW_IBKR_REQUIRE_PAPER",
            "ORDERFLOW_IBKR_REQUIRE_PAPER_ACCOUNT",
        ),
    )
    connection_timeout_seconds: float = Field(default=10.0, gt=0)
    market_data_type: MarketDataType = MarketDataType.DELAYED
    enabled: bool = False
    allow_unsafe_live_port: bool = False

