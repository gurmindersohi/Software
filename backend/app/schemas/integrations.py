"""Schemas for OAuth connect, billing, and media upload."""
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


class MediaUploadResponse(BaseModel):
    url: str
    key: str
