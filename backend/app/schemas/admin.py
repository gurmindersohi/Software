"""Admin (back-office) schemas."""
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class AdminAccountRead(BaseModel):
    id: UUID
    account_name: Optional[str] = None
    plan_name: str
    is_account_paid: bool
    on_hold: bool
    is_deleted: bool

    model_config = {"from_attributes": True}


class AdminMetrics(BaseModel):
    accounts: int
    paid_accounts: int
    users: int
    leads: int
