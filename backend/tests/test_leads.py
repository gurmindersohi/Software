"""Leads CRUD + search + tenant isolation (Phase 3.3/3.7)."""
from fastapi.testclient import TestClient

from app.main import app
from tests.conftest import register_confirm_login

LEAD = {"email": "lead@example.com", "first_name": "Jane", "last_name": "Doe"}


def test_lead_crud_and_search(client, session):
    register_confirm_login(client, session)

    created = client.post("/api/v1/leads", json=LEAD)
    assert created.status_code == 201
    lead_id = created.json()["id"]

    assert len(client.get("/api/v1/leads").json()) == 1
    assert client.get(f"/api/v1/leads/{lead_id}").json()["email"] == "lead@example.com"

    updated = client.put(f"/api/v1/leads/{lead_id}", json={"city": "Calgary"})
    assert updated.json()["city"] == "Calgary"

    found = client.get("/api/v1/leads/search", params={"name": "Jane"})
    assert len(found.json()) == 1
    assert client.get("/api/v1/leads/search", params={"name": "Nobody"}).json() == []

    assert client.delete(f"/api/v1/leads/{lead_id}").status_code == 204
    assert client.get("/api/v1/leads").json() == []


def test_duplicate_lead_email_conflicts(client, session):
    register_confirm_login(client, session)
    assert client.post("/api/v1/leads", json=LEAD).status_code == 201
    assert client.post("/api/v1/leads", json=LEAD).status_code == 409


def test_leads_isolated_between_tenants(client, session):
    register_confirm_login(client, session, email="a@acme.com")
    client_b = TestClient(app)
    register_confirm_login(client_b, session, email="b@acme.com")

    client.post("/api/v1/leads", json=LEAD)

    assert len(client.get("/api/v1/leads").json()) == 1
    assert client_b.get("/api/v1/leads").json() == []  # B sees none of A's leads

    lead_id = client.get("/api/v1/leads").json()[0]["id"]
    assert client_b.get(f"/api/v1/leads/{lead_id}").status_code == 404  # no cross-tenant read
    assert client_b.delete(f"/api/v1/leads/{lead_id}").status_code == 404  # no cross-tenant delete
