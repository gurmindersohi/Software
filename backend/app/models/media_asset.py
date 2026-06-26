"""Uploaded media tracked for the library/browse view (gap #7)."""
from datetime import datetime, timezone
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class MediaAsset(SQLModel, table=True):
    __tablename__ = "media_assets"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
    url: str
    key: str
    content_type: Optional[str] = None
    kind: str = "file"  # image | video | file
    created_on: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
