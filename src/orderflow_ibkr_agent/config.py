from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Environment-backed strategy settings; IBKR has a separate safe config."""

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    max_risk_per_trade: float = Field(default=100.0, gt=0)
    max_daily_loss: float = Field(default=300.0, gt=0)
    min_reward_risk: float = Field(default=2.0, gt=0)
    default_symbol: str = "ES"


@lru_cache
def get_settings() -> Settings:
    return Settings()
