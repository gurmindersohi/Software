"""Two-factor auth: setup/enable/disable + the login step-2 verify (task 2.8)."""
from typing import Optional
from uuid import UUID

from fastapi import APIRouter, Cookie, Depends, HTTPException, Response, status
from sqlmodel import Session

from app.api.deps import get_current_user, set_auth_cookies
from app.api.routes.auth import TWOFA_COOKIE, _to_user_read
from app.core import security, totp
from app.db.session import get_session
from app.models.user import User
from app.schemas.auth import MessageResponse, UserRead
from app.schemas.twofa import CodeRequest, RecoveryCodesResponse, TwoFASetupResponse
from app.services import twofa as twofa_service

router = APIRouter(prefix="/api/v1/auth/2fa", tags=["2fa"])


@router.post("/setup", response_model=TwoFASetupResponse)
def setup(
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> TwoFASetupResponse:
    secret = twofa_service.begin_setup(user)
    session.add(user)
    session.commit()
    return TwoFASetupResponse(
        secret=secret, otpauth_uri=totp.provisioning_uri(secret, user.email)
    )


@router.post("/enable", response_model=RecoveryCodesResponse)
def enable(
    body: CodeRequest,
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> RecoveryCodesResponse:
    if user.totp_secret is None:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Run /setup first.")
    if not twofa_service.verify_code(user, body.code):
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid code.")
    user.two_factor_enabled = True
    codes = twofa_service.generate_recovery_codes(user)
    session.add(user)
    session.commit()
    return RecoveryCodesResponse(recovery_codes=codes)


@router.post("/disable", response_model=MessageResponse)
def disable(
    body: CodeRequest,
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> MessageResponse:
    if not user.two_factor_enabled:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Two-factor is not enabled.")
    if not (
        twofa_service.verify_code(user, body.code)
        or twofa_service.consume_recovery_code(user, body.code)
    ):
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid code.")
    twofa_service.disable(user)
    session.add(user)
    session.commit()
    return MessageResponse(detail="Two-factor disabled.")


@router.post("/verify", response_model=UserRead)
def verify_login(
    body: CodeRequest,
    response: Response,
    session: Session = Depends(get_session),
    twofa_token: Optional[str] = Cookie(default=None),
) -> UserRead:
    """Login step 2: validate the TOTP / recovery code against the challenge cookie."""
    if not twofa_token:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "No pending two-factor login.")
    try:
        payload = security.decode_token(twofa_token)
    except Exception as exc:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Challenge expired.") from exc
    if payload.get("type") != "2fa_login":
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid challenge.")

    user = session.get(User, UUID(payload["sub"]))
    if user is None or user.is_deleted:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "User not found.")

    if not (
        twofa_service.verify_code(user, body.code)
        or twofa_service.consume_recovery_code(user, body.code)
    ):
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid code.")

    session.add(user)  # persists recovery-code consumption, if any
    session.commit()
    set_auth_cookies(response, user)
    response.delete_cookie(TWOFA_COOKIE, path="/")
    return _to_user_read(user)
