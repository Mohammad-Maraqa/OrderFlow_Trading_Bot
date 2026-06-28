import pytest

from orderflow_ibkr_agent.ibkr.contracts import etf_contract, futures_contract, stock_contract
from orderflow_ibkr_agent.models import DecisionStatus, TradeDecision, TradeSetupType
from orderflow_ibkr_agent.paper.simulator import PaperExecutor


def test_contract_factories_are_sdk_independent() -> None:
    future = futures_contract("ES", expiry="202609")
    stock = stock_contract("AAPL")
    etf = etf_contract("SPY")
    assert (future.security_type, future.exchange, future.expiry) == ("FUT", "CME", "202609")
    assert (stock.security_type, stock.exchange) == ("STK", "SMART")
    assert (etf.security_type, etf.symbol) == ("STK", "SPY")


def test_paper_executor_only_accepts_valid_long() -> None:
    decision = TradeDecision(
        symbol="ES",
        timestamp="2026-06-24T14:30:00Z",
        market_state="bullish",
        setup_type=TradeSetupType.PULLBACK_CONTINUATION_LONG,
        decision=DecisionStatus.LONG_VALID,
        confidence_score=0.8,
        reasons=["CLC valid"],
        warnings=[],
        entry_price=6000,
        stop_price=5998,
        target_price=6004,
        risk_points=2,
        reward_points=4,
        risk_reward_ratio=2,
        invalidation_level=5998,
    )
    order = PaperExecutor().create_order(decision)
    assert order.action == "BUY"
    assert order.order_type == "LIMIT"


def test_paper_executor_rejects_non_valid_decision(failed_breakdown_snapshot) -> None:
    decision = TradeDecision(
        symbol="ES",
        timestamp=failed_breakdown_snapshot.timestamp,
        market_state=failed_breakdown_snapshot.market_state,
        setup_type=None,
        decision=DecisionStatus.NO_TRADE,
        confidence_score=0,
        reasons=["No setup"],
        warnings=[],
    )
    with pytest.raises(ValueError, match="LONG_VALID"):
        PaperExecutor().create_order(decision)
