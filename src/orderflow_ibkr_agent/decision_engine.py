from orderflow_ibkr_agent.models import (
    DataQuality,
    DecisionStatus,
    MarketSnapshot,
    OrderFlowSnapshot,
    RiskPlan,
    TradeDecision,
)
from orderflow_ibkr_agent.strategy.context_engine import ContextEngine
from orderflow_ibkr_agent.strategy.location_engine import LocationEngine
from orderflow_ibkr_agent.strategy.orderflow_engine import OrderFlowEngine
from orderflow_ibkr_agent.strategy.risk_engine import RiskEngine
from orderflow_ibkr_agent.strategy.setup_classifier import SetupClassifier


class DecisionEngine:
    """Coordinate Context + Location + Confirmation using fixed precedence."""

    def __init__(self) -> None:
        self.context_engine = ContextEngine()
        self.location_engine = LocationEngine()
        self.orderflow_engine = OrderFlowEngine()
        self.setup_classifier = SetupClassifier()
        self.risk_engine = RiskEngine()

    def evaluate(
        self,
        snapshot: MarketSnapshot,
        order_flow: OrderFlowSnapshot | None,
        risk_plan: RiskPlan | None,
    ) -> TradeDecision:
        quality = snapshot.data_quality.model_copy(deep=True)
        warnings = list(quality.notes)
        if quality.stale_data:
            warnings.append("Market snapshot is stale")
        if order_flow is None:
            quality.order_flow_complete = False
            return self._blocked(
                snapshot, DecisionStatus.DATA_MISSING, ["Required order-flow data is missing"], warnings, quality
            )
        if risk_plan is None:
            quality.risk_plan_complete = False
            return self._blocked(
                snapshot, DecisionStatus.DATA_MISSING, ["Required risk plan is missing"], warnings, quality
            )
        if not quality.is_usable:
            return self._blocked(
                snapshot, DecisionStatus.DATA_MISSING, ["Input data-quality checks failed"], warnings, quality
            )

        context = self.context_engine.evaluate(snapshot, order_flow)
        if not context.valid:
            return self._blocked(snapshot, DecisionStatus.INVALID_CONTEXT, [context.reason], warnings, quality)
        location = self.location_engine.evaluate(snapshot, order_flow)
        if not location.valid:
            return self._blocked(snapshot, DecisionStatus.INVALID_LOCATION, [context.reason, location.reason], warnings, quality)
        confirmation = self.orderflow_engine.evaluate(order_flow)
        if not confirmation.confirmed:
            return self._blocked(
                snapshot,
                DecisionStatus.WAITING_FOR_CONFIRMATION,
                [context.reason, location.reason, confirmation.reason],
                warnings,
                quality,
            )
        setup = self.setup_classifier.classify(snapshot, order_flow)
        if setup.setup_type is None:
            return self._blocked(
                snapshot,
                DecisionStatus.NO_TRADE,
                [context.reason, location.reason, confirmation.reason, setup.reason],
                warnings,
                quality,
            )
        risk = self.risk_engine.evaluate(risk_plan)
        reasons = [context.reason, location.reason, confirmation.reason, setup.reason, risk.reason]
        if risk.status is not None:
            return TradeDecision(
                symbol=snapshot.symbol,
                timestamp=snapshot.timestamp,
                market_state=snapshot.market_state,
                setup_type=setup.setup_type,
                decision=risk.status,
                confidence_score=0.0,
                reasons=reasons,
                warnings=warnings,
                data_quality=quality,
            )
        return TradeDecision(
            symbol=snapshot.symbol,
            timestamp=snapshot.timestamp,
            market_state=snapshot.market_state,
            setup_type=setup.setup_type,
            decision=DecisionStatus.LONG_VALID,
            confidence_score=0.85,
            reasons=reasons,
            warnings=warnings,
            entry_price=risk_plan.proposed_entry,
            stop_price=risk_plan.proposed_stop,
            target_price=risk_plan.proposed_target,
            risk_points=risk.risk_points,
            reward_points=risk.reward_points,
            risk_reward_ratio=risk.risk_reward_ratio,
            invalidation_level=risk_plan.proposed_stop,
            data_quality=quality,
        )

    @staticmethod
    def _blocked(
        snapshot: MarketSnapshot,
        status: DecisionStatus,
        reasons: list[str],
        warnings: list[str],
        quality: DataQuality,
    ) -> TradeDecision:
        return TradeDecision(
            symbol=snapshot.symbol,
            timestamp=snapshot.timestamp,
            market_state=snapshot.market_state,
            setup_type=None,
            decision=status,
            confidence_score=0.0,
            reasons=reasons,
            warnings=warnings,
            data_quality=quality,
        )

