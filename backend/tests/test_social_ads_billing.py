"""Social tokens, ad accounts, billing (Phase 3.4/3.5/3.6)."""
from tests.conftest import register_confirm_login


def test_social_token_scoped_and_never_leaked(client, session):
    register_confirm_login(client, session)
    created = client.post(
        "/api/v1/social",
        json={"type": "facebook", "name": "Page", "access_token": "SECRET", "page_id": "1"},
    )
    assert created.status_code == 201
    assert "access_token" not in created.json()  # token must never be returned
    assert "secret" not in created.json()

    by_platform = client.get("/api/v1/social/facebook")
    assert by_platform.status_code == 200
    assert "access_token" not in by_platform.json()

    assert len(client.get("/api/v1/social").json()) == 1
    assert client.get("/api/v1/social/instagram").status_code == 404

    token_id = created.json()["id"]
    assert client.delete(f"/api/v1/social/{token_id}").status_code == 204
    assert client.get("/api/v1/social").json() == []


def test_ad_accounts(client, session):
    register_confirm_login(client, session)
    created = client.post(
        "/api/v1/ad-accounts",
        json={"name": "Ads", "user_account_id": "act_1", "access_token": "X"},
    )
    assert created.status_code == 201
    assert "access_token" not in created.json()
    assert len(client.get("/api/v1/ad-accounts").json()) == 1


def test_billing_get_and_update(client, session):
    register_confirm_login(client, session)
    assert client.get("/api/v1/billing").json()["is_account_paid"] is False

    updated = client.put(
        "/api/v1/billing",
        json={"customer_id": "cus_1", "subscription_id": "sub_1", "is_account_paid": True},
    )
    assert updated.status_code == 200
    assert updated.json()["is_account_paid"] is True
    assert updated.json()["customer_id"] == "cus_1"
