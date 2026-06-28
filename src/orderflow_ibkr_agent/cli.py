import json
from datetime import datetime, timezone
from pathlib import Path

import typer
from rich.console import Console

from orderflow_ibkr_agent.data.input_adapter import evaluate_market_input
from orderflow_ibkr_agent.data.json_loader import MarketInputError, load_market_decision_input
from orderflow_ibkr_agent.data.sample_snapshots import failed_breakdown_sample
from orderflow_ibkr_agent.decision_engine import DecisionEngine
from orderflow_ibkr_agent.journal.trade_journal import TradeJournal
from orderflow_ibkr_agent.ibkr.adapter import (
    IBKRReadOnlyAdapter,
    mask_account_identifier,
)
from orderflow_ibkr_agent.ibkr.config import IBKRConfig
from orderflow_ibkr_agent.ibkr.contracts import ContractSpec
from orderflow_ibkr_agent.ibkr.errors import IBKRError
from orderflow_ibkr_agent.models import TradeSetupType
from orderflow_ibkr_agent.paper.lifecycle import (
    PaperTradeLifecycleManager,
    PaperTradeRejectedError,
)
from orderflow_ibkr_agent.paper.models import PaperPosition
from orderflow_ibkr_agent.paper.persistence import PaperPersistenceError
from orderflow_ibkr_agent.paper.simulator import PaperExecutor


app = typer.Typer(help="Long-only order-flow decision engine")
console = Console()


def _sample_decision():
    sample = failed_breakdown_sample()
    return DecisionEngine().evaluate(sample.market, sample.order_flow, sample.risk_plan)


@app.command("evaluate-sample")
def evaluate_sample() -> None:
    """Evaluate a deterministic failed-breakdown sample without a broker."""
    console.print_json(_sample_decision().model_dump_json())


@app.command("evaluate-json")
def evaluate_json(
    input_path: Path = typer.Argument(..., help="Path to a MarketDecisionInput JSON file"),
    journal_path: Path = typer.Option(
        Path("journal/decisions.jsonl"),
        "--journal-path",
        help="JSONL audit journal destination",
    ),
) -> None:
    """Validate and evaluate one normalized market snapshot without a broker."""
    try:
        market_input = load_market_decision_input(input_path)
        decision = evaluate_market_input(market_input)
    except (MarketInputError, ValueError) as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error

    TradeJournal(journal_path).append(decision)
    typer.echo(f"Symbol: {decision.symbol}")
    typer.echo(f"Candidate setup: {market_input.setup_candidate.setup_type.value}")
    typer.echo(f"Decision: {decision.decision.value}")
    typer.echo(f"Engine setup: {decision.setup_type.value if decision.setup_type else 'NONE'}")
    typer.echo(f"Confidence: {decision.confidence_score:.2f}")
    if decision.entry_price is not None:
        typer.echo(
            f"Entry / Stop / Target: {decision.entry_price} / "
            f"{decision.stop_price} / {decision.target_price}"
        )
        typer.echo(f"Reward:risk: {decision.risk_reward_ratio:.2f}")
    typer.echo("Reasons:")
    for reason in decision.reasons:
        typer.echo(f"- {reason}")
    if decision.warnings:
        typer.echo("Warnings:")
        for warning in decision.warnings:
            typer.echo(f"- {warning}")
    typer.echo(f"Journal: {journal_path}")


@app.command("validate-long-only")
def validate_long_only() -> None:
    """Prove forbidden short setup values cannot enter the model."""
    try:
        TradeSetupType("SHORT")
    except ValueError:
        console.print("PASS: long-only model guard is active")
        return
    raise typer.Exit(code=1)


@app.command("paper-sim-sample")
def paper_sim_sample() -> None:
    """Create an offline simulated order; never connects to IBKR."""
    order = PaperExecutor().create_order(_sample_decision())
    console.print(json.dumps(order.model_dump(), indent=2))


def _paper_manager(
    positions_path: Path, paper_journal_path: Path
) -> PaperTradeLifecycleManager:
    return PaperTradeLifecycleManager(
        state_path=positions_path,
        journal_path=paper_journal_path,
    )


def _print_paper_position(position: PaperPosition) -> None:
    typer.echo(
        f"{position.symbol} {position.status.value} qty={position.quantity} "
        f"entry={position.entry_price} stop={position.stop_price} "
        f"target={position.target_price}"
    )
    if position.close_reason is not None:
        typer.echo(
            f"Close: {position.close_reason.value} at {position.close_price} | "
            f"PnL={position.realized_pnl:.2f} | {position.r_multiple:.2f}R"
        )


@app.command("paper-open-from-json")
def paper_open_from_json(
    input_path: Path = typer.Argument(..., help="Validated market input JSON"),
    positions_path: Path = typer.Option(
        Path("data/paper_positions.json"), "--positions-path"
    ),
    paper_journal_path: Path = typer.Option(
        Path("logs/paper_trades.jsonl"), "--paper-journal-path"
    ),
) -> None:
    """Open an internal fake-fill trade from a LONG_VALID decision."""
    try:
        market_input = load_market_decision_input(input_path)
        decision = evaluate_market_input(market_input)
    except (MarketInputError, ValueError) as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error

    if decision.decision.value != "LONG_VALID":
        typer.echo(f"Paper trade not opened. Decision: {decision.decision.value}")
        for reason in decision.reasons:
            typer.echo(f"- {reason}")
        return

    try:
        record = _paper_manager(
            positions_path, paper_journal_path
        ).open_paper_trade_from_decision(decision, market_input)
    except (PaperTradeRejectedError, PaperPersistenceError) as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error

    typer.echo(f"Paper trade opened: {record.trade_id}")
    typer.echo(f"{record.symbol} {record.setup_type.value} qty={record.quantity}")
    typer.echo(
        f"Entry / Stop / Target: {record.entry_price} / "
        f"{record.stop_price} / {record.target_price}"
    )
    typer.echo(f"Planned R: {record.planned_r_multiple:.2f}R")
    typer.echo(f"State: {positions_path}")
    typer.echo(f"Journal: {paper_journal_path}")


@app.command("paper-update-price")
def paper_update_price(
    symbol: str = typer.Option(..., "--symbol", help="Position symbol"),
    price: float = typer.Option(..., "--price", min=0.0000001),
    positions_path: Path = typer.Option(
        Path("data/paper_positions.json"), "--positions-path"
    ),
    paper_journal_path: Path = typer.Option(
        Path("logs/paper_trades.jsonl"), "--paper-journal-path"
    ),
) -> None:
    """Apply one fake market price and close any triggered paper position."""
    try:
        positions = _paper_manager(
            positions_path, paper_journal_path
        ).update_open_position_with_price(
            symbol=symbol,
            last_price=price,
            timestamp=datetime.now(timezone.utc),
        )
    except (PaperTradeRejectedError, PaperPersistenceError) as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error
    for position in positions:
        _print_paper_position(position)


@app.command("paper-positions")
def paper_positions(
    positions_path: Path = typer.Option(
        Path("data/paper_positions.json"), "--positions-path"
    ),
    paper_journal_path: Path = typer.Option(
        Path("logs/paper_trades.jsonl"), "--paper-journal-path"
    ),
) -> None:
    """List persisted open internal paper positions."""
    try:
        positions = _paper_manager(positions_path, paper_journal_path).list_open_positions()
    except PaperPersistenceError as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error
    if not positions:
        typer.echo("No open paper positions")
        return
    for position in positions:
        _print_paper_position(position)


def _create_ibkr_adapter(config: IBKRConfig) -> IBKRReadOnlyAdapter:
    return IBKRReadOnlyAdapter(config)


def _ibkr_config_or_disabled() -> IBKRConfig | None:
    try:
        config = IBKRConfig()
    except ValueError as error:
        typer.echo(f"Error: invalid IBKR configuration: {error}", err=True)
        raise typer.Exit(code=2) from error
    if not config.enabled:
        typer.echo(
            "IBKR read-only access is disabled. Set ORDERFLOW_IBKR_ENABLED=true "
            "and keep the TWS/Gateway API Read-Only setting enabled."
        )
        return None
    return config


@app.command("ibkr-status")
def ibkr_status() -> None:
    """Show paper-account connection status through the read-only adapter."""
    config = _ibkr_config_or_disabled()
    if config is None:
        return
    adapter = _create_ibkr_adapter(config)
    try:
        adapter.connect()
        status = adapter.get_connection_status()
        summary = adapter.get_account_summary()
        typer.echo(f"Connected: {'yes' if status.connected else 'no'}")
        typer.echo(f"Safety guard: {'PASSED' if status.safety_guard_passed else 'FAILED'}")
        typer.echo(f"Endpoint: {status.host}:{status.port}")
        typer.echo(f"Read-only: {'yes' if status.readonly else 'no'}")
        typer.echo(f"Account: {mask_account_identifier(summary.account_id)}")
        for value in summary.values:
            currency = f" {value.currency}" if value.currency else ""
            typer.echo(f"{value.tag}: {value.value}{currency}")
    except IBKRError as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error
    finally:
        adapter.disconnect()


@app.command("ibkr-positions")
def ibkr_positions() -> None:
    """Read current IBKR paper-account positions without modifying them."""
    config = _ibkr_config_or_disabled()
    if config is None:
        return
    adapter = _create_ibkr_adapter(config)
    try:
        adapter.connect()
        positions = adapter.get_positions()
        if not positions:
            typer.echo("No IBKR positions")
            return
        for position in positions:
            typer.echo(
                f"{position.symbol} {position.security_type} qty={position.quantity} "
                f"avg_cost={position.average_cost} {position.currency} "
                f"account={mask_account_identifier(position.account_id)}"
            )
    except IBKRError as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error
    finally:
        adapter.disconnect()


@app.command("ibkr-snapshot")
def ibkr_snapshot(
    symbol: str = typer.Option(..., "--symbol"),
    security_type: str = typer.Option(..., "--security-type"),
    exchange: str = typer.Option(..., "--exchange"),
    currency: str = typer.Option("USD", "--currency"),
    expiry: str | None = typer.Option(None, "--expiry"),
) -> None:
    """Request one read-only market snapshot; futures expiry is mandatory."""
    normalized_type = security_type.upper()
    if normalized_type not in {"STK", "FUT"}:
        typer.echo("Error: --security-type must be STK or FUT", err=True)
        raise typer.Exit(code=2)
    if normalized_type == "FUT" and not expiry:
        typer.echo("Error: FUT snapshots require an explicit --expiry", err=True)
        raise typer.Exit(code=2)
    try:
        contract = ContractSpec(
            security_type=normalized_type,
            symbol=symbol,
            exchange=exchange,
            currency=currency,
            expiry=expiry,
        )
    except ValueError as error:
        typer.echo(f"Error: invalid contract: {error}", err=True)
        raise typer.Exit(code=2) from error

    config = _ibkr_config_or_disabled()
    if config is None:
        return
    adapter = _create_ibkr_adapter(config)
    try:
        adapter.connect()
        snapshot = adapter.request_market_snapshot(contract)
        typer.echo(f"{snapshot.symbol} {snapshot.security_type}")
        typer.echo(
            f"bid={snapshot.bid} ask={snapshot.ask} last={snapshot.last} close={snapshot.close}"
        )
        typer.echo(f"timestamp={snapshot.timestamp.isoformat()}")
    except IBKRError as error:
        typer.echo(f"Error: {error}", err=True)
        raise typer.Exit(code=2) from error
    finally:
        adapter.disconnect()


if __name__ == "__main__":
    app()
