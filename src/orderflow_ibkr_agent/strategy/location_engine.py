from dataclasses import dataclass

from orderflow_ibkr_agent.models import MarketSnapshot, OrderFlowSnapshot


@dataclass(frozen=True)
class LocationResult:
    valid: bool
    reason: str
    location: str | None = None


class LocationEngine:
    def evaluate(
        self, snapshot: MarketSnapshot, order_flow: OrderFlowSnapshot
    ) -> LocationResult:
        price = snapshot.last_price
        tolerance = max(0.25, price * 0.0005)
        candidates = {
            "previous VAL": snapshot.previous_val,
            "developing VAL": snapshot.current_val,
            "lower VWAP deviation": snapshot.vwap_lower_1,
            "composite value low": snapshot.composite_value_low,
        }
        for name, level in candidates.items():
            if abs(price - level) <= tolerance:
                return LocationResult(True, f"Price is at a valid long location: {name}", name)
        if order_flow.price_reclaimed_level and abs(price - snapshot.vwap) <= tolerance:
            return LocationResult(True, "Price reclaimed VWAP", "VWAP reclaim")
        return LocationResult(False, "Price is not at a defined long location")

