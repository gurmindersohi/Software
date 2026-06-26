"""2FA setup/enable + two-step login + recovery + disable (task 2.8)."""
import time

from app.core import totp
from tests.conftest import register_confirm_login

EMAIL = "user@acme.com"
PASSWORD = "S3cret!"


def _code(secret: str) -> str:
    return totp._hotp(secret, int(time.time()) // 30)


def _setup_and_enable(client):
    secret = client.post("/api/v1/auth/2fa/setup").json()["secret"]
    resp = client.post("/api/v1/auth/2fa/enable", json={"code": _code(secret)})
    assert resp.status_code == 200
    return secret, resp.json()["recovery_codes"]


def test_setup_enable_then_login_requires_2fa(client, session):
    register_confirm_login(client, session)
    secret, recovery = _setup_and_enable(client)
    assert len(recovery) == 10

    client.post("/api/v1/auth/logout")
    login = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert login.status_code == 200
    assert login.json()["two_factor_required"] is True

    verify = client.post("/api/v1/auth/2fa/verify", json={"code": _code(secret)})
    assert verify.status_code == 200
    assert client.get("/api/v1/auth/me").status_code == 200


def test_wrong_code_rejected(client, session):
    register_confirm_login(client, session)
    _setup_and_enable(client)
    client.post("/api/v1/auth/logout")
    client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert client.post("/api/v1/auth/2fa/verify", json={"code": "000000"}).status_code == 401


def test_recovery_code_single_use(client, session):
    register_confirm_login(client, session)
    _, recovery = _setup_and_enable(client)

    client.post("/api/v1/auth/logout")
    client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert client.post("/api/v1/auth/2fa/verify", json={"code": recovery[0]}).status_code == 200

    client.post("/api/v1/auth/logout")
    client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    # same recovery code can't be reused
    assert client.post("/api/v1/auth/2fa/verify", json={"code": recovery[0]}).status_code == 401


def test_disable_removes_2fa(client, session):
    register_confirm_login(client, session)
    secret, _ = _setup_and_enable(client)
    assert client.post("/api/v1/auth/2fa/disable", json={"code": _code(secret)}).status_code == 200

    client.post("/api/v1/auth/logout")
    login = client.post("/api/v1/auth/login", json={"email": EMAIL, "password": PASSWORD})
    assert login.json()["two_factor_required"] is False
