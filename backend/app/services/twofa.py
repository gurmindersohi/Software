"""Two-factor (TOTP) domain logic — operates on a User (task 2.8)."""
import hashlib
import secrets
from typing import List

from app.core import crypto, totp
from app.models.user import User

RECOVERY_CODE_COUNT = 10


def begin_setup(user: User) -> str:
    """Generate + store (encrypted) a TOTP secret; returns plaintext for the QR.
    Does NOT enable 2FA — the user must confirm a code via `enable`."""
    secret = totp.generate_secret()
    user.totp_secret = crypto.encrypt(secret)
    return secret


def _secret(user: User) -> str:
    return crypto.decrypt(user.totp_secret) if user.totp_secret else ""


def verify_code(user: User, code: str) -> bool:
    return totp.verify(_secret(user), code)


def generate_recovery_codes(user: User) -> List[str]:
    codes = [f"{secrets.token_hex(2)}-{secrets.token_hex(2)}" for _ in range(RECOVERY_CODE_COUNT)]
    user.recovery_codes = ",".join(
        hashlib.sha256(c.encode()).hexdigest() for c in codes
    )
    return codes


def consume_recovery_code(user: User, code: str) -> bool:
    if not user.recovery_codes:
        return False
    target = hashlib.sha256(code.encode()).hexdigest()
    hashes = user.recovery_codes.split(",")
    if target not in hashes:
        return False
    hashes.remove(target)
    user.recovery_codes = ",".join(hashes)
    return True


def disable(user: User) -> None:
    user.two_factor_enabled = False
    user.totp_secret = None
    user.recovery_codes = None
