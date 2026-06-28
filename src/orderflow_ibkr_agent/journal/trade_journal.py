from pathlib import Path

from orderflow_ibkr_agent.models import TradeDecision


class TradeJournal:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def append(self, decision: TradeDecision) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        with self.path.open("a", encoding="utf-8") as stream:
            stream.write(decision.model_dump_json() + "\n")

