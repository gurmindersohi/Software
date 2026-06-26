"""Auth endpoint flow + legacy-hash login/rehash (the no-reset migration)."""
import base64
import hashlib

from sqlmodel import select

from app.models.account import Account
from app.models.user import User

EMAIL = "owner@acme.com"
PASSWORD = "S3cret!"


def _register(client, email=EMAIL, password=PASSWORD):
    return client.post(
        "/api/v1/auth/register",
        json={"email": email, "password": password, "first_name": "Ow", "account_name": "Acme"},
    )


def _confirm(client, session, email=EMAIL):
    user = session.exec(select(User).where(User.email == email)).first()
    user.email_confirmed = True
    session.add(user)
    session.commit()
    return user


def test_register_creates_user_account_and_role(client, session):
    resp = _register(client)
    assert resp.status_code == 201
    body = resp.json()
    assert body["email"] == EMAIL
    assert body["email_confirmed"] is False
    assert body["roles"] == ["Owner"]
    assert body["account_id"]
    assert session.exec(select(Account)).first() is not None


def test_duplicate_register_conflicts(client):
    _register(client)
    assert _register(client).status_code == 409


def test_login_blocked_until_confirmed(client):
    _register(client)
    resp = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert resp.status_code == 403


def test_me_requires_auth(client):
    assert client.get("/api/v1/auth/me").status_code == 401


def test_full_login_flow_sets_cookie_and_me_works(client, session):
    _register(client)
    _confirm(client, session)
    resp = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert resp.status_code == 200
    assert "access_token" in client.cookies

    me = client.get("/api/v1/auth/me")
    assert me.status_code == 200
    assert me.json()["email"] == EMAIL


def test_wrong_password_rejected(client, session):
    _register(client)
    _confirm(client, session)
    resp = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": "WRONG"})
    assert resp.status_code == 401


def test_change_password_then_relogin(client, session):
    _register(client)
    _confirm(client, session)
    client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    rc = client.post(
        "/api/v1/auth/change-password",
        json={"current_password": PASSWORD, "new_password": "N3wPass!"},
    )
    assert rc.status_code == 200
    client.post("/api/v1/auth/logout")
    assert client.post(
        "/api/v1/auth/login", json={"email": EMAIL, "password": "N3wPass!"}
    ).status_code == 200


def test_legacy_aspnet_hash_login_and_transparent_rehash(client, session):
    """A user imported with an ASP.NET Identity hash logs in with no reset,
    and their hash is silently upgraded to the new format."""
    salt = b"0123456789abcdef"
    subkey = hashlib.pbkdf2_hmac("sha256", b"Legacy1!", salt, 10000, dklen=32)
    legacy = base64.b64encode(
        bytes([0x01])
        + (1).to_bytes(4, "big")
        + (10000).to_bytes(4, "big")
        + (16).to_bytes(4, "big")
        + salt
        + subkey
    ).decode()

    acc = Account(account_name="Legacy")
    session.add(acc)
    session.flush()
    user = User(
        email="legacy@acme.com",
        normalized_email="LEGACY@ACME.COM",
        password_hash=legacy,
        account_id=acc.id,
        email_confirmed=True,
    )
    session.add(user)
    session.commit()

    resp = client.post(
        "/api/v1/auth/login", json={"email": "legacy@acme.com", "password": "Legacy1!"}
    )
    assert resp.status_code == 200

    session.refresh(user)
    assert user.password_hash.startswith("pbkdf2_sha256$")  # upgraded
    assert not user.password_hash == legacy
