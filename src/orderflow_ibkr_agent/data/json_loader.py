import json
from pathlib import Path

from pydantic import ValidationError

from orderflow_ibkr_agent.data.input_models import MarketDecisionInput


class MarketInputError(ValueError):
    """A readable error at the external JSON input boundary."""


def _format_validation_error(error: ValidationError) -> str:
    messages: list[str] = []
    for item in error.errors(include_url=False, include_context=False):
        location = ".".join(str(part) for part in item["loc"])
        messages.append(f"{location}: {item['msg']}")
    return "; ".join(messages)


def load_market_decision_input(path: str | Path) -> MarketDecisionInput:
    input_path = Path(path)
    try:
        raw = input_path.read_text(encoding="utf-8")
    except OSError as error:
        raise MarketInputError(f"Unable to read JSON input '{input_path}': {error}") from error

    try:
        payload = json.loads(raw)
    except json.JSONDecodeError as error:
        raise MarketInputError(
            f"Invalid JSON in '{input_path}' at line {error.lineno}, column {error.colno}: {error.msg}"
        ) from error

    try:
        return MarketDecisionInput.model_validate(payload)
    except ValidationError as error:
        raise MarketInputError(
            f"Input validation failed for '{input_path}': {_format_validation_error(error)}"
        ) from error

