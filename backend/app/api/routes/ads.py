"""Connected Facebook ad accounts, scoped to the caller's tenant."""
from typing import List

from fastapi import APIRouter, Depends, status
from sqlmodel import Session, select

from app.api.deps import get_current_account
from app.core import crypto
from app.db.session import get_session
from app.models.account import Account
from app.models.ad_account import AdAccount
from app.schemas.ads import AdAccountCreate, AdAccountRead

router = APIRouter(prefix="/api/v1/ad-accounts", tags=["ads"])


@router.get("", response_model=List[AdAccountRead])
def list_ad_accounts(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[AdAccount]:
    return session.exec(
        select(AdAccount).where(AdAccount.account_id == account.id)
    ).all()


@router.post("", response_model=AdAccountRead, status_code=status.HTTP_201_CREATED)
def save_ad_account(
    body: AdAccountCreate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> AdAccount:
    data = body.model_dump()
    data["access_token"] = crypto.encrypt_optional(data.get("access_token"))
    data["secret"] = crypto.encrypt_optional(data.get("secret"))
    ad_account = AdAccount(**data, account_id=account.id)
    session.add(ad_account)
    session.commit()
    session.refresh(ad_account)
    return ad_account
