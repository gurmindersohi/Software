"""Scheduled-post (Queue) API: CRUD + tenant scoping (task 5.2)."""
import uuid
from datetime import datetime, timedelta, timezone

from tests.conftest import register_confirm_login


def _make_connection(client):
    resp = client.post(
        "/api/v1/social",
        json={"type": "facebook", "name": "Page", "access_token": "TKN", "page_id": "p1"},
    )
    return resp.json()["id"]


def _future():
    return (datetime.now(timezone.utc) + timedelta(hours=1)).isoformat()


def test_requires_auth(client):
    assert client.get("/api/v1/scheduled-posts").status_code == 401


def test_create_and_list(client, session):
    register_confirm_login(client, session)
    conn_id = _make_connection(client)
    resp = client.post(
        "/api/v1/scheduled-posts",
        json={
            "social_media_id": conn_id,
            "platform": "facebook",
            "message": "Hi",
            "scheduled_at": _future(),
        },
    )
    assert resp.status_code == 201
    assert resp.json()["status"] == "pending"
    assert len(client.get("/api/v1/scheduled-posts").json()) == 1


def test_create_rejects_foreign_connection(client, session):
    register_confirm_login(client, session)
    resp = client.post(
        "/api/v1/scheduled-posts",
        json={"social_media_id": str(uuid.uuid4()), "scheduled_at": _future()},
    )
    assert resp.status_code == 404


def test_cancel_pending(client, session):
    register_confirm_login(client, session)
    conn_id = _make_connection(client)
    post_id = client.post(
        "/api/v1/scheduled-posts",
        json={"social_media_id": conn_id, "scheduled_at": _future()},
    ).json()["id"]
    assert client.delete(f"/api/v1/scheduled-posts/{post_id}").status_code == 204
    assert client.get("/api/v1/scheduled-posts").json() == []
