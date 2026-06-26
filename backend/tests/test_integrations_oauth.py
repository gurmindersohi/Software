"""Facebook OAuth connect/callback (task 4.1) with a fake Graph client."""
from sqlmodel import select

from app.api.routes.integrations import get_graph_client
from app.core import crypto
from app.core.config import settings
from app.main import app
from app.models.social import SocialMedia
from tests.conftest import register_confirm_login


class FakeGraph:
    access_token = None

    def exchange_code_for_token(self, code, redirect_uri):
        return {"access_token": "short-token"}

    def get_long_lived_token(self, short_token):
        return {"access_token": "long-token"}

    def get_pages(self):
        return [{"id": "p1", "name": "Page 1", "access_token": "PAGE_SECRET"}]


def test_connect_503_when_unconfigured(client, session):
    register_confirm_login(client, session)
    assert client.get("/api/v1/integrations/facebook/connect").status_code == 503


def test_connect_returns_oauth_url(client, session, monkeypatch):
    monkeypatch.setattr(settings, "facebook_app_id", "APP123")
    register_confirm_login(client, session)

    resp = client.get("/api/v1/integrations/facebook/connect")
    assert resp.status_code == 200
    url = resp.json()["authorize_url"]
    assert "client_id=APP123" in url

    account_id = client.get("/api/v1/account").json()["id"]
    assert account_id in url  # tenant carried in state


def test_callback_stores_pages_with_encrypted_tokens(client, session):
    register_confirm_login(client, session)
    account_id = client.get("/api/v1/account").json()["id"]

    app.dependency_overrides[get_graph_client] = lambda: FakeGraph()
    try:
        resp = client.get(
            "/api/v1/integrations/facebook/callback",
            params={"code": "CODE", "state": account_id},
        )
    finally:
        app.dependency_overrides.pop(get_graph_client, None)

    assert resp.status_code == 200
    assert resp.json()["pages_connected"] == 1

    row = session.exec(select(SocialMedia)).first()
    assert row.type == "facebook"
    assert row.access_token != "PAGE_SECRET"  # stored encrypted
    assert crypto.decrypt(row.access_token) == "PAGE_SECRET"
