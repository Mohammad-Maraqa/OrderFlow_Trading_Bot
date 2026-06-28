from typing import Protocol

from orderflow_ibkr_agent.ibkr.config import IBKRConfig, MarketDataType
from orderflow_ibkr_agent.ibkr.contracts import ContractSpec
from orderflow_ibkr_agent.ibkr.errors import (
    IBKRConnectionError,
    IBKRDependencyError,
    IBKRSafetyError,
)
from orderflow_ibkr_agent.ibkr.models import (
    IBKRAccountSummary,
    IBKRConnectionStatus,
    IBKRMarketSnapshot,
    IBKRPosition,
)
from orderflow_ibkr_agent.ibkr.safety import validate_ibkr_safety


class IBKRReadClient(Protocol):
    def connect(
        self,
        host: str,
        port: int,
        client_id: int,
        timeout: float,
        readonly: bool,
    ) -> None: ...

    def disconnect(self) -> None: ...

    def is_connected(self) -> bool: ...

    def get_account_summary(self) -> IBKRAccountSummary: ...

    def get_positions(self) -> list[IBKRPosition]: ...

    def request_market_snapshot(
        self, contract: ContractSpec, market_data_type: MarketDataType
    ) -> IBKRMarketSnapshot: ...


def mask_account_identifier(account_id: str) -> str:
    if len(account_id) <= 4:
        return account_id[:2] + "**"
    return account_id[:2] + "*" * (len(account_id) - 4) + account_id[-2:]


class IBKRReadOnlyAdapter:
    """Observation-only IBKR boundary with no broker order capability."""

    def __init__(
        self, config: IBKRConfig | None = None, client: IBKRReadClient | None = None
    ) -> None:
        self.config = config or IBKRConfig()
        self._client = client
        self._account_summary: IBKRAccountSummary | None = None

    def connect(self) -> None:
        validate_ibkr_safety(self.config, type(self))
        if self._client is None:
            from orderflow_ibkr_agent.ibkr.backend import create_ib_insync_read_client

            self._client = create_ib_insync_read_client()
        try:
            self._client.connect(
                host=self.config.host,
                port=self.config.port,
                client_id=self.config.client_id,
                timeout=self.config.connection_timeout_seconds,
                readonly=self.config.readonly,
            )
            if not self._client.is_connected():
                raise IBKRConnectionError("IBKR client did not report a connected state")
            summary = self._client.get_account_summary()
            if self.config.require_paper_account and not summary.account_id.upper().startswith("DU"):
                raise IBKRSafetyError(
                    "Connected account does not look like an IBKR paper account"
                )
            self._account_summary = summary
        except (IBKRDependencyError, IBKRSafetyError):
            self.disconnect()
            raise
        except Exception as error:
            self.disconnect()
            if isinstance(error, IBKRConnectionError):
                raise
            raise IBKRConnectionError(f"IBKR read-only connection failed: {error}") from error

    def disconnect(self) -> None:
        if self._client is not None:
            self._client.disconnect()
        self._account_summary = None

    def is_connected(self) -> bool:
        return self._client is not None and self._client.is_connected()

    def get_connection_status(self) -> IBKRConnectionStatus:
        safety_passed = False
        try:
            validate_ibkr_safety(self.config, type(self))
            safety_passed = True
        except IBKRSafetyError:
            pass
        return IBKRConnectionStatus(
            enabled=self.config.enabled,
            connected=self.is_connected(),
            readonly=self.config.readonly,
            host=self.config.host,
            port=self.config.port,
            client_id=self.config.client_id,
            safety_guard_passed=safety_passed,
            masked_account=(
                mask_account_identifier(self._account_summary.account_id)
                if self._account_summary is not None
                else None
            ),
        )

    def get_account_summary(self) -> IBKRAccountSummary:
        self._require_connected()
        if self._account_summary is None:
            self._account_summary = self._client.get_account_summary()
        return self._account_summary

    def get_positions(self) -> list[IBKRPosition]:
        self._require_connected()
        return self._client.get_positions()

    def request_market_snapshot(self, contract: ContractSpec) -> IBKRMarketSnapshot:
        self._require_connected()
        return self._client.request_market_snapshot(contract, self.config.market_data_type)

    def _require_connected(self) -> None:
        if not self.is_connected():
            raise IBKRConnectionError("IBKR read-only adapter is not connected")

