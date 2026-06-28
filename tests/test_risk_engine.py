from orderflow_ibkr_agent.models import DecisionStatus, RiskPlan
from orderflow_ibkr_agent.strategy.risk_engine import RiskEngine


def test_valid_risk_plan_calculates_reward_to_risk(valid_risk_plan: RiskPlan) -> None:
    result = RiskEngine().evaluate(valid_risk_plan)
    assert result.status is None
    assert result.risk_points == 2.5
    assert result.reward_points == 6.0
    assert result.risk_reward_ratio == 2.4


def test_bad_reward_to_risk_is_rejected() -> None:
    plan = RiskPlan(
        proposed_entry=100,
        proposed_stop=98,
        proposed_target=103,
        max_risk_points=3,
        min_reward_risk_ratio=2,
    )
    result = RiskEngine().evaluate(plan)
    assert result.status is DecisionStatus.INVALID_RISK_REWARD


def test_entry_too_far_from_invalidation_is_rejected() -> None:
    plan = RiskPlan(
        proposed_entry=100,
        proposed_stop=95,
        proposed_target=112,
        max_risk_points=3,
        min_reward_risk_ratio=2,
    )
    result = RiskEngine().evaluate(plan)
    assert result.status is DecisionStatus.ENTRY_TOO_LATE

