from orderflow_ibkr_agent.models import TradeSetupType
from orderflow_ibkr_agent.strategy.setup_classifier import SetupClassifier


def test_stop_run_reclaim_with_absorption_is_failed_breakdown(
    failed_breakdown_snapshot, confirming_order_flow
) -> None:
    result = SetupClassifier().classify(failed_breakdown_snapshot, confirming_order_flow)
    assert result.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG


def test_no_confirmation_does_not_classify_setup(
    failed_breakdown_snapshot, confirming_order_flow
) -> None:
    flow = confirming_order_flow.model_copy(
        update={
            "seller_absorption_detected": False,
            "passive_buy_liquidity": 0,
            "price_reclaimed_level": False,
            "stop_run_detected": False,
        }
    )
    result = SetupClassifier().classify(failed_breakdown_snapshot, flow)
    assert result.setup_type is None
