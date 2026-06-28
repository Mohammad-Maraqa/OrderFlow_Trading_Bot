from typing import Literal

from pydantic import BaseModel, Field

from orderflow_ibkr_agent.models import DecisionStatus, TradeDecision


class SimulatedOrder(BaseModel):
    symbol: str
    action: Literal["BUY"] = "BUY"
    order_type: Literal["LIMIT"] = "LIMIT"
    quantity: int = Field(default=1, gt=0)
    limit_price: float = Field(gt=0)
    stop_price: float = Field(gt=0)
    target_price: float = Field(gt=0)
    status: Literal["SIMULATED"] = "SIMULATED"


class PaperExecutor:
    """Create an in-memory fake order without any broker dependency."""

    def create_order(self, decision: TradeDecision, quantity: int = 1) -> SimulatedOrder:
        if decision.decision is not DecisionStatus.LONG_VALID:
            raise ValueError("Paper simulation requires a LONG_VALID decision")
        if decision.entry_price is None or decision.stop_price is None or decision.target_price is None:
            raise ValueError("LONG_VALID decision is missing prices")
        return SimulatedOrder(
            symbol=decision.symbol,
            quantity=quantity,
            limit_price=decision.entry_price,
            stop_price=decision.stop_price,
            target_price=decision.target_price,
        )

