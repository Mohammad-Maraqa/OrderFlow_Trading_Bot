from orderflow_ibkr_agent.ibkr.config import IBKRConfig
from orderflow_ibkr_agent.ibkr.errors import IBKRSafetyError


PAPER_PORTS = frozenset({7497, 4002})
KNOWN_LIVE_PORTS = frozenset({7496, 4001})
FORBIDDEN_ORDER_METHODS = frozenset(
    {
        "place_order",
        "submit_order",
        "cancel_order",
        "modify_order",
        "bracket_order",
        "transmit_order",
        "submit_paper_order",
    }
)


def validate_ibkr_safety(
    config: IBKRConfig, adapter_type: type | None = None
) -> None:
    if not config.enabled:
        raise IBKRSafetyError("IBKR must be explicitly enabled before connection")
    if not config.readonly:
        raise IBKRSafetyError("IBKR readonly mode must remain true")
    if not config.require_paper_account:
        raise IBKRSafetyError("IBKR paper account enforcement must remain enabled")
    if not config.allow_unsafe_live_port:
        if config.port in KNOWN_LIVE_PORTS:
            raise IBKRSafetyError(
                f"Port {config.port} is an unsafe live IBKR port for Phase 3A"
            )
        if config.port not in PAPER_PORTS:
            raise IBKRSafetyError(
                f"Port {config.port} is not a recognized paper-safe IBKR port"
            )
    if adapter_type is not None:
        exposed = [name for name in FORBIDDEN_ORDER_METHODS if hasattr(adapter_type, name)]
        if exposed:
            raise IBKRSafetyError(
                "Read-only adapter exposes forbidden method(s): " + ", ".join(sorted(exposed))
            )
