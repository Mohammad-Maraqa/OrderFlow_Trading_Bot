from dataclasses import dataclass

from orderflow_ibkr_agent.models import DecisionStatus, RiskPlan


@dataclass(frozen=True)
class RiskResult:
    status: DecisionStatus | None
    reason: str
    risk_points: float
    reward_points: float
    risk_reward_ratio: float


class RiskEngine:
    def evaluate(self, plan: RiskPlan) -> RiskResult:
        risk = plan.proposed_entry - plan.proposed_stop
        reward = plan.proposed_target - plan.proposed_entry
        ratio = reward / risk
        if risk > plan.max_risk_points:
            return RiskResult(
                DecisionStatus.ENTRY_TOO_LATE,
                f"Entry risk {risk:.2f} exceeds maximum {plan.max_risk_points:.2f}",
                risk,
                reward,
                ratio,
            )
        if ratio < plan.min_reward_risk_ratio:
            return RiskResult(
                DecisionStatus.INVALID_RISK_REWARD,
                f"Reward:risk {ratio:.2f} is below {plan.min_reward_risk_ratio:.2f}",
                risk,
                reward,
                ratio,
            )
        return RiskResult(None, f"Reward:risk {ratio:.2f} meets the minimum", risk, reward, ratio)

