"""Shared FastAPI dependencies: current user + auth cookie helpers."""
from typing import Optional
from uuid import UUID

from fastapi import Cookie, Depends, Header, HTTPException, Response, status
from sqlmodel import Session

from app.core import security
from app.core.config import settings
from app.db.session import get_session
from app.models.account import Account
from app.models.user import User

ACCESS_COOKIE = "access_token"
REFRESH_COOKIE = "refresh_token"

_UNAUTHORIZED = HTTPException(
    status_code=status.HTTP_401_UNAUTHORIZED,
    detail="Not authenticated",
    headers={"WWW-Authenticate": "Bearer"},
)


def _secure_cookie() -> bool:
    return settings.environment != "local"


def set_auth_cookies(response: Response, user: User) -> None:
    access = security.create_access_token(str(user.id))
    refresh = security.create_refresh_token(str(user.id))
    response.set_cookie(
        ACCESS_COOKIE,
        access,
        httponly=True,
        secure=_secure_cookie(),
        samesite="lax",
        max_age=settings.access_token_expire_minutes * 60,
        path="/",
    )
    response.set_cookie(
        REFRESH_COOKIE,
        refresh,
        httponly=True,
        secure=_secure_cookie(),
        samesite="lax",
        max_age=settings.refresh_token_expire_days * 86400,
        path="/",
    )


def clear_auth_cookies(response: Response) -> None:
    response.delete_cookie(ACCESS_COOKIE, path="/")
    response.delete_cookie(REFRESH_COOKIE, path="/")


def get_current_user(
    session: Session = Depends(get_session),
    access_token: Optional[str] = Cookie(default=None),
    authorization: Optional[str] = Header(default=None),
) -> User:
    token = access_token
    if not token and authorization and authorization.lower().startswith("bearer "):
        token = authorization[7:]
    if not token:
        raise _UNAUTHORIZED
    try:
        payload = security.decode_token(token)
    except Exception as exc:  # invalid/expired
        raise _UNAUTHORIZED from exc
    if payload.get("type") != "access":
        raise _UNAUTHORIZED
    try:
        user = session.get(User, UUID(payload["sub"]))
    except (KeyError, ValueError) as exc:
        raise _UNAUTHORIZED from exc
    if user is None or user.is_deleted:
        raise _UNAUTHORIZED
    return user


def get_current_account(
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> Account:
    """The tenant Account owned by the current user. Every tenant-scoped route
    depends on this so callers can only ever touch their own account's data."""
    if user.account_id is None:
        raise HTTPException(status.HTTP_403_FORBIDDEN, "User is not linked to an account.")
    account = session.get(Account, user.account_id)
    if account is None or account.is_deleted:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Account not found.")
    return account
