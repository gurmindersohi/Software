"""Auth: User + Role (many-to-many). Unifies the old ASP.NET Identity `User`
(IdentityUser subclass) and `IdentityRole` into the new schema."""
from datetime import datetime
from typing import List, Optional
from uuid import UUID, uuid4

from sqlmodel import Field, Relationship, SQLModel

from app.models.base import AuditMixin


class UserRoleLink(SQLModel, table=True):
    __tablename__ = "user_roles"

    user_id: UUID = Field(foreign_key="users.id", primary_key=True)
    role_id: UUID = Field(foreign_key="roles.id", primary_key=True)


class Role(SQLModel, table=True):
    __tablename__ = "roles"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    name: str = Field(index=True, unique=True)

    users: List["User"] = Relationship(back_populates="roles", link_model=UserRoleLink)


class User(AuditMixin, table=True):
    __tablename__ = "users"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    email: str = Field(index=True, unique=True)
    normalized_email: Optional[str] = Field(default=None, index=True)
    password_hash: str
    email_confirmed: bool = False

    first_name: Optional[str] = None
    last_name: Optional[str] = None
    date_of_birth: Optional[datetime] = None
    gender: Optional[str] = None
    photo_path: Optional[str] = None
    phone_number: Optional[str] = None

    # Identity security/lockout fields
    two_factor_enabled: bool = False
    totp_secret: Optional[str] = None  # encrypted at rest when set
    recovery_codes: Optional[str] = None  # comma-separated sha256 hashes
    lockout_enabled: bool = True
    lockout_end: Optional[datetime] = None
    access_failed_count: int = 0
    security_stamp: Optional[str] = Field(default_factory=lambda: uuid4().hex)

    account_id: Optional[UUID] = Field(default=None, foreign_key="accounts.id", index=True)
    is_deleted: bool = False

    roles: List[Role] = Relationship(back_populates="users", link_model=UserRoleLink)
