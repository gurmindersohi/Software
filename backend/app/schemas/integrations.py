"""Schemas for OAuth connect, billing, and media upload."""
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class FacebookConnectResponse(BaseModel):
    authorize_url: str


class ConnectionResult(BaseModel):
    detail: str
    pages_connected: int = 0


class SubscriptionRequest(BaseModel):
    price_id: str
    plan: str = "premium"  # basic | premium | unlimited — drives quota limits


class SubscriptionResult(BaseModel):
    subscription_id: str
    status: str
    client_secret: Optional[str] = None


class MediaUploadResponse(BaseModel):
    url: str
    key: str


class MediaAssetRead(BaseModel):
    id: UUID
    url: str
    kind: str
    content_type: Optional[str] = None

    model_config = {"from_attributes": True}
