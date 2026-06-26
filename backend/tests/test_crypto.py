"""Token encryption at rest (task 4.1)."""
import pytest
from cryptography.fernet import Fernet

from app.core import crypto
from app.core.config import settings


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


def test_key_rotation(monkeypatch):
    old, new = Fernet.generate_key().decode(), Fernet.generate_key().decode()
    monkeypatch.setattr(settings, "token_encryption_key", old)
    monkeypatch.setattr(settings, "token_encryption_key_old", None)
    blob = crypto.encrypt("secret")  # encrypted with the old key

    # rotate: new primary, old retained for decryption
    monkeypatch.setattr(settings, "token_encryption_key", new)
    monkeypatch.setattr(settings, "token_encryption_key_old", old)
    assert crypto.decrypt(blob) == "secret"  # still decryptable via retained old key
    fresh = crypto.encrypt("secret2")  # re-encrypted with the new primary

    monkeypatch.setattr(settings, "token_encryption_key_old", None)  # drop the old key
    assert crypto.decrypt(fresh) == "secret2"
