"""Team management + roles (task 8.4)."""
from uuid import UUID

from sqlmodel import select

from app.models.user import User
from tests.conftest import register_confirm_login


def test_list_team_includes_owner(client, session):
    register_confirm_login(client, session)
    resp = client.get("/api/v1/team")
    assert resp.status_code == 200
    assert len(resp.json()) == 1
    assert resp.json()[0]["roles"] == ["Owner"]


def test_invite_member(client, session):
    register_confirm_login(client, session)
    resp = client.post(
        "/api/v1/team", json={"email": "member@acme.com", "first_name": "Mem", "role": "User"}
    )
    assert resp.status_code == 201
    assert resp.json()["roles"] == ["User"]
    assert resp.json()["email_confirmed"] is False
    assert len(client.get("/api/v1/team").json()) == 2


def test_invite_requires_owner(client, session):
    register_confirm_login(client, session)
    user = session.exec(select(User).where(User.email == "user@acme.com")).first()
    user.roles = []  # strip Owner
    session.add(user)
    session.commit()
    resp = client.post("/api/v1/team", json={"email": "x@acme.com"})
    assert resp.status_code == 403


def test_roles_list_and_create(client, session):
    register_confirm_login(client, session)
    assert client.get("/api/v1/roles").status_code == 200
    assert client.post("/api/v1/roles", json={"name": "Manager"}).status_code == 201
    names = [r["name"] for r in client.get("/api/v1/roles").json()]
    assert "Manager" in names


def test_cannot_remove_self(client, session):
    register_confirm_login(client, session)
    me = client.get("/api/v1/auth/me").json()
    assert client.delete(f"/api/v1/team/{me['id']}").status_code == 400


def test_remove_member_soft_deletes(client, session):
    register_confirm_login(client, session)
    member_id = client.post("/api/v1/team", json={"email": "member@acme.com"}).json()["id"]
    assert client.delete(f"/api/v1/team/{member_id}").status_code == 204
    member = session.get(User, UUID(member_id))
    assert member.is_deleted is True
