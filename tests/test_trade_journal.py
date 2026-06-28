from orderflow_ibkr_agent.decision_engine import DecisionEngine
from orderflow_ibkr_agent.journal.trade_journal import TradeJournal


def test_journal_appends_auditable_json_line(
    tmp_path, failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
) -> None:
    decision = DecisionEngine().evaluate(
        failed_breakdown_snapshot, confirming_order_flow, valid_risk_plan
    )
    path = tmp_path / "decisions.jsonl"
    TradeJournal(path).append(decision)
    content = path.read_text(encoding="utf-8")
    assert '"decision":"LONG_VALID"' in content
    assert content.endswith("\n")
