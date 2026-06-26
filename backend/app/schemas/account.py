"""Account (tenant) + Plan API schemas."""
from datetime import datetime
from decimal import Decimal
from typing import Optional
from uuid import UUID

from pydantic import BaseModel, EmailStr


class AccountRead(BaseModel):
    id: UUID
    account_name: Optional[str] = None
    account_type: Optional[str] = None
    email: Optional[EmailStr] = None
    phone: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None
    users_limit: Optional[str] = None
    logo: Optional[str] = None
    trial_expiry: Optional[datetime] = None
    is_account_paid: bool
    on_hold: bool

    model_config = {"from_attributes": True}


class AccountUpdate(BaseModel):
    account_name: Optional[str] = None
    account_type: Optional[str] = None
    email: Optional[EmailStr] = None
    phone: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None
    logo: Optional[str] = None


class PlanRead(BaseModel):
    id: UUID
    name: Optional[str] = None
    type: Optional[str] = None
    price: Decimal
    billing_period: Optional[str] = None
    tax: Decimal
    total: Decimal

    model_config = {"from_attributes": True}
