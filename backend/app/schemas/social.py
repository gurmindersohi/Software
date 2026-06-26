"""Social connection (token) API schemas. Read schemas DELIBERATELY omit
`access_token`/`secret` so tokens are never returned to clients."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class SocialMediaCreate(BaseModel):
    page_id: Optional[str] = None
    name: Optional[str] = None
    image: Optional[str] = None
    type: str  # facebook | instagram
    access_token: Optional[str] = None
    secret: Optional[str] = None
    token_expiry_date: Optional[datetime] = None
    email: Optional[str] = None
    user_id: Optional[str] = None
    client_id: Optional[UUID] = None


class SocialMediaRead(BaseModel):
    id: UUID
    page_id: Optional[str] = None
    name: Optional[str] = None
    image: Optional[str] = None
    type: Optional[str] = None
    token_expiry_date: Optional[datetime] = None
    email: Optional[str] = None
    account_id: UUID
    client_id: Optional[UUID] = None

    model_config = {"from_attributes": True}
