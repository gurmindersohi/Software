"""Platform back-office (Tier 4) — superuser only. Cross-tenant by design."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy import func
from sqlmodel import Session, select

from app.api.deps import require_superuser
from app.core.exceptions import NotFoundError
from app.db.session import get_session
from app.models.account import Account
from app.models.lead import Lead
from app.models.user import User
from app.schemas.admin import AdminAccountRead, AdminMetrics

router = APIRouter(prefix="/api/v1/admin", tags=["admin"])


@router.get("/accounts", response_model=List[AdminAccountRead])
def list_accounts(
    _: User = Depends(require_superuser),
    session: Session = Depends(get_session),
) -> List[Account]:
    return session.exec(select(Account)).all()


@router.post("/accounts/{account_id}/suspend", response_model=AdminAccountRead)
def suspend_account(
    account_id: UUID,
    _: User = Depends(require_superuser),
    session: Session = Depends(get_session),
) -> Account:
    account = session.get(Account, account_id)
    if account is None:
        raise NotFoundError("Account not found.")
    account.on_hold = True
    session.add(account)
    session.commit()
    session.refresh(account)
    return account


@router.post("/accounts/{account_id}/unsuspend", response_model=AdminAccountRead)
def unsuspend_account(
    account_id: UUID,
    _: User = Depends(require_superuser),
    session: Session = Depends(get_session),
) -> Account:
    account = session.get(Account, account_id)
    if account is None:
        raise NotFoundError("Account not found.")
    account.on_hold = False
    session.add(account)
    session.commit()
    session.refresh(account)
    return account


@router.get("/metrics", response_model=AdminMetrics)
def platform_metrics(
    _: User = Depends(require_superuser),
    session: Session = Depends(get_session),
) -> AdminMetrics:
    def count(model, *conds) -> int:
        stmt = select(func.count()).select_from(model)
        for c in conds:
            stmt = stmt.where(c)
        return session.scalar(stmt) or 0

    return AdminMetrics(
        accounts=count(Account),
        paid_accounts=count(Account, Account.is_account_paid == True),  # noqa: E712
        users=count(User, User.is_deleted == False),  # noqa: E712
        leads=count(Lead),
    )
