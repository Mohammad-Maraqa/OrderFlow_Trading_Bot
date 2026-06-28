from dataclasses import dataclass

from orderflow_ibkr_agent.models import OrderFlowSnapshot


@dataclass(frozen=True)
class OrderFlowResult:
    confirmed: bool
    reason: str


class OrderFlowEngine:
    def evaluate(self, order_flow: OrderFlowSnapshot) -> OrderFlowResult:
        sellers_rejected = (
            order_flow.seller_absorption_detected
            and order_flow.aggressive_sell_volume > 0
            and not order_flow.price_accepted_below_level
        )
        buyer_defense = (
            order_flow.buyer_absorption_detected
            or order_flow.passive_buy_liquidity > order_flow.passive_sell_liquidity
        )
        level_held = order_flow.price_accepted_above_level
        if level_held and (sellers_rejected or buyer_defense):
            return OrderFlowResult(True, "Failed selling was absorbed and the candidate level held")
        if order_flow.price_accepted_below_level:
            return OrderFlowResult(False, "Price is accepting below the candidate support")
        return OrderFlowResult(False, "Waiting for seller failure and buyer defense/reclaim")
