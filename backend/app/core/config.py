"""Application settings, loaded from environment / .env (pydantic-settings)."""
from typing import Optional

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env", env_file_encoding="utf-8", extra="ignore"
    )

    # App
    app_name: str = "Sohi API"
    environment: str = "local"
    debug: bool = True
    secret_key: str = "dev-insecure-change-me-0123456789-please-override-in-env"

    # Database — default to local SQLite so tests/dev run without Postgres.
    database_url: str = "sqlite:///./sohi_dev.db"

    # Redis / object storage
    redis_url: str = "redis://localhost:6379/0"
    storage_backend: str = "local"  # local | s3
    local_storage_dir: str = "./_media"
    s3_endpoint_url: Optional[str] = None
    s3_bucket: str = "sohi-media"
    s3_access_key: Optional[str] = None
    s3_secret_key: Optional[str] = None

    # Token encryption at rest (task 4.1). If unset, derived from secret_key.
    token_encryption_key: Optional[str] = None

    # Auth
    jwt_algorithm: str = "HS256"
    access_token_expire_minutes: int = 15
    refresh_token_expire_days: int = 30

    # CORS
    frontend_origin: str = "http://localhost:3000"

    # Email (Phase 2.6)
    email_provider: str = "console"  # console | resend | sendgrid
    email_from: str = "no-reply@sohi.app"

    # Integrations (filled in Phase 4)
    facebook_app_id: Optional[str] = None
    facebook_app_secret: Optional[str] = None
    facebook_graph_version: str = "v19.0"
    facebook_redirect_url: str = "http://localhost:8000/api/v1/integrations/facebook/callback"
    facebook_scopes: str = (
        "public_profile,email,pages_show_list,pages_read_engagement,"
        "pages_manage_posts,instagram_basic,instagram_content_publish,"
        "ads_management,ads_read,leads_retrieval"
    )
    stripe_secret_key: Optional[str] = None
    stripe_webhook_secret: Optional[str] = None


settings = Settings()
