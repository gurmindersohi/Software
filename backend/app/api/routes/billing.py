"""Billing details for the current tenant Account."""
from datetime import datetime, timezone

from fastapi import APIRouter, Depends
from sqlmodel import Session

from app.api.deps import get_current_account
from app.db.session import get_session
from app.models.account import Account
from app.schemas.billing import BillingRead, BillingUpdate

router = APIRouter(prefix="/api/v1/billing", tags=["billing"])


@router.get("", response_model=BillingRead)
def get_billing(account: Account = Depends(get_current_account)) -> BillingRead:
    return BillingRead.model_validate(account)


@router.put("", response_model=BillingRead)
def update_billing(
    body: BillingUpdate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> BillingRead:
    for key, value in body.model_dump(exclude_unset=True).items():
        setattr(account, key, value)
    account.modified_on = datetime.now(timezone.utc)
    session.add(account)
    session.commit()
    session.refresh(account)
    return BillingRead.model_validate(account)
