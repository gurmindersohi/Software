"""Clients/workspaces + PDF report + client assignment (Tier 2)."""
from sqlmodel import select

from app.models.user import User
from tests.conftest import register_confirm_login


def test_client_crud(client, session):
    register_confirm_login(client, session)
    created = client.post("/api/v1/clients", json={"name": "Acme Corp"})
    assert created.status_code == 201
    cid = created.json()["id"]

    assert client.get("/api/v1/clients").json()[0]["name"] == "Acme Corp"
    client.put(f"/api/v1/clients/{cid}", json={"name": "Acme Inc"})
    assert client.get(f"/api/v1/clients/{cid}").json()["name"] == "Acme Inc"
    assert client.delete(f"/api/v1/clients/{cid}").status_code == 204
    assert client.get("/api/v1/clients").json() == []  # soft-deleted, hidden


def test_client_create_requires_owner(client, session):
    register_confirm_login(client, session)
    user = session.exec(select(User)).first()
    user.roles = []
    session.add(user)
    session.commit()
    assert client.post("/api/v1/clients", json={"name": "X"}).status_code == 403


def test_client_report_is_pdf(client, session):
    register_confirm_login(client, session)
    cid = client.post("/api/v1/clients", json={"name": "Acme"}).json()["id"]
    resp = client.get(f"/api/v1/clients/{cid}/report")
    assert resp.status_code == 200
    assert resp.headers["content-type"] == "application/pdf"
    assert resp.content[:4] == b"%PDF"


def test_lead_client_assignment_and_filter(client, session):
    register_confirm_login(client, session)
    cid = client.post("/api/v1/clients", json={"name": "Acme"}).json()["id"]
    client.post("/api/v1/leads", json={"email": "a@example.com", "client_id": cid})
    client.post("/api/v1/leads", json={"email": "b@example.com"})

    filtered = client.get("/api/v1/leads", params={"client_id": cid}).json()
    assert filtered["total"] == 1
    assert filtered["items"][0]["email"] == "a@example.com"


def test_lead_rejects_foreign_client(client, session):
    import uuid

    register_confirm_login(client, session)
    resp = client.post(
        "/api/v1/leads", json={"email": "c@example.com", "client_id": str(uuid.uuid4())}
    )
    assert resp.status_code == 404
