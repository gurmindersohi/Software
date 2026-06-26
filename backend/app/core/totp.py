"""RFC 6238 TOTP (SHA-1, 30s, 6 digits) — hand-rolled, no external dependency."""
import base64
import hashlib
import hmac
import secrets
import struct
import time
from typing import Optional
from urllib.parse import quote, urlencode

_DIGITS = 6
_PERIOD = 30


def generate_secret() -> str:
    """A new base32 TOTP secret (160-bit)."""
    return base64.b32encode(secrets.token_bytes(20)).decode().rstrip("=")


def _hotp(secret_b32: str, counter: int) -> str:
    key = base64.b32decode(secret_b32 + "=" * (-len(secret_b32) % 8))
    digest = hmac.new(key, struct.pack(">Q", counter), hashlib.sha1).digest()
    offset = digest[-1] & 0x0F
    code = struct.unpack(">I", digest[offset : offset + 4])[0] & 0x7FFFFFFF
    return f"{code % (10 ** _DIGITS):0{_DIGITS}d}"


def verify(secret_b32: str, code: str, *, window: int = 1, at: Optional[float] = None) -> bool:
    """Validate a code, tolerating +/- `window` steps for clock skew."""
    if not secret_b32 or not code:
        return False
    counter = int((at if at is not None else time.time()) // _PERIOD)
    return any(
        hmac.compare_digest(_hotp(secret_b32, counter + offset), code)
        for offset in range(-window, window + 1)
    )


def provisioning_uri(secret_b32: str, account_name: str, issuer: str = "Sohi") -> str:
    """otpauth:// URI for authenticator apps (rendered as a QR by the client)."""
    label = quote(f"{issuer}:{account_name}")
    params = urlencode({"secret": secret_b32, "issuer": issuer})
    return f"otpauth://totp/{label}?{params}"
