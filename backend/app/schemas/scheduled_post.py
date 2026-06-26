"""Scheduled-post (Queue) API schemas."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class ScheduledPostCreate(BaseModel):
    social_media_id: UUID
    client_id: Optional[UUID] = None
    platform: str = "facebook"
    message: Optional[str] = None
    link: Optional[str] = None
    image_url: Optional[str] = None
    video_url: Optional[str] = None
    scheduled_at: datetime
    requires_approval: bool = False


class ScheduledPostRead(BaseModel):
    id: UUID
    account_id: UUID
    client_id: Optional[UUID] = None
    social_media_id: UUID
    platform: str
    requires_approval: bool = False
    approval_status: str = "approved"
    message: Optional[str] = None
    link: Optional[str] = None
    image_url: Optional[str] = None
    video_url: Optional[str] = None
    scheduled_at: datetime
    status: str
    attempts: int
    last_error: Optional[str] = None
    external_post_id: Optional[str] = None

    model_config = {"from_attributes": True}
