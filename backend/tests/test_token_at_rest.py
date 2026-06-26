"""Tokens saved via the social/ads routers are encrypted at rest (task 4.1)."""
from sqlmodel import select

from app.core import crypto
from app.models.ad_account import AdAccount
from app.models.social import SocialMedia
from tests.conftest import register_confirm_login


def test_social_token_encrypted_at_rest(client, session):
    register_confirm_login(client, session)
    client.post(
        "/api/v1/social", json={"type": "facebook", "name": "P", "access_token": "PLAINTOKEN"}
    )
    row = session.exec(select(SocialMedia)).first()
    assert row.access_token != "PLAINTOKEN"
    assert crypto.decrypt(row.access_token) == "PLAINTOKEN"


def test_ad_account_token_encrypted_at_rest(client, session):
    register_confirm_login(client, session)
    client.post("/api/v1/ad-accounts", json={"name": "Ads", "access_token": "ADTOKEN"})
    row = session.exec(select(AdAccount)).first()
    assert row.access_token != "ADTOKEN"
    assert crypto.decrypt(row.access_token) == "ADTOKEN"
