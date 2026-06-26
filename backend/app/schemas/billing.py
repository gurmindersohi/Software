"""Billing API schemas (operates on the current tenant Account)."""
from datetime import datetime
from typing import Optional

from pydantic import BaseModel


class BillingRead(BaseModel):
    customer_id: Optional[str] = None
    subscription_id: Optional[str] = None
    is_account_paid: bool
    on_hold: bool
    trial_expiry: Optional[datetime] = None

    model_config = {"from_attributes": True}


class BillingUpdate(BaseModel):
    customer_id: Optional[str] = None
    subscription_id: Optional[str] = None
    is_account_paid: Optional[bool] = None
    on_hold: Optional[bool] = None
