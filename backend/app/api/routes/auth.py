"""Auth endpoints: register, login (httpOnly cookies), refresh, logout, me,
email verification, forgot/reset password, change password."""
from typing import Optional
from uuid import UUID

from fastapi import APIRouter, Cookie, Depends, HTTPException, Response, status
from sqlmodel import Session

from app.api.deps import (
    clear_auth_cookies,
    get_current_user,
    set_auth_cookies,
)
from app.core import security
from app.core.config import settings
from app.core.email import send_email
from app.db.session import get_session
from app.models.user import User
from app.schemas.auth import (
    ChangePasswordRequest,
    ForgotPasswordRequest,
    LoginRequest,
    LoginResponse,
    MessageResponse,
    RegisterRequest,
    ResetPasswordRequest,
    UserRead,
)
from app.services import auth as auth_service

TWOFA_COOKIE = "twofa_token"

router = APIRouter(prefix="/api/v1/auth", tags=["auth"])


def _to_user_read(user: User) -> UserRead:
    return UserRead(
        id=user.id,
        email=user.email,
        first_name=user.first_name,
        last_name=user.last_name,
        email_confirmed=user.email_confirmed,
        two_factor_enabled=user.two_factor_enabled,
        account_id=user.account_id,
        roles=[r.name for r in user.roles],
    )


@router.post("/register", response_model=UserRead, status_code=status.HTTP_201_CREATED)
def register(body: RegisterRequest, session: Session = Depends(get_session)) -> UserRead:
    try:
        user = auth_service.register_user(
            session,
            email=str(body.email),
            password=body.password,
            first_name=body.first_name,
            last_name=body.last_name,
            account_name=body.account_name,
        )
    except ValueError as exc:
        raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    token = security.create_email_token(str(user.id), "email_verify")
    send_email(
        user.email,
        "Confirm your email",
        f"Verify your account: {settings.frontend_origin}/verify-email?token={token}",
    )
    return _to_user_read(user)


@router.post("/login", response_model=LoginResponse)
def login(
    body: LoginRequest, response: Response, session: Session = Depends(get_session)
) -> LoginResponse:
    user = auth_service.authenticate(session, str(body.email), body.password)
    if user is None:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid email or password.")
    if not user.email_confirmed:
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Please confirm your email first.")
    if user.two_factor_enabled:
        # Issue a short-lived challenge cookie; full cookies are set after /2fa/verify.
        challenge = security.create_email_token(str(user.id), "2fa_login", hours=1)
        response.set_cookie(
            TWOFA_COOKIE,
            challenge,
            httponly=True,
            secure=settings.environment != "local",
            samesite="lax",
            max_age=300,
            path="/",
        )
        return LoginResponse(two_factor_required=True)
    set_auth_cookies(response, user)
    return LoginResponse(user=_to_user_read(user))


@router.post("/refresh", response_model=MessageResponse)
def refresh(
    response: Response,
    session: Session = Depends(get_session),
    refresh_token: Optional[str] = Cookie(default=None),
) -> MessageResponse:
    if not refresh_token:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Missing refresh token.")
    try:
        payload = security.decode_token(refresh_token)
    except Exception as exc:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid refresh token.") from exc
    if payload.get("type") != "refresh":
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid refresh token.")
    user = session.get(User, UUID(payload["sub"]))
    if user is None or user.is_deleted:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "User not found.")
    set_auth_cookies(response, user)
    return MessageResponse(detail="Token refreshed.")


@router.post("/logout", response_model=MessageResponse)
def logout(response: Response) -> MessageResponse:
    clear_auth_cookies(response)
    return MessageResponse(detail="Logged out.")


@router.get("/me", response_model=UserRead)
def me(user: User = Depends(get_current_user)) -> UserRead:
    return _to_user_read(user)


@router.get("/verify-email", response_model=MessageResponse)
def verify_email(token: str, session: Session = Depends(get_session)) -> MessageResponse:
    try:
        payload = security.decode_token(token)
    except Exception as exc:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid or expired token.") from exc
    if payload.get("type") != "email_verify":
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid token.")
    user = session.get(User, UUID(payload["sub"]))
    if user is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "User not found.")
    user.email_confirmed = True
    session.add(user)
    session.commit()
    return MessageResponse(detail="Email confirmed.")


@router.post("/forgot-password", response_model=MessageResponse)
def forgot_password(
    body: ForgotPasswordRequest, session: Session = Depends(get_session)
) -> MessageResponse:
    user = auth_service.get_user_by_email(session, str(body.email))
    if user is not None:  # never reveal whether the email exists
        token = security.create_email_token(str(user.id), "reset", hours=2)
        send_email(
            user.email,
            "Reset your password",
            f"Reset link: {settings.frontend_origin}/reset-password?token={token}",
        )
    return MessageResponse(detail="If that email exists, a reset link has been sent.")


@router.post("/reset-password", response_model=MessageResponse)
def reset_password(
    body: ResetPasswordRequest, session: Session = Depends(get_session)
) -> MessageResponse:
    try:
        payload = security.decode_token(body.token)
    except Exception as exc:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid or expired token.") from exc
    if payload.get("type") != "reset":
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid token.")
    user = session.get(User, UUID(payload["sub"]))
    if user is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "User not found.")
    auth_service.set_password(session, user, body.new_password)
    return MessageResponse(detail="Password updated.")


@router.post("/change-password", response_model=MessageResponse)
def change_password(
    body: ChangePasswordRequest,
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> MessageResponse:
    ok, _ = security.verify_password(body.current_password, user.password_hash)
    if not ok:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Current password is incorrect.")
    auth_service.set_password(session, user, body.new_password)
    return MessageResponse(detail="Password changed.")
