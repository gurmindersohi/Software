"""ScheduledPost — the social "Queue" feature (task 5.2).

A row is created (status=pending) when a user schedules a post. A worker sweep
(`enqueue_due_posts`) picks up due rows and a job publishes them via Graph,
updating status / attempts / external id.
"""
from datetime import datetime, timezone
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class ScheduledPost(SQLModel, table=True):
    __tablename__ = "scheduled_posts"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
    social_media_id: UUID = Field(foreign_key="social_media_accounts.id", index=True)

    platform: str = "facebook"  # facebook | instagram
    message: Optional[str] = None
    link: Optional[str] = None
    image_url: Optional[str] = None

    scheduled_at: datetime = Field(index=True)
    status: str = Field(default="pending", index=True)  # pending|queued|published|failed
    attempts: int = 0
    last_error: Optional[str] = None
    external_post_id: Optional[str] = None

    created_on: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
    modified_on: Optional[datetime] = None
