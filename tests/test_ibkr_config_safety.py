import pytest

from orderflow_ibkr_agent.ibkr.config import IBKRConfig, MarketDataType
from orderflow_ibkr_agent.ibkr.errors import IBKRSafetyError
from orderflow_ibkr_agent.ibkr.safety import validate_ibkr_safety


def enabled_config(**updates) -> IBKRConfig:
    return IBKRConfig(enabled=True, **updates)


def test_ibkr_config_has_safe_disabled_defaults() -> None:
    config = IBKRConfig()
    assert config.enabled is False
    assert config.readonly is True
    assert config.require_paper_account is True
    assert config.host == "127.0.0.1"
    assert config.port == 7497
    assert config.client_id == 101
    assert config.connection_timeout_seconds == 10
    assert config.market_data_type is MarketDataType.DELAYED


@pytest.mark.parametrize("port", [7496, 4001])
def test_live_ports_are_rejected_by_default(port: int) -> None:
    with pytest.raises(IBKRSafetyError, match="unsafe live"):
        validate_ibkr_safety(enabled_config(port=port))


@pytest.mark.parametrize("port", [7497, 4002])
def test_paper_ports_pass_preconnection_guard(port: int) -> None:
    validate_ibkr_safety(enabled_config(port=port))


def test_suspicious_port_requires_explicit_override() -> None:
    validate_ibkr_safety(enabled_config(port=7496, allow_unsafe_live_port=True))


def test_disabled_connection_is_rejected() -> None:
    with pytest.raises(IBKRSafetyError, match="explicitly enabled"):
        validate_ibkr_safety(IBKRConfig())


def test_non_readonly_connection_is_rejected() -> None:
    with pytest.raises(IBKRSafetyError, match="readonly"):
        validate_ibkr_safety(enabled_config(readonly=False))


def test_disabling_paper_account_requirement_is_rejected() -> None:
    with pytest.raises(IBKRSafetyError, match="paper account"):
        validate_ibkr_safety(enabled_config(require_paper_account=False))


@pytest.mark.parametrize(
    "method_name",
    [
        "place_order",
        "submit_order",
        "cancel_order",
        "modify_order",
        "bracket_order",
        "transmit_order",
    ],
)
def test_order_shaped_public_method_fails_surface_guard(method_name: str) -> None:
    unsafe_type = type("UnsafeAdapter", (), {method_name: lambda self: None})
    with pytest.raises(IBKRSafetyError, match=method_name):
        validate_ibkr_safety(enabled_config(), adapter_type=unsafe_type)
