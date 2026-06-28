from typer.testing import CliRunner

import orderflow_ibkr_agent.cli as cli_module
from orderflow_ibkr_agent.cli import app
from orderflow_ibkr_agent.ibkr.adapter import IBKRReadOnlyAdapter
from orderflow_ibkr_agent.ibkr.fakes import FakeIBKRReadOnlyClient


runner = CliRunner()


def enable_safe_ibkr(monkeypatch) -> None:
    monkeypatch.setenv("ORDERFLOW_IBKR_ENABLED", "true")
    monkeypatch.setenv("ORDERFLOW_IBKR_HOST", "127.0.0.1")
    monkeypatch.setenv("ORDERFLOW_IBKR_PORT", "7497")
    monkeypatch.setenv("ORDERFLOW_IBKR_CLIENT_ID", "101")
    monkeypatch.setenv("ORDERFLOW_IBKR_READONLY", "true")
    monkeypatch.setenv("ORDERFLOW_IBKR_REQUIRE_PAPER", "true")


def install_fake_adapter(monkeypatch) -> FakeIBKRReadOnlyClient:
    fake = FakeIBKRReadOnlyClient()
    monkeypatch.setattr(
        cli_module,
        "_create_ibkr_adapter",
        lambda config: IBKRReadOnlyAdapter(config, fake),
    )
    return fake


def test_ibkr_status_disabled_mode_is_safe(monkeypatch, tmp_path) -> None:
    monkeypatch.chdir(tmp_path)
    monkeypatch.delenv("ORDERFLOW_IBKR_ENABLED", raising=False)
    result = runner.invoke(app, ["ibkr-status"])
    assert result.exit_code == 0
    assert "disabled" in result.output.lower()
    assert "ORDERFLOW_IBKR_ENABLED=true" in result.output


def test_ibkr_status_uses_fake_and_masks_account(monkeypatch) -> None:
    enable_safe_ibkr(monkeypatch)
    fake = install_fake_adapter(monkeypatch)
    result = runner.invoke(app, ["ibkr-status"])
    assert result.exit_code == 0, result.output
    assert "Safety guard: PASSED" in result.output
    assert "DU*****67" in result.output
    assert "Connected: yes" in result.output
    assert fake.is_connected() is False


def test_ibkr_positions_reads_only_from_fake(monkeypatch) -> None:
    enable_safe_ibkr(monkeypatch)
    fake = install_fake_adapter(monkeypatch)
    result = runner.invoke(app, ["ibkr-positions"])
    assert result.exit_code == 0, result.output
    assert "ES" in result.output
    assert "FUT" in result.output
    assert "1.0" in result.output
    assert fake.is_connected() is False


def test_ibkr_snapshot_reads_stock_snapshot_from_fake(monkeypatch) -> None:
    enable_safe_ibkr(monkeypatch)
    fake = install_fake_adapter(monkeypatch)
    result = runner.invoke(
        app,
        [
            "ibkr-snapshot",
            "--symbol",
            "AAPL",
            "--security-type",
            "STK",
            "--exchange",
            "SMART",
            "--currency",
            "USD",
        ],
    )
    assert result.exit_code == 0, result.output
    assert "AAPL" in result.output
    assert "200.25" in result.output
    assert fake.is_connected() is False


def test_ibkr_snapshot_refuses_to_guess_futures_expiry(monkeypatch) -> None:
    enable_safe_ibkr(monkeypatch)
    install_fake_adapter(monkeypatch)
    result = runner.invoke(
        app,
        [
            "ibkr-snapshot",
            "--symbol",
            "ES",
            "--security-type",
            "FUT",
            "--exchange",
            "CME",
        ],
    )
    assert result.exit_code == 2
    assert "expiry" in result.output.lower()
