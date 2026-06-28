import math
from datetime import datetime, timezone
from importlib import import_module
from typing import Any

from orderflow_ibkr_agent.ibkr.config import MarketDataType
from orderflow_ibkr_agent.ibkr.contracts import ContractSpec
from orderflow_ibkr_agent.ibkr.errors import IBKRDependencyError
from orderflow_ibkr_agent.ibkr.models import (
    IBKRAccountSummary,
    IBKRAccountValue,
    IBKRMarketSnapshot,
    IBKRPosition,
)


MARKET_DATA_CODES = {
    MarketDataType.LIVE: 1,
    MarketDataType.FROZEN: 2,
    MarketDataType.DELAYED: 3,
    MarketDataType.DELAYED_FROZEN: 4,
}


def _number(value: Any) -> float | None:
    if isinstance(value, (int, float)) and math.isfinite(value):
        return float(value)
    return None


class IBInsyncReadClient:
    """Narrow read wrapper around ib_insync; no order methods are wrapped."""

    def __init__(self, module: Any) -> None:
        self._module = module
        self._ib = module.IB()

    def connect(
        self,
        host: str,
        port: int,
        client_id: int,
        timeout: float,
        readonly: bool,
    ) -> None:
        self._ib.connect(
            host,
            port,
            clientId=client_id,
            timeout=timeout,
            readonly=readonly,
        )

    def disconnect(self) -> None:
        if self._ib.isConnected():
            self._ib.disconnect()

    def is_connected(self) -> bool:
        return bool(self._ib.isConnected())

    def get_account_summary(self) -> IBKRAccountSummary:
        rows = list(self._ib.accountSummary())
        account_id = next((row.account for row in rows if row.account), "")
        return IBKRAccountSummary(
            account_id=account_id,
            values=[
                IBKRAccountValue(
                    tag=str(row.tag), value=str(row.value), currency=str(row.currency or "")
                )
                for row in rows
            ],
        )

    def get_positions(self) -> list[IBKRPosition]:
        return [
            IBKRPosition(
                account_id=str(row.account),
                symbol=str(row.contract.symbol),
                security_type=str(row.contract.secType),
                exchange=str(row.contract.exchange or ""),
                currency=str(row.contract.currency or ""),
                quantity=float(row.position),
                average_cost=float(row.avgCost),
            )
            for row in self._ib.positions()
        ]

    def request_market_snapshot(
        self, contract: ContractSpec, market_data_type: MarketDataType
    ) -> IBKRMarketSnapshot:
        if contract.security_type == "FUT":
            ib_contract = self._module.Future(
                contract.symbol,
                contract.expiry,
                contract.exchange,
                currency=contract.currency,
            )
        else:
            ib_contract = self._module.Stock(
                contract.symbol, contract.exchange, contract.currency
            )
        self._ib.reqMarketDataType(MARKET_DATA_CODES[market_data_type])
        ticker = self._ib.reqMktData(ib_contract, "", True, False)
        self._ib.sleep(1.0)
        timestamp = ticker.time
        if timestamp is None:
            timestamp = datetime.now(timezone.utc)
        elif timestamp.tzinfo is None:
            timestamp = timestamp.replace(tzinfo=timezone.utc)
        return IBKRMarketSnapshot(
            symbol=contract.symbol,
            security_type=contract.security_type,
            bid=_number(ticker.bid),
            ask=_number(ticker.ask),
            last=_number(ticker.last),
            close=_number(ticker.close),
            timestamp=timestamp,
        )


def create_ib_insync_read_client() -> IBInsyncReadClient:
    try:
        module = import_module("ib_insync")
    except ModuleNotFoundError as error:
        raise IBKRDependencyError(
            "The optional IBKR dependency is not installed; use pip install -e '.[ibkr]'"
        ) from error
    return IBInsyncReadClient(module)

