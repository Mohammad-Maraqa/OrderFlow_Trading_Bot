import json
from pathlib import Path

import pytest
from typer.testing import CliRunner

from orderflow_ibkr_agent.cli import app


runner = CliRunner()
SAMPLES = Path(__file__).parents[1] / "samples"


def invoke_sample(name: str, journal: Path):
    return runner.invoke(
        app,
        ["evaluate-json", str(SAMPLES / name), "--journal-path", str(journal)],
    )


@pytest.mark.parametrize(
    ("sample", "expected_decision", "expected_setup"),
    [
        ("valid_failed_breakdown_long.json", "LONG_VALID", "FAILED_BREAKDOWN_LONG"),
        (
            "valid_pullback_continuation_long.json",
            "LONG_VALID",
            "PULLBACK_CONTINUATION_LONG",
        ),
        ("invalid_bad_risk_reward.json", "INVALID_RISK_REWARD", "FAILED_BREAKDOWN_LONG"),
        (
            "invalid_missing_confirmation.json",
            "WAITING_FOR_CONFIRMATION",
            "PULLBACK_CONTINUATION_LONG",
        ),
    ],
)
def test_evaluate_json_reports_expected_decision_and_journals_it(
    tmp_path: Path, sample: str, expected_decision: str, expected_setup: str
) -> None:
    journal = tmp_path / "decisions.jsonl"
    result = invoke_sample(sample, journal)
    assert result.exit_code == 0, result.output
    assert expected_decision in result.output
    assert expected_setup in result.output

    records = [json.loads(line) for line in journal.read_text(encoding="utf-8").splitlines()]
    assert len(records) == 1
    assert records[0]["decision"] == expected_decision


def test_invalid_short_sample_fails_validation_and_is_not_journaled(tmp_path: Path) -> None:
    journal = tmp_path / "decisions.jsonl"
    result = invoke_sample("invalid_short_attempt.json", journal)
    assert result.exit_code == 2
    assert "Input validation failed" in result.output
    assert "setup_type" in result.output
    assert not journal.exists()
