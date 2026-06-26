"""TOTP primitive (task 2.8)."""
import time

from app.core import totp


def test_rfc6238_known_vector():
    # secret = ASCII "12345678901234567890" in base32; RFC 6238 @T=59 → 287082
    secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
    assert totp.verify(secret, "287082", at=59)
    assert not totp.verify(secret, "000000", at=59)


def test_generate_and_verify_current():
    secret = totp.generate_secret()
    code = totp._hotp(secret, int(time.time()) // 30)
    assert totp.verify(secret, code)


def test_provisioning_uri():
    uri = totp.provisioning_uri("ABCSECRET", "user@example.com")
    assert uri.startswith("otpauth://totp/")
    assert "secret=ABCSECRET" in uri
    assert "issuer=Sohi" in uri
