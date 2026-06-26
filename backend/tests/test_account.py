"""Account/Settings + Plan routes (Phase 3.1/3.2)."""
from decimal import Decimal

from app.models.plan import Plan
from tests.conftest import register_confirm_login


def test_account_requires_auth(client):
    assert client.get("/api/v1/account").status_code == 401


def test_get_and_update_account(client, session):
    register_confirm_login(client, session)
    got = client.get("/api/v1/account")
    assert got.status_code == 200
    assert got.json()["account_name"] == "Acme"

    updated = client.put("/api/v1/account", json={"city": "Toronto", "phone": "555-1234"})
    assert updated.status_code == 200
    assert updated.json()["city"] == "Toronto"
    assert updated.json()["phone"] == "555-1234"


def test_get_plan_by_name(client, session):
    register_confirm_login(client, session)
    session.add(
        Plan(name="Premium", price=Decimal("99.00"), tax=Decimal("0"), total=Decimal("99.00"))
    )
    session.commit()

    resp = client.get("/api/v1/plans/Premium")
    assert resp.status_code == 200
    assert resp.json()["name"] == "Premium"
    assert float(resp.json()["price"]) == 99.0
    assert client.get("/api/v1/plans/DoesNotExist").status_code == 404
