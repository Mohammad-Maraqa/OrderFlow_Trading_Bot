from datetime import datetime
from hashlib import sha256
from pathlib import Path
from uuid import uuid4

from orderflow_ibkr_agent.data.input_models import MarketDecisionInput
from orderflow_ibkr_agent.models import DecisionStatus, TradeDecision, TradeSetupType
from orderflow_ibkr_agent.paper.models import (
    PaperCloseReason,
    PaperEventType,
    PaperOrder,
    PaperOrderSide,
    PaperOrderStatus,
    PaperOrderType,
    PaperPosition,
    PaperPositionStatus,
    PaperTradeEvent,
    PaperTradeRecord,
)
from orderflow_ibkr_agent.paper.persistence import (
    PaperState,
    PaperStateStore,
    PaperTradeJournal,
)


class PaperTradeRejectedError(ValueError):
    pass


class PaperTradeLifecycleManager:
    """Manage fake long-only fills and positions without any broker dependency."""

    def __init__(
        self,
        state_path: str | Path = Path("data/paper_positions.json"),
        journal_path: str | Path = Path("logs/paper_trades.jsonl"),
        allow_duplicate_symbols: bool = False,
    ) -> None:
        self.state_path = Path(state_path)
        self.journal_path = Path(journal_path)
        self.allow_duplicate_symbols = allow_duplicate_symbols
        self._store = PaperStateStore(self.state_path)
        self._journal = PaperTradeJournal(self.journal_path)
        self._state = self._store.load()

    def list_open_positions(self) -> list[PaperPosition]:
        return [
            position
            for position in self._state.positions
            if position.status is PaperPositionStatus.OPEN
        ]

    def list_trade_records(self) -> list[PaperTradeRecord]:
        return list(self._state.trade_records)

    def read_journal_events(self) -> list[dict]:
        return self._journal.read()

    def open_paper_trade_from_decision(
        self,
        decision: TradeDecision,
        market_input: MarketDecisionInput,
        allow_duplicate: bool | None = None,
    ) -> PaperTradeRecord:
        self._validate_open_request(decision, market_input)
        duplicates_allowed = (
            self.allow_duplicate_symbols if allow_duplicate is None else allow_duplicate
        )
        if not duplicates_allowed and any(
            position.symbol == decision.symbol for position in self.list_open_positions()
        ):
            raise PaperTradeRejectedError(
                f"An open paper position for {decision.symbol} is already open"
            )

        quantity = self._quantity_from_metadata(market_input)
        entry = float(decision.entry_price)
        stop = float(decision.stop_price)
        target = float(decision.target_price)
        risk = entry - stop
        reward = target - entry
        opened_at = market_input.instrument.timestamp
        trade_id = f"trade-{uuid4()}"
        decision_id = "decision-" + sha256(
            decision.model_dump_json().encode("utf-8")
        ).hexdigest()[:24]

        order = PaperOrder(
            order_id=f"order-{uuid4()}",
            symbol=decision.symbol,
            side=PaperOrderSide.BUY,
            order_type=PaperOrderType.LIMIT,
            price=entry,
            quantity=quantity,
            status=PaperOrderStatus.FILLED,
            created_at=opened_at,
            filled_at=opened_at,
        )
        position = PaperPosition(
            position_id=trade_id,
            symbol=decision.symbol,
            quantity=quantity,
            entry_price=entry,
            stop_price=stop,
            target_price=target,
            point_value=market_input.instrument.point_value,
            status=PaperPositionStatus.OPEN,
            opened_at=opened_at,
        )
        record = PaperTradeRecord(
            trade_id=trade_id,
            decision_id=decision_id,
            setup_type=decision.setup_type,
            symbol=decision.symbol,
            entry_price=entry,
            stop_price=stop,
            target_price=target,
            quantity=quantity,
            point_value=market_input.instrument.point_value,
            risk_per_unit=risk,
            reward_per_unit=reward,
            planned_r_multiple=reward / risk,
            status=PaperPositionStatus.OPEN,
            decision_reason="; ".join(decision.reasons),
            created_at=opened_at,
        )
        self._state.positions.append(position)
        self._state.trade_records.append(record)
        self._store.save(self._state)
        self._journal.append(
            PaperTradeEvent(
                event_id=f"event-{uuid4()}",
                event_type=PaperEventType.OPENED,
                trade_id=trade_id,
                symbol=decision.symbol,
                timestamp=opened_at,
                position_status=PaperPositionStatus.OPEN,
                last_price=entry,
                order=order,
            )
        )
        return record

    def update_open_position_with_price(
        self, symbol: str, last_price: float, timestamp: datetime
    ) -> list[PaperPosition]:
        if last_price <= 0:
            raise PaperTradeRejectedError("last_price must be positive")
        if timestamp.tzinfo is None or timestamp.utcoffset() is None:
            raise PaperTradeRejectedError("timestamp must include a timezone")
        matching = [
            (index, position)
            for index, position in enumerate(self._state.positions)
            if position.symbol == symbol and position.status is PaperPositionStatus.OPEN
        ]
        if not matching:
            raise PaperTradeRejectedError(f"No open paper position for {symbol}")

        updated_positions: list[PaperPosition] = []
        pending_events: list[PaperTradeEvent] = []
        for index, position in matching:
            pending_events.append(
                PaperTradeEvent(
                    event_id=f"event-{uuid4()}",
                    event_type=PaperEventType.UPDATED,
                    trade_id=position.position_id,
                    symbol=symbol,
                    timestamp=timestamp,
                    position_status=PaperPositionStatus.OPEN,
                    last_price=last_price,
                )
            )
            close_reason: PaperCloseReason | None = None
            order_type: PaperOrderType | None = None
            if last_price <= position.stop_price:
                close_reason = PaperCloseReason.STOP_HIT
                order_type = PaperOrderType.STOP
            elif last_price >= position.target_price:
                close_reason = PaperCloseReason.TARGET_HIT
                order_type = PaperOrderType.TARGET

            if close_reason is None:
                updated_positions.append(position)
                continue

            pnl = (last_price - position.entry_price) * position.quantity * position.point_value
            realized_r = (last_price - position.entry_price) / (
                position.entry_price - position.stop_price
            )
            closed_position = PaperPosition.model_validate(
                {
                    **position.model_dump(),
                    "status": PaperPositionStatus.CLOSED,
                    "closed_at": timestamp,
                    "close_price": last_price,
                    "close_reason": close_reason,
                    "realized_pnl": pnl,
                    "r_multiple": realized_r,
                }
            )
            self._state.positions[index] = closed_position
            record_index = self._record_index(position.position_id)
            record = self._state.trade_records[record_index]
            self._state.trade_records[record_index] = PaperTradeRecord.model_validate(
                {
                    **record.model_dump(),
                    "status": PaperPositionStatus.CLOSED,
                    "close_price": last_price,
                    "close_reason": close_reason,
                    "realized_pnl": pnl,
                    "realized_r_multiple": realized_r,
                    "closed_at": timestamp,
                }
            )
            exit_order = PaperOrder(
                order_id=f"order-{uuid4()}",
                symbol=symbol,
                side=PaperOrderSide.SELL_TO_CLOSE,
                order_type=order_type,
                price=last_price,
                quantity=position.quantity,
                status=PaperOrderStatus.FILLED,
                created_at=timestamp,
                filled_at=timestamp,
            )
            pending_events.append(
                PaperTradeEvent(
                    event_id=f"event-{uuid4()}",
                    event_type=PaperEventType.CLOSED,
                    trade_id=position.position_id,
                    symbol=symbol,
                    timestamp=timestamp,
                    position_status=PaperPositionStatus.CLOSED,
                    last_price=last_price,
                    close_reason=close_reason,
                    realized_pnl=pnl,
                    realized_r_multiple=realized_r,
                    order=exit_order,
                )
            )
            updated_positions.append(closed_position)

        self._store.save(self._state)
        for event in pending_events:
            self._journal.append(event)
        return updated_positions

    def _record_index(self, trade_id: str) -> int:
        for index, record in enumerate(self._state.trade_records):
            if record.trade_id == trade_id:
                return index
        raise PaperTradeRejectedError(f"Missing trade record for {trade_id}")

    @staticmethod
    def _quantity_from_metadata(market_input: MarketDecisionInput) -> int:
        value = (market_input.metadata or {}).get("quantity", 1)
        if isinstance(value, bool) or not isinstance(value, int) or value <= 0:
            raise PaperTradeRejectedError("metadata.quantity must be a positive integer")
        return value

    @staticmethod
    def _validate_open_request(
        decision: TradeDecision, market_input: MarketDecisionInput
    ) -> None:
        if decision.decision is not DecisionStatus.LONG_VALID:
            raise PaperTradeRejectedError("Paper trade requires a LONG_VALID decision")
        if not isinstance(decision.setup_type, TradeSetupType):
            raise PaperTradeRejectedError("Paper trade setup must pass the long-only guard")
        if decision.setup_type is not market_input.setup_candidate.setup_type:
            raise PaperTradeRejectedError("Decision setup does not match the input candidate")
        if decision.symbol != market_input.instrument.symbol:
            raise PaperTradeRejectedError("Decision symbol does not match the input instrument")
        prices = (decision.entry_price, decision.stop_price, decision.target_price)
        if any(price is None for price in prices):
            raise PaperTradeRejectedError("LONG_VALID decision is missing prices")
        entry, stop, target = prices
        if not stop < entry < target:
            raise PaperTradeRejectedError(
                "Long paper trade requires stop < entry < target"
            )
