from typing import Literal

from typing import Self

from pydantic import BaseModel, ConfigDict, Field, model_validator


class ContractSpec(BaseModel):
    model_config = ConfigDict(extra="forbid", str_strip_whitespace=True)

    security_type: Literal["FUT", "STK"]
    symbol: str = Field(min_length=1)
    exchange: str = Field(min_length=1)
    currency: str = Field(default="USD", min_length=1)
    expiry: str | None = None

    @model_validator(mode="after")
    def futures_require_expiry(self) -> Self:
        if self.security_type == "FUT" and not self.expiry:
            raise ValueError("FUT contract requires explicit expiry")
        if self.security_type == "STK" and self.expiry is not None:
            raise ValueError("STK contract must not include expiry")
        return self


def futures_contract(
    symbol: str, exchange: str = "CME", currency: str = "USD", expiry: str | None = None
) -> ContractSpec:
    return ContractSpec(
        security_type="FUT", symbol=symbol, exchange=exchange, currency=currency, expiry=expiry
    )


def stock_contract(symbol: str, exchange: str = "SMART", currency: str = "USD") -> ContractSpec:
    return ContractSpec(security_type="STK", symbol=symbol, exchange=exchange, currency=currency)


def etf_contract(symbol: str, exchange: str = "SMART", currency: str = "USD") -> ContractSpec:
    return stock_contract(symbol, exchange, currency)
