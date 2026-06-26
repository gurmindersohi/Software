"""Lead API schemas."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel, EmailStr


class LeadBase(BaseModel):
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    full_name: Optional[str] = None
    email: EmailStr
    primary_phone: Optional[str] = None
    secondary_phone: Optional[str] = None
    date_of_birth: Optional[datetime] = None
    gender: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None
    lead_source: Optional[str] = None
    is_phone_call_allowed: bool = False
    is_email_allowed: bool = False
    is_text_allowed: bool = False
    is_member: bool = False


class LeadCreate(LeadBase):
    pass


class LeadUpdate(BaseModel):
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    full_name: Optional[str] = None
    email: Optional[EmailStr] = None
    primary_phone: Optional[str] = None
    secondary_phone: Optional[str] = None
    date_of_birth: Optional[datetime] = None
    gender: Optional[str] = None
    address: Optional[str] = None
    city: Optional[str] = None
    province: Optional[str] = None
    country: Optional[str] = None
    postal_code: Optional[str] = None
    lead_source: Optional[str] = None
    is_phone_call_allowed: Optional[bool] = None
    is_email_allowed: Optional[bool] = None
    is_text_allowed: Optional[bool] = None
    is_member: Optional[bool] = None


class LeadRead(LeadBase):
    id: UUID
    account_id: UUID

    model_config = {"from_attributes": True}
