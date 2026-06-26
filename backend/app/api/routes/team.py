"""Team management: list/invite/remove users in the current account + roles.
Mutations require the Owner role (task 8.4: ManageUsers/NewUser/Roles)."""
import secrets
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import get_current_account, get_current_user, require_owner
from app.core import security
from app.core.config import settings
from app.core.email import send_email
from app.db.session import get_session
from app.models.account import Account
from app.models.user import Role, User
from app.schemas.team import RoleCreate, RoleRead, TeamInvite, TeamMemberRead
from app.services import auth as auth_service

router = APIRouter(prefix="/api/v1", tags=["team"])


def _to_member(user: User) -> TeamMemberRead:
    return TeamMemberRead(
        id=user.id,
        email=user.email,
        first_name=user.first_name,
        last_name=user.last_name,
        email_confirmed=user.email_confirmed,
        is_deleted=user.is_deleted,
        roles=[r.name for r in user.roles],
    )


@router.get("/team", response_model=List[TeamMemberRead])
def list_team(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[TeamMemberRead]:
    users = session.exec(select(User).where(User.account_id == account.id)).all()
    return [_to_member(u) for u in users]


@router.post("/team", response_model=TeamMemberRead, status_code=status.HTTP_201_CREATED)
def invite_member(
    body: TeamInvite,
    _: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> TeamMemberRead:
    if auth_service.get_user_by_email(session, str(body.email)) is not None:
        raise HTTPException(status.HTTP_409_CONFLICT, "A user with this email already exists.")
    member = User(
        email=str(body.email),
        normalized_email=str(body.email).upper(),
        password_hash=security.hash_password(secrets.token_urlsafe(16)),
        first_name=body.first_name,
        account_id=account.id,
        email_confirmed=False,
    )
    member.roles.append(auth_service.get_or_create_role(session, body.role))
    session.add(member)
    session.commit()
    session.refresh(member)

    token = security.create_email_token(str(member.id), "reset", hours=72)
    send_email(
        member.email,
        "You've been invited to Sohi",
        f"Set your password: {settings.frontend_origin}/reset-password?token={token}",
    )
    return _to_member(member)


@router.delete("/team/{user_id}", status_code=status.HTTP_204_NO_CONTENT)
def remove_member(
    user_id: UUID,
    owner: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    if user_id == owner.id:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "You cannot remove yourself.")
    member = session.get(User, user_id)
    if member is None or member.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Member not found.")
    member.is_deleted = True
    session.add(member)
    session.commit()


@router.get("/roles", response_model=List[RoleRead])
def list_roles(
    _: User = Depends(get_current_user),
    session: Session = Depends(get_session),
) -> List[Role]:
    return session.exec(select(Role)).all()


@router.post("/roles", response_model=RoleRead, status_code=status.HTTP_201_CREATED)
def create_role(
    body: RoleCreate,
    _: User = Depends(require_owner),
    session: Session = Depends(get_session),
) -> Role:
    if session.exec(select(Role).where(Role.name == body.name)).first() is not None:
        raise HTTPException(status.HTTP_409_CONFLICT, "Role already exists.")
    role = Role(name=body.name)
    session.add(role)
    session.commit()
    session.refresh(role)
    return role
