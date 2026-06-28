import importlib

import pytest
from pydantic import ValidationError

from orderflow_ibkr_agent.ibkr.adapter import IBKRReadOnlyAdapter
from orderflow_ibkr_agent.ibkr.backend import create_ib_insync_read_client
from orderflow_ibkr_agent.ibkr.config import IBKRConfig
from orderflow_ibkr_agent.ibkr.contracts import ContractSpec, futures_contract, stock_contract
from orderflow_ibkr_agent.ibkr.errors import (
    IBKRConnectionError,
    IBKRDependencyError,
    IBKRSafetyError,
)
from orderflow_ibkr_agent.ibkr.fakes import FakeIBKRReadOnlyClient
from orderflow_ibkr_agent.ibkr.safety import FORBIDDEN_ORDER_METHODS


def safe_config(**updates) -> IBKRConfig:
    return IBKRConfig(enabled=True, **updates)


def test_adapter_constructs_without_loading_optional_dependency() -> None:
    adapter = IBKRReadOnlyAdapter(IBKRConfig())
    assert adapter.is_connected() is False


def test_adapter_exposes_no_public_order_methods() -> None:
    for method_name in FORBIDDEN_ORDER_METHODS:
        assert not hasattr(IBKRReadOnlyAdapter, method_name)


def test_connect_refuses_disabled_config() -> None:
    adapter = IBKRReadOnlyAdapter(IBKRConfig(), FakeIBKRReadOnlyClient())
    with pytest.raises(IBKRSafetyError, match="explicitly enabled"):
        adapter.connect()


def test_connect_and_disconnect_with_safe_fake_client() -> None:
    fake = FakeIBKRReadOnlyClient()
    adapter = IBKRReadOnlyAdapter(safe_config(), fake)
    adapter.connect()
    assert adapter.is_connected() is True
    assert fake.connection_args["readonly"] is True
    assert fake.connection_args["port"] == 7497
    status = adapter.get_connection_status()
    assert status.safety_guard_passed is True
    assert status.masked_account == "DU*****67"
    adapter.disconnect()
    assert adapter.is_connected() is False


def test_connection_failure_is_wrapped_and_left_disconnected() -> None:
    fake = FakeIBKRReadOnlyClient(connect_error=RuntimeError("TWS unavailable"))
    adapter = IBKRReadOnlyAdapter(safe_config(), fake)
    with pytest.raises(IBKRConnectionError, match="TWS unavailable"):
        adapter.connect()
    assert fake.is_connected() is False


def test_non_paper_account_is_rejected_and_disconnected() -> None:
    fake = FakeIBKRReadOnlyClient(account_id="U1234567")
    adapter = IBKRReadOnlyAdapter(safe_config(), fake)
    with pytest.raises(IBKRSafetyError, match="paper account"):
        adapter.connect()
    assert fake.is_connected() is False


def test_account_summary_and_positions_are_read_from_fake() -> None:
    adapter = IBKRReadOnlyAdapter(safe_config(), FakeIBKRReadOnlyClient())
    adapter.connect()
    summary = adapter.get_account_summary()
    positions = adapter.get_positions()
    assert summary.account_id == "DU1234567"
    assert summary.values[0].tag == "NetLiquidation"
    assert positions[0].symbol == "ES"
    assert positions[0].quantity == 1


def test_market_snapshot_is_read_from_fake() -> None:
    adapter = IBKRReadOnlyAdapter(safe_config(), FakeIBKRReadOnlyClient())
    adapter.connect()
    snapshot = adapter.request_market_snapshot(stock_contract("AAPL"))
    assert snapshot.symbol == "AAPL"
    assert snapshot.last == 200.25


def test_read_operations_require_connection() -> None:
    adapter = IBKRReadOnlyAdapter(safe_config(), FakeIBKRReadOnlyClient())
    with pytest.raises(IBKRConnectionError, match="not connected"):
        adapter.get_positions()


def test_futures_contract_requires_explicit_expiry() -> None:
    with pytest.raises(ValidationError, match="expiry"):
        ContractSpec(
            security_type="FUT", symbol="ES", exchange="CME", currency="USD"
        )
    assert futures_contract("ES", expiry="202609").expiry == "202609"


def test_missing_ib_insync_raises_dependency_error(monkeypatch) -> None:
    real_import = importlib.import_module

    def missing(name: str):
        if name == "ib_insync":
            raise ModuleNotFoundError("ib_insync")
        return real_import(name)

    monkeypatch.setattr("orderflow_ibkr_agent.ibkr.backend.import_module", missing)
    with pytest.raises(IBKRDependencyError, match="optional IBKR dependency"):
        create_ib_insync_read_client()
