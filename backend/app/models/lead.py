"""Lead (a prospective customer captured for a tenant). Ports C# `Lead`."""
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from sqlalchemy import UniqueConstraint
from sqlmodel import Field

from app.models.base import AuditMixin


class Lead(AuditMixin, table=True):
    __tablename__ = "leads"
    # DB-level backstop for the app's per-tenant dedup (closes the race window).
    __table_args__ = (UniqueConstraint("account_id", "email", name="uq_lead_account_email"),)

    id: UUID = Field(default_factory=uuid4, primary_key=True)  # was LeadId
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    full_name: Optional[str] = None
    email: str = Field(index=True)
    primary_phone: Optional[str] = None
    secondary_phone: Optional[str] = None
    date_of_birth: Optional[datetime] = None
    gender: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None

    account_id: UUID = Field(foreign_key="accounts.id", index=True)
    lead_source: Optional[str] = None

    is_phone_call_allowed: bool = False
    is_email_allowed: bool = False
    is_text_allowed: bool = False
    is_member: bool = False
