import json
from pathlib import Path

from pydantic import BaseModel, ConfigDict, Field, ValidationError

from orderflow_ibkr_agent.paper.models import (
    PaperPosition,
    PaperTradeEvent,
    PaperTradeRecord,
)


class PaperPersistenceError(RuntimeError):
    pass


class PaperState(BaseModel):
    model_config = ConfigDict(extra="forbid")

    schema_version: int = Field(default=1, ge=1)
    positions: list[PaperPosition] = Field(default_factory=list)
    trade_records: list[PaperTradeRecord] = Field(default_factory=list)


class PaperStateStore:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def load(self) -> PaperState:
        if not self.path.exists():
            return PaperState()
        try:
            return PaperState.model_validate_json(self.path.read_text(encoding="utf-8"))
        except (OSError, ValidationError) as error:
            raise PaperPersistenceError(
                f"Unable to load paper state '{self.path}': {error}"
            ) from error

    def save(self, state: PaperState) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        temporary = self.path.with_suffix(self.path.suffix + ".tmp")
        try:
            temporary.write_text(state.model_dump_json(indent=2), encoding="utf-8")
            temporary.replace(self.path)
        except OSError as error:
            raise PaperPersistenceError(
                f"Unable to save paper state '{self.path}': {error}"
            ) from error


class PaperTradeJournal:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def append(self, event: PaperTradeEvent) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        try:
            with self.path.open("a", encoding="utf-8") as stream:
                stream.write(event.model_dump_json() + "\n")
        except OSError as error:
            raise PaperPersistenceError(
                f"Unable to append paper journal '{self.path}': {error}"
            ) from error

    def read(self) -> list[dict]:
        if not self.path.exists():
            return []
        try:
            return [
                json.loads(line)
                for line in self.path.read_text(encoding="utf-8").splitlines()
                if line.strip()
            ]
        except (OSError, json.JSONDecodeError) as error:
            raise PaperPersistenceError(
                f"Unable to read paper journal '{self.path}': {error}"
            ) from error

