"""Client = a workspace under an agency Account (Tier 2).

An agency (the tenant Account) manages many Clients; pages, ad accounts, leads,
and scheduled posts can be assigned to a Client (null = agency-level).
"""
from datetime import datetime, timezone
from uuid import UUID, uuid4

from sqlmodel import Field, SQLModel


class Client(SQLModel, table=True):
    __tablename__ = "clients"

    id: UUID = Field(default_factory=uuid4, primary_key=True)
    account_id: UUID = Field(foreign_key="accounts.id", index=True)
    name: str
    is_deleted: bool = False
    created_on: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
