from datetime import datetime, timezone
from typing import Any

from orderflow_ibkr_agent.ibkr.config import MarketDataType
from orderflow_ibkr_agent.ibkr.contracts import ContractSpec
from orderflow_ibkr_agent.ibkr.models import (
    IBKRAccountSummary,
    IBKRAccountValue,
    IBKRMarketSnapshot,
    IBKRPosition,
)


class FakeIBKRReadOnlyClient:
    """Deterministic read-only client used by tests; it has no order API."""

    def __init__(
        self,
        account_id: str = "DU1234567",
        connect_error: Exception | None = None,
    ) -> None:
        self.account_id = account_id
        self.connect_error = connect_error
        self.connection_args: dict[str, Any] = {}
        self._connected = False

    def connect(
        self,
        host: str,
        port: int,
        client_id: int,
        timeout: float,
        readonly: bool,
    ) -> None:
        self.connection_args = {
            "host": host,
            "port": port,
            "client_id": client_id,
            "timeout": timeout,
            "readonly": readonly,
        }
        if self.connect_error is not None:
            raise self.connect_error
        self._connected = True

    def disconnect(self) -> None:
        self._connected = False

    def is_connected(self) -> bool:
        return self._connected

    def get_account_summary(self) -> IBKRAccountSummary:
        return IBKRAccountSummary(
            account_id=self.account_id,
            values=[
                IBKRAccountValue(
                    tag="NetLiquidation", value="100000.00", currency="USD"
                )
            ],
        )

    def get_positions(self) -> list[IBKRPosition]:
        return [
            IBKRPosition(
                account_id=self.account_id,
                symbol="ES",
                security_type="FUT",
                exchange="CME",
                currency="USD",
                quantity=1,
                average_cost=6000.25,
            )
        ]

    def request_market_snapshot(
        self, contract: ContractSpec, market_data_type: MarketDataType
    ) -> IBKRMarketSnapshot:
        return IBKRMarketSnapshot(
            symbol=contract.symbol,
            security_type=contract.security_type,
            bid=200.0,
            ask=200.5,
            last=200.25,
            close=199.75,
            timestamp=datetime(2026, 6, 24, 14, 30, tzinfo=timezone.utc),
        )

