"""Email providers (task 2.6) — request construction via mock transport."""
import httpx

from app.core import email
from app.core.config import settings


def test_console_provider_is_noop(monkeypatch):
    monkeypatch.setattr(settings, "email_provider", "console")
    email.send_email("a@example.com", "Hi", "Body")  # should not raise


def test_resend_builds_request(monkeypatch):
    monkeypatch.setattr(settings, "resend_api_key", "re_test")
    captured = {}

    def handler(request):
        captured["url"] = str(request.url)
        captured["auth"] = request.headers.get("authorization")
        captured["body"] = request.content.decode()
        return httpx.Response(200, json={"id": "email_1"})

    client = httpx.Client(transport=httpx.MockTransport(handler))
    email._send_resend("to@example.com", "Subject", "Hello", client=client)

    assert captured["url"] == "https://api.resend.com/emails"
    assert captured["auth"] == "Bearer re_test"
    assert "to@example.com" in captured["body"]


def test_resend_raises_on_error(monkeypatch):
    monkeypatch.setattr(settings, "resend_api_key", "re_test")
    client = httpx.Client(transport=httpx.MockTransport(lambda r: httpx.Response(422, json={})))
    try:
        email._send_resend("to@example.com", "S", "B", client=client)
        raise AssertionError("expected error")
    except RuntimeError as exc:
        assert "Resend error 422" in str(exc)
