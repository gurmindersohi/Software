"""Audit log, admin back-office, and sign-out-everywhere (Tier 4)."""
from sqlmodel import select

from app.models.user import User
from tests.conftest import register_confirm_login


def _make_superuser(session):
    user = session.exec(select(User)).first()
    user.is_superuser = True
    session.add(user)
    session.commit()


def test_audit_records_login_and_client(client, session):
    register_confirm_login(client, session)  # login is audited
    client.post("/api/v1/clients", json={"name": "Acme"})
    actions = [r["action"] for r in client.get("/api/v1/audit-logs").json()["items"]]
    assert "auth.login" in actions
    assert "client.created" in actions


def test_audit_requires_owner(client, session):
    register_confirm_login(client, session)
    user = session.exec(select(User)).first()
    user.roles = []
    session.add(user)
    session.commit()
    assert client.get("/api/v1/audit-logs").status_code == 403


def test_admin_requires_superuser(client, session):
    register_confirm_login(client, session)
    assert client.get("/api/v1/admin/accounts").status_code == 403


def test_admin_accounts_suspend_and_metrics(client, session):
    register_confirm_login(client, session)
    _make_superuser(session)

    accounts = client.get("/api/v1/admin/accounts")
    assert accounts.status_code == 200
    account_id = accounts.json()[0]["id"]

    assert client.post(f"/api/v1/admin/accounts/{account_id}/suspend").json()["on_hold"] is True
    assert client.post(f"/api/v1/admin/accounts/{account_id}/unsuspend").json()["on_hold"] is False

    metrics = client.get("/api/v1/admin/metrics").json()
    assert metrics["accounts"] >= 1
    assert "users" in metrics


def test_sign_out_everywhere_invalidates_old_tokens(client, session):
    register_confirm_login(client, session)
    old_token = client.cookies.get("access_token")
    assert client.get("/api/v1/auth/me").status_code == 200

    assert client.post("/api/v1/auth/sign-out-everywhere").status_code == 200
    # the previously issued token now carries a stale stamp → rejected
    resp = client.get("/api/v1/auth/me", headers={"Authorization": f"Bearer {old_token}"})
    assert resp.status_code == 401
