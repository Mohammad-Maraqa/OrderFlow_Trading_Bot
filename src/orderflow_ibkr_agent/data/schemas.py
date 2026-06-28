from pydantic import BaseModel

from orderflow_ibkr_agent.models import MarketSnapshot, OrderFlowSnapshot, RiskPlan


class EvaluationInput(BaseModel):
    market: MarketSnapshot
    order_flow: OrderFlowSnapshot
    risk_plan: RiskPlan

