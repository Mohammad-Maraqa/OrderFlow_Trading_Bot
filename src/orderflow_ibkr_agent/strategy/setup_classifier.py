from dataclasses import dataclass

from orderflow_ibkr_agent.models import (
    MarketSnapshot,
    MarketState,
    OrderFlowSnapshot,
    TradeSetupType,
)


@dataclass(frozen=True)
class SetupResult:
    setup_type: TradeSetupType | None
    reason: str


class SetupClassifier:
    def classify(
        self, snapshot: MarketSnapshot, order_flow: OrderFlowSnapshot
    ) -> SetupResult:
        absorbed = (
            order_flow.seller_absorption_detected
            or order_flow.buyer_absorption_detected
            or order_flow.passive_buy_liquidity > order_flow.passive_sell_liquidity
        )
        if order_flow.stop_run_detected and order_flow.price_reclaimed_level and absorbed:
            return SetupResult(
                TradeSetupType.FAILED_BREAKDOWN_LONG,
                "Stop run below support failed and reclaimed with buyer response",
            )
        tolerance = max(0.25, snapshot.last_price * 0.0005)
        if abs(snapshot.last_price - snapshot.vwap_lower_1) <= tolerance and absorbed:
            return SetupResult(
                TradeSetupType.DEVIATION_REJECTION_LONG,
                "Lower VWAP deviation rejected with absorption",
            )
        if order_flow.price_reclaimed_level and absorbed:
            return SetupResult(
                TradeSetupType.VALUE_RECLAIM_LONG,
                "Value/VWAP level reclaimed with buyer response",
            )
        if (
            snapshot.last_price >= snapshot.previous_vah
            and order_flow.price_accepted_above_level
            and absorbed
        ):
            return SetupResult(
                TradeSetupType.BREAKOUT_PULLBACK_LONG,
                "Breakout retest held above prior value",
            )
        if snapshot.market_state in {MarketState.BULLISH, MarketState.IMBALANCED_UP} and absorbed:
            return SetupResult(
                TradeSetupType.PULLBACK_CONTINUATION_LONG,
                "Bullish pullback met buyer defense",
            )
        return SetupResult(None, "No allowed long setup is fully formed")

