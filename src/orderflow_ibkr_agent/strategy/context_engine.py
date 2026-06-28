from dataclasses import dataclass

from orderflow_ibkr_agent.models import MarketSnapshot, MarketState, OrderFlowSnapshot


@dataclass(frozen=True)
class ContextResult:
    valid: bool
    reason: str


class ContextEngine:
    def evaluate(
        self, snapshot: MarketSnapshot, order_flow: OrderFlowSnapshot
    ) -> ContextResult:
        if snapshot.market_state in {MarketState.BULLISH, MarketState.IMBALANCED_UP}:
            return ContextResult(True, "Bullish higher-timeframe context supports longs")
        if snapshot.market_state is MarketState.BALANCED:
            return ContextResult(True, "Balanced context permits longs at a discount extreme")
        if (
            snapshot.market_state in {MarketState.BEARISH, MarketState.IMBALANCED_DOWN}
            and order_flow.stop_run_detected
            and order_flow.price_reclaimed_level
        ):
            return ContextResult(True, "Bearish context has a failed-breakdown reclaim exception")
        return ContextResult(False, "Market context does not permit a long setup")

