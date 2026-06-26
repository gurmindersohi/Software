"""Pluggable email sender (task 2.6).

Default 'console' provider logs the message — enough for local dev and tests.
Swap in Resend/SendGrid by implementing another branch + env config.
"""
import logging

from app.core.config import settings

logger = logging.getLogger("app.email")


def send_email(to: str, subject: str, body: str) -> None:
    if settings.email_provider == "console":
        logger.info("EMAIL → %s | %s\n%s", to, subject, body)
        return
    # TODO(Phase 2.6): real providers (resend / sendgrid).
    raise NotImplementedError(f"Email provider '{settings.email_provider}' not implemented")
