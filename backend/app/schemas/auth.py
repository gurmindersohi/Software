"""Auth request/response schemas."""
from typing import List, Optional
from uuid import UUID

from pydantic import BaseModel, EmailStr, Field


class RegisterRequest(BaseModel):
    email: EmailStr
    password: str = Field(min_length=6)  # original Identity policy: MinimumLength=6
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    account_name: Optional[str] = None


class LoginRequest(BaseModel):
    email: EmailStr
    password: str


class UserRead(BaseModel):
    id: UUID
    email: EmailStr
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    email_confirmed: bool
    two_factor_enabled: bool = False
    is_superuser: bool = False
    account_id: Optional[UUID] = None
    roles: List[str] = []


class LoginResponse(BaseModel):
    two_factor_required: bool = False
    user: Optional[UserRead] = None


class MessageResponse(BaseModel):
    detail: str


class ForgotPasswordRequest(BaseModel):
    email: EmailStr


class ResetPasswordRequest(BaseModel):
    token: str
    new_password: str = Field(min_length=6)


class ChangePasswordRequest(BaseModel):
    current_password: str
    new_password: str = Field(min_length=6)


class ChangeEmailRequest(BaseModel):
    new_email: EmailStr
    password: str


class DeleteAccountRequest(BaseModel):
    password: str
