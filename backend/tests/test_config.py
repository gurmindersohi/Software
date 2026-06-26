"""Production config fail-fast validation (design hardening)."""
import pytest
from cryptography.fernet import Fernet

from app.core.config import Settings


def test_production_rejects_insecure_defaults():
    with pytest.raises(ValueError):
        Settings(
            _env_file=None,
            environment="production",
            secret_key="dev-insecure-change-me",
            database_url="sqlite:///x.db",
            token_encryption_key=None,
        )


def test_production_accepts_secure_config():
    settings = Settings(
        _env_file=None,
        environment="production",
        debug=False,
        secret_key="a-strong-random-secret-key-0123456789-abcdef",
        database_url="postgresql+psycopg://u:p@h/db",
        token_encryption_key=Fernet.generate_key().decode(),
    )
    assert settings.environment == "production"


def test_local_allows_defaults():
    assert Settings(_env_file=None).environment == "local"
