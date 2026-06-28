import pytest
from pydantic import ValidationError

from orderflow_ibkr_agent.models import RiskPlan, TradeSetupType


@pytest.mark.parametrize("forbidden", ["SHORT", "BEARISH_CONTINUATION_SHORT", "SELL_RESISTANCE"])
def test_forbidden_short_setup_cannot_be_constructed(forbidden: str) -> None:
    with pytest.raises(ValueError):
        TradeSetupType(forbidden)


def test_risk_plan_rejects_short_geometry() -> None:
    with pytest.raises(ValidationError, match="long trade"):
        RiskPlan(
            proposed_entry=100,
            proposed_stop=102,
            proposed_target=95,
            max_risk_points=5,
        )

