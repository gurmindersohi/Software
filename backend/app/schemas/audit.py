"""Audit log read schema."""
from datetime import datetime
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class AuditLogRead(BaseModel):
    id: UUID
    action: str
    detail: Optional[str] = None
    user_id: Optional[UUID] = None
    created_on: datetime

    model_config = {"from_attributes": True}
