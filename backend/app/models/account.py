"""Account = the tenant (a business/agency). Ports C# `Account`."""
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field

from app.models.base import AuditMixin


class Account(AuditMixin, table=True):
    __tablename__ = "accounts"

    id: UUID = Field(default_factory=uuid4, primary_key=True)  # was AccountId
    account_name: Optional[str] = None
    account_type: Optional[str] = None
    email: Optional[str] = Field(default=None, index=True)
    phone: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None
    users_limit: Optional[str] = None
    logo: Optional[str] = None
    plan_name: str = Field(default="trial")  # trial | basic | premium | unlimited

    # Trial / subscription lifecycle (drives tasks 3.9 + billing)
    trial_expiry: Optional[datetime] = None
    is_account_paid: bool = False
    is_deleted: bool = False
    on_hold: bool = False
    hold_date: Optional[datetime] = None

    # Stripe linkage
    customer_id: Optional[str] = None
    subscription_id: Optional[str] = None
