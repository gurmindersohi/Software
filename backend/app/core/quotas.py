"""Per-plan quota enforcement (task 3.8). Pure domain logic — no HTTP. Limits
mirror the pricing tiers; `None` means unlimited."""
from typing import Optional, Type
from uuid import UUID

from sqlmodel import Session, SQLModel

from app.core.exceptions import PaymentRequiredError
from app.core.tenancy import count_owned

PLAN_QUOTAS = {
    "trial": {"seats": 1, "social_sets": 3, "scheduled_posts": 30},
    "basic": {"seats": 1, "social_sets": 3, "scheduled_posts": 30},
    "premium": {"seats": 3, "social_sets": 5, "scheduled_posts": 70},
    "unlimited": {"seats": None, "social_sets": None, "scheduled_posts": None},
}


def limit_for(plan_name: Optional[str], resource: str) -> Optional[int]:
    quotas = PLAN_QUOTAS.get((plan_name or "trial").lower(), PLAN_QUOTAS["trial"])
    return quotas.get(resource)


def enforce_quota(
    session: Session,
    *,
    plan_name: Optional[str],
    account_id: UUID,
    resource: str,
    model: Type[SQLModel],
    extra=None,
) -> None:
    """Raise if creating one more `resource` would exceed the plan limit."""
    limit = limit_for(plan_name, resource)
    if limit is None:
        return  # unlimited
    if count_owned(session, model, account_id, extra) >= limit:
        raise PaymentRequiredError(
            f"Your plan allows {limit} {resource.replace('_', ' ')}. Upgrade to add more."
        )
