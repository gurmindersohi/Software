"""Connected Facebook ad account. Ports C# `AdAccount`.

NOTE: `access_token` / `secret` must be ENCRYPTED AT REST (task 4.1).
"""
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class AdAccount(SQLModel, table=True):
    __tablename__ = "ad_accounts"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    user_account_id: Optional[str] = None  # external Meta ad-account id (act_...)
    name: Optional[str] = None
    image: Optional[str] = None
    type: Optional[str] = None
    access_token: Optional[str] = None
    secret: Optional[str] = None
    created_on: Optional[datetime] = Field(default_factory=datetime.utcnow)
    token_expiry_date: Optional[datetime] = None
    email: Optional[str] = None
    user_id: Optional[str] = None
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
