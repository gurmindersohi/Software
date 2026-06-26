"""Time-series insight snapshots (Tier 3) — we proxy live insights *and* persist
captures so analytics can show trends over time and feed client reports."""
from datetime import datetime, timezone
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class MetricSnapshot(SQLModel, table=True):
    __tablename__ = "metric_snapshots"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
    social_media_id: UUID = Field(foreign_key="social_media_accounts.id", index=True)
    client_id: Optional[UUID] = Field(default=None, foreign_key="clients.id", index=True)
    metric: str = Field(index=True)
    value: int = 0
    captured_at: datetime = Field(
        default_factory=lambda: datetime.now(timezone.utc), index=True
    )
