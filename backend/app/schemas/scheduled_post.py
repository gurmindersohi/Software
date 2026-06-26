"""Scheduled-post (Queue) API schemas."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class ScheduledPostCreate(BaseModel):
    social_media_id: UUID
    platform: str = "facebook"
    message: Optional[str] = None
    link: Optional[str] = None
    image_url: Optional[str] = None
    video_url: Optional[str] = None
    scheduled_at: datetime


class ScheduledPostRead(BaseModel):
    id: UUID
    account_id: UUID
    social_media_id: UUID
    platform: str
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
