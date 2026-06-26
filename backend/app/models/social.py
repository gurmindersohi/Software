"""Connected social account + its access token. Ports C# `SocialMedia`.

NOTE: `access_token` / `secret` must be ENCRYPTED AT REST (task 4.1). Stored
plaintext here only as a schema placeholder until the crypto layer lands.
"""
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class SocialMedia(SQLModel, table=True):
    __tablename__ = "social_media_accounts"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    page_id: Optional[str] = None
    name: Optional[str] = None
    image: Optional[str] = None
    type: Optional[str] = None  # facebook | instagram
    access_token: Optional[str] = None
    secret: Optional[str] = None
    created_on: Optional[datetime] = Field(default_factory=datetime.utcnow)
    token_expiry_date: Optional[datetime] = None
    email: Optional[str] = None
    user_id: Optional[str] = None
    # Normalised to a real FK to the tenant (was a loose string in the C# model).
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
