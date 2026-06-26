"""Account + Settings (merged — the old Account/Settings controllers were
near-identical) + Plan lookup. All scoped to the caller's tenant."""
from datetime import datetime, timezone

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import get_current_account
from app.core.quotas import limit_for
from app.core.tenancy import count_owned
from app.db.session import get_session
from app.models.account import Account
from app.models.plan import Plan
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia
from app.models.user import User
from app.schemas.account import AccountRead, AccountUpdate, PlanRead, UsageItem, UsageResponse

router = APIRouter(prefix="/api/v1", tags=["account"])


@router.get("/account", response_model=AccountRead)
def get_account(account: Account = Depends(get_current_account)) -> AccountRead:
    return AccountRead.model_validate(account)


@router.get("/account/usage", response_model=UsageResponse)
def account_usage(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> UsageResponse:
    plan = account.plan_name
    return UsageResponse(
        plan=plan,
        seats=UsageItem(
            used=count_owned(session, User, account.id, User.is_deleted == False),  # noqa: E712
            limit=limit_for(plan, "seats"),
        ),
        social_sets=UsageItem(
            used=count_owned(session, SocialMedia, account.id),
            limit=limit_for(plan, "social_sets"),
        ),
        scheduled_posts=UsageItem(
            used=count_owned(
                session, ScheduledPost, account.id, ScheduledPost.status.in_(["pending", "queued"])
            ),
            limit=limit_for(plan, "scheduled_posts"),
        ),
    )


@router.put("/account", response_model=AccountRead)
def update_account(
    body: AccountUpdate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> AccountRead:
    for key, value in body.model_dump(exclude_unset=True).items():
        setattr(account, key, value)
    account.modified_on = datetime.now(timezone.utc)
    session.add(account)
    session.commit()
    session.refresh(account)
    return AccountRead.model_validate(account)


@router.get("/plans/{name}", response_model=PlanRead)
def get_plan(
    name: str,
    session: Session = Depends(get_session),
    _: Account = Depends(get_current_account),
) -> PlanRead:
    plan = session.exec(select(Plan).where(Plan.name == name)).first()
    if plan is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Plan not found.")
    return PlanRead.model_validate(plan)
