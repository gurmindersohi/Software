"""Connected social accounts (tokens), scoped to the caller's tenant."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import get_current_account
from app.core import crypto
from app.db.session import get_session
from app.models.account import Account
from app.models.social import SocialMedia
from app.schemas.social import SocialMediaCreate, SocialMediaRead

router = APIRouter(prefix="/api/v1/social", tags=["social"])


@router.get("", response_model=List[SocialMediaRead])
def list_tokens(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[SocialMedia]:
    return session.exec(
        select(SocialMedia).where(SocialMedia.account_id == account.id)
    ).all()


@router.get("/{platform}", response_model=SocialMediaRead)
def get_token_by_platform(
    platform: str,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> SocialMedia:
    row = session.exec(
        select(SocialMedia).where(
            SocialMedia.account_id == account.id, SocialMedia.type == platform
        )
    ).first()
    if row is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "No connection for that platform.")
    return row


@router.post("", response_model=SocialMediaRead, status_code=status.HTTP_201_CREATED)
def save_token(
    body: SocialMediaCreate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> SocialMedia:
    data = body.model_dump()
    data["access_token"] = crypto.encrypt_optional(data.get("access_token"))
    data["secret"] = crypto.encrypt_optional(data.get("secret"))
    token = SocialMedia(**data, account_id=account.id)
    session.add(token)
    session.commit()
    session.refresh(token)
    return token


@router.delete("/{token_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_token(
    token_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    token = session.get(SocialMedia, token_id)
    if token is None or token.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Connection not found.")
    session.delete(token)
    session.commit()
