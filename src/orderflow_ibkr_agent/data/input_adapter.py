from typing import Any

from orderflow_ibkr_agent.data.input_models import (
    ContextBias,
    InputSession,
    MarketDecisionInput,
    OrderFlowConfidence,
    TapeSpeed,
    ValueMarketState,
)
from orderflow_ibkr_agent.decision_engine import DecisionEngine
from orderflow_ibkr_agent.models import (
    DecisionStatus,
    MarketSnapshot,
    MarketState,
    OrderFlowSnapshot,
    RiskPlan,
    SessionType,
    TradeDecision,
    TradeSetupType,
)


def _internal_market_state(data: MarketDecisionInput) -> MarketState:
    state = data.value_context.market_state
    direct = {
        ValueMarketState.BALANCE: MarketState.BALANCED,
        ValueMarketState.BULLISH_IMBALANCE: MarketState.IMBALANCED_UP,
        ValueMarketState.BEARISH_IMBALANCE: MarketState.IMBALANCED_DOWN,
        ValueMarketState.ACCEPTANCE_HIGHER: MarketState.BULLISH,
        ValueMarketState.ACCEPTANCE_LOWER: MarketState.BEARISH,
        ValueMarketState.REJECTION_HIGHER: MarketState.BEARISH,
        ValueMarketState.NO_TRADE: MarketState.UNCERTAIN,
    }
    if state in direct:
        return direct[state]
    bias = data.value_context.context_bias
    return {
        ContextBias.BULLISH: MarketState.BULLISH,
        ContextBias.BEARISH: MarketState.BEARISH,
        ContextBias.BALANCED: MarketState.BALANCED,
        ContextBias.UNCERTAIN: MarketState.UNCERTAIN,
    }[bias]


def _metadata_number(metadata: dict[str, Any] | None, key: str, default: float) -> float:
    if metadata is None or key not in metadata:
        return default
    value = metadata[key]
    if isinstance(value, bool) or not isinstance(value, (int, float)):
        raise ValueError(f"metadata.{key} must be a number")
    return float(value)


def to_engine_inputs(
    data: MarketDecisionInput,
) -> tuple[MarketSnapshot, OrderFlowSnapshot, RiskPlan]:
    instrument = data.instrument
    value = data.value_context
    flow = data.order_flow
    candidate = data.setup_candidate
    confirmation = candidate.confirmation_type.upper()

    reclaim = (
        "RECLAIM" in confirmation
        or candidate.setup_type
        in {TradeSetupType.FAILED_BREAKDOWN_LONG, TradeSetupType.VALUE_RECLAIM_LONG}
    )
    buyer_defense = (
        flow.buyer_absorption_detected
        or flow.passive_buy_liquidity_detected
        or flow.seller_absorption_detected
    )
    confirmation_present = (
        flow.orderflow_confidence is not OrderFlowConfidence.LOW
        and (buyer_defense or reclaim or flow.trapped_sellers_detected)
    )

    market = MarketSnapshot(
        symbol=instrument.symbol,
        timestamp=instrument.timestamp,
        last_price=candidate.entry_price,
        session_type=SessionType.RTH if instrument.session is InputSession.RTH else SessionType.ETH,
        previous_vah=value.previous_rth_vah,
        previous_val=value.previous_rth_val,
        previous_poc=value.previous_rth_poc,
        current_vah=value.current_rth_vah,
        current_val=value.current_rth_val,
        current_poc=value.current_rth_poc,
        vwap=value.current_vwap,
        vwap_upper_1=value.vwap_upper_1,
        vwap_upper_2=value.vwap_upper_2,
        vwap_lower_1=value.vwap_lower_1,
        vwap_lower_2=value.vwap_lower_2,
        composite_value_high=value.composite_value_high,
        composite_value_low=value.composite_value_low,
        market_state=_internal_market_state(data),
    )
    order_flow = OrderFlowSnapshot(
        aggressive_buy_volume=flow.aggressive_buy_volume,
        aggressive_sell_volume=flow.aggressive_sell_volume,
        delta=flow.delta,
        cumulative_delta=flow.cumulative_delta,
        seller_absorption_detected=flow.seller_absorption_detected,
        buyer_absorption_detected=flow.buyer_absorption_detected,
        passive_buy_liquidity=1.0 if flow.passive_buy_liquidity_detected else 0.0,
        passive_sell_liquidity=1.0 if flow.passive_sell_liquidity_detected else 0.0,
        liquidity_wall_nearby=flow.liquidity_wall_price is not None,
        iceberg_detected=False,
        stop_run_detected=(
            flow.trapped_sellers_detected
            or candidate.setup_type is TradeSetupType.FAILED_BREAKDOWN_LONG
        ),
        tape_speed={TapeSpeed.SLOW: 0.5, TapeSpeed.NORMAL: 1.0, TapeSpeed.FAST: 2.0}[
            flow.tape_speed
        ],
        volume_outlier_score=2.0 if flow.volume_spike_detected else 1.0,
        price_reclaimed_level=reclaim and confirmation_present,
        price_accepted_above_level=confirmation_present and (buyer_defense or reclaim),
        price_accepted_below_level=flow.trapped_buyers_detected,
    )
    risk_points = candidate.entry_price - candidate.stop_price
    risk_plan = RiskPlan(
        proposed_entry=candidate.entry_price,
        proposed_stop=candidate.stop_price,
        proposed_target=candidate.target_price,
        max_risk_points=_metadata_number(data.metadata, "max_risk_points", risk_points),
        min_reward_risk_ratio=_metadata_number(
            data.metadata, "min_reward_risk_ratio", 2.0
        ),
        position_size=None,
    )
    return market, order_flow, risk_plan


def evaluate_market_input(
    data: MarketDecisionInput, engine: DecisionEngine | None = None
) -> TradeDecision:
    market, order_flow, risk_plan = to_engine_inputs(data)
    decision = (engine or DecisionEngine()).evaluate(market, order_flow, risk_plan)
    declared = data.setup_candidate.setup_type
    if decision.decision is DecisionStatus.LONG_VALID and decision.setup_type is not declared:
        payload = decision.model_dump()
        payload.update(
            {
                "decision": DecisionStatus.NO_TRADE,
                "confidence_score": 0.0,
                "reasons": [
                    *decision.reasons,
                    f"Declared setup {declared.value} does not match engine classification "
                    f"{decision.setup_type.value}",
                ],
                "entry_price": None,
                "stop_price": None,
                "target_price": None,
                "risk_points": None,
                "reward_points": None,
                "risk_reward_ratio": None,
                "invalidation_level": None,
            }
        )
        return TradeDecision.model_validate(payload)
    return decision

