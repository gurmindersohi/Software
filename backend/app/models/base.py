"""Shared mixins for SQLModel tables."""
from datetime import datetime
from typing import Optional

from sqlmodel import Field, SQLModel


class AuditMixin(SQLModel):
    """Ports the C# `ModelBase` audit fields (CreatedBy/On, ModifiedBy/On, IsActive)."""

    created_by: Optional[str] = None
    created_on: Optional[datetime] = Field(default_factory=datetime.utcnow)
    modified_by: Optional[str] = None
    modified_on: Optional[datetime] = None
    is_active: Optional[bool] = True
