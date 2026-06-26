"""Team management (users within an account) + roles."""
from typing import List, Optional
from uuid import UUID

from pydantic import BaseModel, EmailStr


class TeamMemberRead(BaseModel):
    id: UUID
    email: EmailStr
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    email_confirmed: bool
    is_deleted: bool
    roles: List[str] = []


class TeamInvite(BaseModel):
    email: EmailStr
    first_name: Optional[str] = None
    role: str = "User"


class RoleRead(BaseModel):
    id: UUID
    name: str

    model_config = {"from_attributes": True}


class RoleCreate(BaseModel):
    name: str
