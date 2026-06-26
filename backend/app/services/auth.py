"""Auth domain logic: registration, lookup, authentication, password changes."""
import secrets
from datetime import datetime, timedelta, timezone
from typing import Optional

from sqlmodel import Session, select

from app.core import security
from app.models.account import Account
from app.models.user import Role, User

TRIAL_DAYS = 14
DEFAULT_OWNER_ROLE = "Owner"


def get_user_by_email(session: Session, email: str) -> Optional[User]:
    return session.exec(
        select(User).where(User.normalized_email == email.upper())
    ).first()


def get_or_create_role(session: Session, name: str) -> Role:
    role = session.exec(select(Role).where(Role.name == name)).first()
    if role is None:
        role = Role(name=name)
        session.add(role)
        session.flush()
    return role


def register_user(
    session: Session,
    *,
    email: str,
    password: str,
    first_name: Optional[str] = None,
    last_name: Optional[str] = None,
    account_name: Optional[str] = None,
) -> User:
    if get_user_by_email(session, email) is not None:
        raise ValueError("A user with this email already exists.")

    # First user of a new signup owns a fresh tenant Account on a trial.
    account = Account(
        account_name=account_name or email,
        email=email,
        trial_expiry=datetime.now(timezone.utc) + timedelta(days=TRIAL_DAYS),
    )
    session.add(account)
    session.flush()

    user = User(
        email=email,
        normalized_email=email.upper(),
        password_hash=security.hash_password(password),
        first_name=first_name,
        last_name=last_name,
        account_id=account.id,
        email_confirmed=False,
    )
    user.roles.append(get_or_create_role(session, DEFAULT_OWNER_ROLE))
    session.add(user)
    session.commit()
    session.refresh(user)
    return user


def authenticate(session: Session, email: str, password: str) -> Optional[User]:
    user = get_user_by_email(session, email)
    if user is None or user.is_deleted:
        return None
    ok, needs_rehash = security.verify_password(password, user.password_hash)
    if not ok:
        return None
    if needs_rehash:  # migrate legacy ASP.NET hash → new format transparently
        user.password_hash = security.hash_password(password)
        session.add(user)
        session.commit()
    return user


def set_password(session: Session, user: User, new_password: str) -> None:
    user.password_hash = security.hash_password(new_password)
    user.security_stamp = secrets.token_hex(16)
    session.add(user)
    session.commit()
