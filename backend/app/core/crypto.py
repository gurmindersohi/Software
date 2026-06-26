"""Symmetric encryption for secrets at rest (task 4.1).

Facebook/Instagram/ad-account access tokens are encrypted with Fernet
(AES-128-CBC + HMAC) before they touch the database, and decrypted only
server-side when a Graph call needs them. They are never returned to clients.

The key comes from `TOKEN_ENCRYPTION_KEY` (a urlsafe-base64 32-byte Fernet key).
If unset, it is derived deterministically from `SECRET_KEY` so local/dev works
out of the box — production MUST set a dedicated key.
"""
import base64
import hashlib
from functools import lru_cache

from cryptography.fernet import Fernet, InvalidToken

from app.core.config import settings


@lru_cache(maxsize=1)
def _fernet() -> Fernet:
    if settings.token_encryption_key:
        return Fernet(settings.token_encryption_key.encode())
    # Derive a valid 32-byte Fernet key from the app secret (dev fallback).
    digest = hashlib.sha256(settings.secret_key.encode()).digest()
    return Fernet(base64.urlsafe_b64encode(digest))


def encrypt(plaintext: str) -> str:
    return _fernet().encrypt(plaintext.encode()).decode()


def decrypt(token: str) -> str:
    try:
        return _fernet().decrypt(token.encode()).decode()
    except InvalidToken as exc:
        raise ValueError("Could not decrypt value (wrong key or corrupt data).") from exc


def encrypt_optional(plaintext: "str | None") -> "str | None":
    return encrypt(plaintext) if plaintext else None
