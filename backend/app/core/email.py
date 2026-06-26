"""Pluggable email sender (task 2.6).

Providers: `console` (logs — dev/tests), `resend`, `sendgrid` (both via their
HTTP APIs). The HTTP client is injectable so provider request-building is
unit-tested without network.
"""
import logging
from typing import Optional

import httpx

from app.core.config import settings

logger = logging.getLogger("app.email")


def send_email(to: str, subject: str, body: str) -> None:
    provider = settings.email_provider
    if provider == "console":
        logger.info("EMAIL → %s | %s\n%s", to, subject, body)
        return
    if provider == "resend":
        _send_resend(to, subject, body)
        return
    if provider == "sendgrid":
        _send_sendgrid(to, subject, body)
        return
    raise NotImplementedError(f"Email provider '{provider}' is not supported")


def _send_resend(
    to: str, subject: str, body: str, *, client: Optional[httpx.Client] = None
) -> None:
    if not settings.resend_api_key:
        raise RuntimeError("RESEND_API_KEY is not configured")
    http = client or httpx.Client(timeout=10.0)
    resp = http.post(
        "https://api.resend.com/emails",
        headers={"Authorization": f"Bearer {settings.resend_api_key}"},
        json={"from": settings.email_from, "to": [to], "subject": subject, "text": body},
    )
    if resp.status_code >= 400:
        raise RuntimeError(f"Resend error {resp.status_code}: {resp.text}")


def _send_sendgrid(
    to: str, subject: str, body: str, *, client: Optional[httpx.Client] = None
) -> None:
    if not settings.sendgrid_api_key:
        raise RuntimeError("SENDGRID_API_KEY is not configured")
    http = client or httpx.Client(timeout=10.0)
    resp = http.post(
        "https://api.sendgrid.com/v3/mail/send",
        headers={"Authorization": f"Bearer {settings.sendgrid_api_key}"},
        json={
            "personalizations": [{"to": [{"email": to}]}],
            "from": {"email": settings.email_from},
            "subject": subject,
            "content": [{"type": "text/plain", "value": body}],
        },
    )
    if resp.status_code >= 400:
        raise RuntimeError(f"SendGrid error {resp.status_code}: {resp.text}")
