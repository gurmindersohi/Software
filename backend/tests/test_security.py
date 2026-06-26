"""Password hashing: new format + legacy ASP.NET Identity verifier (task 2.2)."""
import base64
import hashlib

from app.core import security


def _encode_aspnet_v3(password: str, salt: bytes, iterations: int, prf: int = 1) -> str:
    """Build a hash exactly as ASP.NET Core Identity's PasswordHasher (v3) would."""
    algo = {0: "sha1", 1: "sha256", 2: "sha512"}[prf]
    subkey = hashlib.pbkdf2_hmac(algo, password.encode(), salt, iterations, dklen=32)
    blob = (
        bytes([0x01])
        + prf.to_bytes(4, "big")
        + iterations.to_bytes(4, "big")
        + len(salt).to_bytes(4, "big")
        + salt
        + subkey
    )
    return base64.b64encode(blob).decode()


def _encode_aspnet_v2(password: str, salt: bytes) -> str:
    subkey = hashlib.pbkdf2_hmac("sha1", password.encode(), salt, 1000, dklen=32)
    return base64.b64encode(bytes([0x00]) + salt + subkey).decode()


def test_new_hash_roundtrip():
    h = security.hash_password("S3cret!")
    assert h.startswith("pbkdf2_sha256$")
    assert security.verify_password("S3cret!", h) == (True, False)
    assert security.verify_password("wrong", h) == (False, False)


def test_legacy_v3_sha256_verifies_and_flags_rehash():
    stored = _encode_aspnet_v3("P@ssw0rd", b"0123456789abcdef", 10000, prf=1)
    assert security.verify_password("P@ssw0rd", stored) == (True, True)
    assert security.verify_password("nope", stored)[0] is False


def test_legacy_v3_sha512():
    stored = _encode_aspnet_v3("hunter2", b"abcdefghijklmnop", 5000, prf=2)
    assert security.verify_password("hunter2", stored) == (True, True)


def test_legacy_v2():
    salt = b"sixteenbytesalt!"
    assert len(salt) == 16
    stored = _encode_aspnet_v2("letmein", salt)
    assert security.verify_password("letmein", stored) == (True, True)


def test_jwt_roundtrip():
    token = security.create_access_token("abc-123")
    payload = security.decode_token(token)
    assert payload["sub"] == "abc-123"
    assert payload["type"] == "access"
