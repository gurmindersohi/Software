"""Password hashing + JWT.

Password strategy (task 2.2): NEW passwords use PBKDF2-HMAC-SHA256 in a
self-describing string format. LEGACY passwords migrated from ASP.NET Core
Identity are verified with a faithful port of Microsoft's `PasswordHasher`
(v2 = 0x00, v3 = 0x01 marker byte). On a successful legacy verify the caller
re-hashes with the new format, so users never need a password reset.
"""
import base64
import hashlib
import hmac
import secrets
from datetime import datetime, timedelta, timezone
from typing import Optional, Tuple

import jwt

from app.core.config import settings

# --- New-password hashing -------------------------------------------------
_PBKDF2_ALGO = "sha256"
_PBKDF2_ITERATIONS = 600_000
_SALT_BYTES = 16
_NEW_PREFIX = "pbkdf2_sha256"


def hash_password(password: str) -> str:
    salt = secrets.token_bytes(_SALT_BYTES)
    dk = hashlib.pbkdf2_hmac(_PBKDF2_ALGO, password.encode(), salt, _PBKDF2_ITERATIONS)
    salt_b64 = base64.b64encode(salt).decode()
    dk_b64 = base64.b64encode(dk).decode()
    return f"{_NEW_PREFIX}${_PBKDF2_ITERATIONS}${salt_b64}${dk_b64}"


def _verify_new(password: str, stored: str) -> bool:
    try:
        prefix, iters_s, salt_b64, hash_b64 = stored.split("$")
    except ValueError:
        return False
    if prefix != _NEW_PREFIX:
        return False
    salt = base64.b64decode(salt_b64)
    expected = base64.b64decode(hash_b64)
    dk = hashlib.pbkdf2_hmac(
        _PBKDF2_ALGO, password.encode(), salt, int(iters_s), dklen=len(expected)
    )
    return hmac.compare_digest(dk, expected)


# --- Legacy ASP.NET Core Identity PasswordHasher --------------------------
# v3 layout: [0]=0x01, [1:5]=PRF(uint32 BE), [5:9]=iter(uint32 BE),
#            [9:13]=saltLen(uint32 BE), [13:13+saltLen]=salt, rest=subkey.
# v2 layout: [0]=0x00, [1:17]=salt(16), [17:49]=subkey(32); HMACSHA1, 1000 iters.
_PRF = {0: "sha1", 1: "sha256", 2: "sha512"}


def _u32_be(b: bytes, offset: int) -> int:
    return int.from_bytes(b[offset : offset + 4], "big")


def verify_aspnet_identity_hash(password: str, stored_b64: str) -> bool:
    try:
        data = base64.b64decode(stored_b64)
    except Exception:
        return False
    if not data:
        return False
    marker = data[0]
    if marker == 0x00:  # version 2
        if len(data) != 1 + 16 + 32:
            return False
        salt, subkey = data[1:17], data[17:49]
        dk = hashlib.pbkdf2_hmac("sha1", password.encode(), salt, 1000, dklen=32)
        return hmac.compare_digest(dk, subkey)
    if marker == 0x01:  # version 3
        prf = _u32_be(data, 1)
        iters = _u32_be(data, 5)
        salt_len = _u32_be(data, 9)
        if prf not in _PRF or salt_len == 0:
            return False
        salt = data[13 : 13 + salt_len]
        subkey = data[13 + salt_len :]
        if len(salt) != salt_len or not subkey:
            return False
        dk = hashlib.pbkdf2_hmac(_PRF[prf], password.encode(), salt, iters, dklen=len(subkey))
        return hmac.compare_digest(dk, subkey)
    return False


def verify_password(password: str, stored: str) -> Tuple[bool, bool]:
    """Return (is_valid, needs_rehash). Legacy ASP.NET hashes set needs_rehash."""
    if not stored:
        return (False, False)
    if "$" in stored:  # our self-describing format
        return (_verify_new(password, stored), False)
    return (verify_aspnet_identity_hash(password, stored), True)


# --- JWT ------------------------------------------------------------------
def _now() -> datetime:
    return datetime.now(timezone.utc)


def _create_token(
    subject: str, token_type: str, expires: timedelta, extra: Optional[dict] = None
) -> str:
    payload = {"sub": subject, "type": token_type, "iat": _now(), "exp": _now() + expires}
    if extra:
        payload.update(extra)
    return jwt.encode(payload, settings.secret_key, algorithm=settings.jwt_algorithm)


def create_access_token(subject: str, extra: Optional[dict] = None) -> str:
    return _create_token(
        subject, "access", timedelta(minutes=settings.access_token_expire_minutes), extra
    )


def create_refresh_token(subject: str, extra: Optional[dict] = None) -> str:
    return _create_token(
        subject, "refresh", timedelta(days=settings.refresh_token_expire_days), extra
    )


def create_email_token(subject: str, purpose: str, hours: int = 24) -> str:
    """Short-lived token for email-verify / password-reset (purpose = token type)."""
    return _create_token(subject, purpose, timedelta(hours=hours))


def decode_token(token: str) -> dict:
    return jwt.decode(token, settings.secret_key, algorithms=[settings.jwt_algorithm])
