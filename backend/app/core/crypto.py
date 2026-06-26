"""Symmetric encryption for secrets at rest (task 4.1).

Facebook/Instagram/ad-account access tokens are encrypted with Fernet
(AES-128-CBC + HMAC) before they touch the database, and decrypted only
server-side when a Graph call needs them. They are never returned to clients.

Keys (design hardening):
- `TOKEN_ENCRYPTION_KEY` is the **primary** key — separate from `SECRET_KEY`, so
  a JWT-signing leak does not also expose stored tokens (production enforces this
  via config validation).
- `TOKEN_ENCRYPTION_KEY_OLD` (comma-separated) holds retired keys → **MultiFernet**
  encrypts with the primary and still decrypts with the old ones, enabling
  zero-downtime key rotation (add new primary, keep old, re-encrypt, then drop old).
- If neither is set, a key is derived from `SECRET_KEY` for local/dev only.
"""
import base64
import hashlib
from typing import List

from cryptography.fernet import Fernet, InvalidToken, MultiFernet

from app.core.config import settings

# Cache MultiFernet per key-set so a config change (tests / rotation) rebuilds it.
_cache: dict = {}


def _keys() -> List[str]:
    if settings.token_encryption_key:
        keys = [settings.token_encryption_key]
        if settings.token_encryption_key_old:
            keys += [k.strip() for k in settings.token_encryption_key_old.split(",") if k.strip()]
        return keys
    digest = hashlib.sha256(settings.secret_key.encode()).digest()
    return [base64.urlsafe_b64encode(digest).decode()]


def _fernet() -> MultiFernet:
    key_tuple = tuple(_keys())
    if key_tuple not in _cache:
        _cache[key_tuple] = MultiFernet([Fernet(k.encode()) for k in key_tuple])
    return _cache[key_tuple]


def encrypt(plaintext: str) -> str:
    return _fernet().encrypt(plaintext.encode()).decode()


def decrypt(token: str) -> str:
    try:
        return _fernet().decrypt(token.encode()).decode()
    except InvalidToken as exc:
        raise ValueError("Could not decrypt value (wrong key or corrupt data).") from exc


def encrypt_optional(plaintext: "str | None") -> "str | None":
    return encrypt(plaintext) if plaintext else None
