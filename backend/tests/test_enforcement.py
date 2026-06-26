"""Trial/suspension gating (3.9) + plan quotas (3.8)."""
from datetime import datetime, timedelta, timezone

from sqlmodel import select

from app.models.account import Account
from tests.conftest import register_confirm_login

LEAD = {"email": "lead@example.com", "first_name": "Q"}


def _set_account(session, **fields):
    account = session.exec(select(Account)).first()
    for key, value in fields.items():
        setattr(account, key, value)
    session.add(account)
    session.commit()
    return account


def test_active_trial_allows_create(client, session):
    register_confirm_login(client, session)  # fresh 14-day trial
    assert client.post("/api/v1/leads", json=LEAD).status_code == 201


def test_expired_trial_blocks_create(client, session):
    register_confirm_login(client, session)
    _set_account(
        session,
        is_account_paid=False,
        trial_expiry=datetime.now(timezone.utc) - timedelta(days=1),
    )
    resp = client.post("/api/v1/leads", json=LEAD)
    assert resp.status_code == 402


def test_on_hold_blocks_create(client, session):
    register_confirm_login(client, session)
    _set_account(session, on_hold=True)
    assert client.post("/api/v1/leads", json=LEAD).status_code == 402


def test_paid_account_not_blocked_after_trial(client, session):
    register_confirm_login(client, session)
    _set_account(
        session,
        is_account_paid=True,
        trial_expiry=datetime.now(timezone.utc) - timedelta(days=30),
    )
    assert client.post("/api/v1/leads", json=LEAD).status_code == 201


def test_social_sets_quota_enforced(client, session):
    register_confirm_login(client, session)  # trial → 3 social sets
    for i in range(3):
        ok = client.post(
            "/api/v1/social",
            json={"type": "facebook", "name": f"P{i}", "page_id": str(i)},
        )
        assert ok.status_code == 201
    over = client.post(
        "/api/v1/social", json={"type": "facebook", "name": "P4", "page_id": "4"}
    )
    assert over.status_code == 402


def test_seats_quota_blocks_invite_on_trial(client, session):
    register_confirm_login(client, session)  # trial → 1 seat, owner fills it
    resp = client.post("/api/v1/team", json={"email": "member@acme.com"})
    assert resp.status_code == 402
