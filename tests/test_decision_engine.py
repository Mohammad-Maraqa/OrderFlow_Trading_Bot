from orderflow_ibkr_agent.decision_engine import DecisionEngine
from orderflow_ibkr_agent.models import DecisionStatus, MarketState


def test_absorption_and_reclaim_at_value_can_produce_valid_long(
    failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
) -> None:
    decision = DecisionEngine().evaluate(
        failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
    )
    assert decision.decision is DecisionStatus.LONG_VALID
    assert decision.entry_price == valid_risk_plan.proposed_entry
    assert decision.stop_price == valid_risk_plan.proposed_stop
    assert decision.target_price == valid_risk_plan.proposed_target
    assert decision.risk_reward_ratio >= 2.0
    assert decision.reasons
    assert isinstance(decision.warnings, list)


def test_missing_confirmation_waits_instead_of_validating(
    failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
) -> None:
    snapshot = failed_breakdown_snapshot.model_copy(
        update={"market_state": MarketState.BULLISH}
    )
    flow = confirming_order_flow.model_copy(
        update={
            "seller_absorption_detected": False,
            "buyer_absorption_detected": False,
            "passive_buy_liquidity": 0,
            "price_reclaimed_level": False,
        }
    )
    decision = DecisionEngine().evaluate(snapshot, flow, valid_risk_plan)
    assert decision.decision is DecisionStatus.WAITING_FOR_CONFIRMATION
    assert decision.entry_price is None
    assert decision.reasons
    assert isinstance(decision.warnings, list)


def test_missing_order_flow_returns_data_missing(
    failed_breakdown_snapshot, valid_risk_plan
) -> None:
    decision = DecisionEngine().evaluate(failed_breakdown_snapshot, None, valid_risk_plan)
    assert decision.decision is DecisionStatus.DATA_MISSING
    assert "order-flow" in " ".join(decision.reasons).lower()


def test_bad_reward_to_risk_blocks_long(
    failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
) -> None:
    plan = valid_risk_plan.model_copy(update={"proposed_target": 6004.0})
    decision = DecisionEngine().evaluate(failed_breakdown_snapshot, confirming_order_flow, plan)
    assert decision.decision is DecisionStatus.INVALID_RISK_REWARD
