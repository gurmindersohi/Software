"""Audit log (Tier 4) — an append-only trail of significant actions."""
from datetime import datetime, timezone
from typing import Optional
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class AuditLog(SQLModel, table=True):
    __tablename__ = "audit_logs"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    account_id: Optional[UUID] = Field(default=None, foreign_key="accounts.id", index=True)
    user_id: Optional[UUID] = Field(default=None, foreign_key="users.id", index=True)
    action: str = Field(index=True)  # e.g. "post.approved", "client.created"
    detail: Optional[str] = None
    created_on: datetime = Field(
        default_factory=lambda: datetime.now(timezone.utc), index=True
    )
