"""Subscription Plan. Ports C# `Plan` (money fields keep Numeric(18,2) per EF config)."""
from datetime import datetime
from decimal import Decimal
from typing import Optional
from uuid import UUID, uuid4

from sqlalchemy import Column, Numeric
from sqlmodel import Field, SQLModel

_MONEY = lambda: Column(Numeric(18, 2), nullable=False)  # noqa: E731


class Plan(SQLModel, table=True):
    __tablename__ = "plans"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    name: Optional[str] = None
    type: Optional[str] = None
    price: Decimal = Field(default=Decimal("0"), sa_column=_MONEY())
    billing_period: Optional[str] = None
    product_id: Optional[str] = None  # Stripe product/price id
    tax: Decimal = Field(default=Decimal("0"), sa_column=_MONEY())
    total: Decimal = Field(default=Decimal("0"), sa_column=_MONEY())
    created_on: Optional[datetime] = Field(default_factory=datetime.utcnow)
    modified_on: Optional[datetime] = None
