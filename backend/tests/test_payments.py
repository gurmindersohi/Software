"""Stripe subscription creation + webhook lifecycle (tasks 4.5/4.6, overlaps 3.9)."""
from sqlmodel import select

from app.core.config import settings
from app.integrations import stripe_service
from app.models.account import Account
from tests.conftest import register_confirm_login


def test_create_subscription_updates_account(client, session, monkeypatch):
    monkeypatch.setattr(settings, "stripe_secret_key", "sk_test_x")
    monkeypatch.setattr(stripe_service, "create_customer", lambda email, name=None: "cus_123")
    monkeypatch.setattr(
        stripe_service,
        "create_subscription",
        lambda cid, pid: {"id": "sub_1", "status": "active", "client_secret": "pi_secret"},
    )
    register_confirm_login(client, session)

    resp = client.post(
        "/api/v1/payments/create-subscription", json={"price_id": "price_1", "plan": "premium"}
    )
    assert resp.status_code == 200
    assert resp.json() == {
        "subscription_id": "sub_1",
        "status": "active",
        "client_secret": "pi_secret",
    }

    account = session.exec(select(Account)).first()
    assert account.customer_id == "cus_123"
    assert account.subscription_id == "sub_1"
    assert account.is_account_paid is True


def test_create_subscription_503_when_unconfigured(client, session):
    register_confirm_login(client, session)
    assert (
        client.post("/api/v1/payments/create-subscription", json={"price_id": "p"}).status_code
        == 503
    )


def test_webhook_subscription_updated_marks_paid(client, session, monkeypatch):
    register_confirm_login(client, session)
    account = session.exec(select(Account)).first()
    account.customer_id = "cus_42"
    session.add(account)
    session.commit()

    event = {
        "type": "customer.subscription.updated",
        "data": {"object": {"customer": "cus_42", "id": "sub_9", "status": "active"}},
    }
    monkeypatch.setattr(stripe_service, "construct_event", lambda payload, sig: event)

    resp = client.post(
        "/api/v1/payments/webhook", content=b"{}", headers={"stripe-signature": "x"}
    )
    assert resp.status_code == 200
    session.refresh(account)
    assert account.subscription_id == "sub_9"
    assert account.is_account_paid is True
    assert account.on_hold is False


def test_webhook_payment_failed_puts_account_on_hold(client, session, monkeypatch):
    register_confirm_login(client, session)
    account = session.exec(select(Account)).first()
    account.customer_id = "cus_7"
    session.add(account)
    session.commit()

    event = {"type": "invoice.payment_failed", "data": {"object": {"customer": "cus_7"}}}
    monkeypatch.setattr(stripe_service, "construct_event", lambda payload, sig: event)

    client.post("/api/v1/payments/webhook", content=b"{}", headers={"stripe-signature": "x"})
    session.refresh(account)
    assert account.on_hold is True
