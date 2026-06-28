import json
from pathlib import Path

from typer.testing import CliRunner

from orderflow_ibkr_agent.cli import app


runner = CliRunner()
SAMPLES = Path(__file__).parents[1] / "samples"


def paper_paths(tmp_path: Path) -> tuple[Path, Path]:
    return tmp_path / "paper_positions.json", tmp_path / "paper_trades.jsonl"


def open_args(sample: str, state: Path, journal: Path) -> list[str]:
    return [
        "paper-open-from-json",
        str(SAMPLES / sample),
        "--positions-path",
        str(state),
        "--paper-journal-path",
        str(journal),
    ]


def update_args(symbol: str, price: float, state: Path, journal: Path) -> list[str]:
    return [
        "paper-update-price",
        "--symbol",
        symbol,
        "--price",
        str(price),
        "--positions-path",
        str(state),
        "--paper-journal-path",
        str(journal),
    ]


def positions_args(state: Path, journal: Path) -> list[str]:
    return [
        "paper-positions",
        "--positions-path",
        str(state),
        "--paper-journal-path",
        str(journal),
    ]


def test_paper_open_update_target_and_list_flow_persists_across_commands(
    tmp_path: Path,
) -> None:
    state, journal = paper_paths(tmp_path)

    opened = runner.invoke(
        app, open_args("valid_failed_breakdown_long.json", state, journal)
    )
    assert opened.exit_code == 0, opened.output
    assert "Paper trade opened" in opened.output
    assert "FAILED_BREAKDOWN_LONG" in opened.output

    listed = runner.invoke(app, positions_args(state, journal))
    assert listed.exit_code == 0
    assert "ES" in listed.output
    assert "OPEN" in listed.output

    held = runner.invoke(app, update_args("ES", 6003.0, state, journal))
    assert held.exit_code == 0, held.output
    assert "OPEN" in held.output

    closed = runner.invoke(app, update_args("ES", 6006.25, state, journal))
    assert closed.exit_code == 0, closed.output
    assert "CLOSED" in closed.output
    assert "TARGET_HIT" in closed.output
    assert "300.00" in closed.output
    assert "2.40R" in closed.output

    empty = runner.invoke(app, positions_args(state, journal))
    assert empty.exit_code == 0
    assert "No open paper positions" in empty.output

    persisted = json.loads(state.read_text(encoding="utf-8"))
    assert persisted["positions"][0]["status"] == "CLOSED"
    assert len(journal.read_text(encoding="utf-8").splitlines()) == 4


def test_non_valid_json_decision_does_not_create_paper_state(tmp_path: Path) -> None:
    state, journal = paper_paths(tmp_path)
    result = runner.invoke(
        app, open_args("invalid_missing_confirmation.json", state, journal)
    )
    assert result.exit_code == 0, result.output
    assert "Paper trade not opened" in result.output
    assert "WAITING_FOR_CONFIRMATION" in result.output
    assert not state.exists()
    assert not journal.exists()
