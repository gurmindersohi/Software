"""Processed webhook event ids — for idempotent webhook handling (Stripe redelivers)."""
from datetime import datetime, timezone

from sqlmodel import Field, SQLModel


class ProcessedWebhookEvent(SQLModel, table=True):
    __tablename__ = "processed_webhook_events"

    id: str = Field(primary_key=True)  # provider event id (e.g. Stripe evt_...)
    created_on: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
