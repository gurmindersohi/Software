"""Two-factor auth schemas."""
from typing import List

from pydantic import BaseModel


class TwoFASetupResponse(BaseModel):
    secret: str
    otpauth_uri: str


class CodeRequest(BaseModel):
    code: str


class RecoveryCodesResponse(BaseModel):
    recovery_codes: List[str]
