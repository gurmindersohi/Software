"""Token encryption at rest (task 4.1)."""
import pytest

from app.core import crypto


def test_encrypt_decrypt_roundtrip():
    secret = "EAAB-super-secret-graph-token"
    blob = crypto.encrypt(secret)
    assert blob != secret  # ciphertext differs from plaintext
    assert crypto.decrypt(blob) == secret


def test_encrypt_is_nondeterministic():
    # Fernet embeds a random IV → two encryptions differ but both decrypt back.
    a, b = crypto.encrypt("x"), crypto.encrypt("x")
    assert a != b
    assert crypto.decrypt(a) == crypto.decrypt(b) == "x"


def test_encrypt_optional_passthrough_none():
    assert crypto.encrypt_optional(None) is None
    assert crypto.decrypt(crypto.encrypt_optional("y")) == "y"


def test_decrypt_garbage_raises():
    with pytest.raises(ValueError):
        crypto.decrypt("not-a-valid-token")
