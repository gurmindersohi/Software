"""Change email + GDPR export/delete (gaps #5/#6)."""
from fastapi.testclient import TestClient

from app.main import app
from tests.conftest import register_confirm_login

EMAIL = "user@acme.com"
PASSWORD = "S3cret!"


def test_change_email(client, session):
    register_confirm_login(client, session)
    resp = client.post(
        "/api/v1/auth/change-email", json={"new_email": "new@acme.com", "password": PASSWORD}
    )
    assert resp.status_code == 200
    assert resp.json()["email"] == "new@acme.com"
    assert resp.json()["email_confirmed"] is False  # must re-confirm


def test_change_email_wrong_password(client, session):
    register_confirm_login(client, session)
    resp = client.post(
        "/api/v1/auth/change-email", json={"new_email": "x@acme.com", "password": "WRONG"}
    )
    assert resp.status_code == 400


def test_change_email_conflict(client, session):
    register_confirm_login(client, session)
    other = TestClient(app)
    register_confirm_login(other, session, email="taken@acme.com")
    resp = client.post(
        "/api/v1/auth/change-email", json={"new_email": "taken@acme.com", "password": PASSWORD}
    )
    assert resp.status_code == 409


def test_export_personal_data(client, session):
    register_confirm_login(client, session)
    resp = client.get("/api/v1/auth/personal-data")
    assert resp.status_code == 200
    assert resp.json()["email"] == EMAIL
    assert "roles" in resp.json()


def test_delete_account_scrubs_and_blocks_login(client, session):
    register_confirm_login(client, session)
    assert (
        client.post("/api/v1/auth/delete-account", json={"password": "WRONG"}).status_code == 400
    )
    assert (
        client.post("/api/v1/auth/delete-account", json={"password": PASSWORD}).status_code == 200
    )
    relogin = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert relogin.status_code == 401  # soft-deleted users can't authenticate
