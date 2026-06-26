"""Ad account API schemas. Read schema omits token/secret."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class AdAccountCreate(BaseModel):
    user_account_id: Optional[str] = None  # external Meta ad-account id (act_...)
    name: Optional[str] = None
    image: Optional[str] = None
    type: Optional[str] = None
    access_token: Optional[str] = None
    secret: Optional[str] = None
    token_expiry_date: Optional[datetime] = None
    email: Optional[str] = None
    user_id: Optional[str] = None


class AdAccountRead(BaseModel):
    id: UUID
    user_account_id: Optional[str] = None
    name: Optional[str] = None
    image: Optional[str] = None
    type: Optional[str] = None
    account_id: UUID

    model_config = {"from_attributes": True}
