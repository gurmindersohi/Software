"""Client (workspace) schemas."""
from typing import Optional
from uuid import UUID

from pydantic import BaseModel


class ClientCreate(BaseModel):
    name: str


class ClientUpdate(BaseModel):
    name: Optional[str] = None


class ClientRead(BaseModel):
    id: UUID
    name: str

    model_config = {"from_attributes": True}
