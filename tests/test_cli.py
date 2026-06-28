from typer.testing import CliRunner

from orderflow_ibkr_agent.cli import app


runner = CliRunner()


def test_evaluate_sample_runs_offline() -> None:
    result = runner.invoke(app, ["evaluate-sample"])
    assert result.exit_code == 0
    assert "LONG_VALID" in result.stdout


def test_validate_long_only_reports_guard_active() -> None:
    result = runner.invoke(app, ["validate-long-only"])
    assert result.exit_code == 0
    assert "PASS" in result.stdout


def test_paper_sim_sample_does_not_connect_to_ibkr() -> None:
    result = runner.invoke(app, ["paper-sim-sample"])
    assert result.exit_code == 0
    assert "SIMULATED" in result.stdout
    assert "BUY" in result.stdout
