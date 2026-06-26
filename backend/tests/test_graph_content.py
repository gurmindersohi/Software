"""Graph-proxy endpoints (social posts/insights/create, ad campaigns) — mock Graph."""
from fastapi.testclient import TestClient

from app.api.deps import get_graph_factory
from app.main import app
from tests.conftest import register_confirm_login


class FakeGraph:
    def __init__(self, token=None):
        self.token = token

    def get_page_posts(self, page_id, token):
        return [{"id": "post_1", "message": "hi"}]

    def get_page_insights(self, page_id, token):
        return [{"name": "page_impressions", "values": []}]

    def create_post(self, page_id, token, message, link=None):
        return {"id": "new_post", "message": message}

    def create_photo_post(self, page_id, token, image_url, message=""):
        return {"id": "photo_post", "image": image_url}

    def create_video_post(self, page_id, token, video_url, message=""):
        return {"id": "video_post", "video": video_url}

    def get_instagram_insights(self, ig_user_id, token):
        return [{"name": "reach", "values": [{"value": 42}]}]

    def get_lead_forms(self, page_id, token):
        return [{"id": "form_1"}]

    def get_leads(self, form_id, token):
        return [
            {
                "field_data": [
                    {"name": "email", "values": ["lead@example.com"]},
                    {"name": "full_name", "values": ["Jane Doe"]},
                    {"name": "phone_number", "values": ["555-0100"]},
                ]
            }
        ]

    def upload_ad_image(self, ad_account_id, image_url):
        return "img_hash_123"

    def get_campaigns(self, act_id):
        return [{"id": "camp_1", "name": "C"}]

    def create_campaign(self, act_id, name, objective, status="PAUSED"):
        return {"id": "camp_new", "name": name}

    def get_adsets(self, act_id):
        return [{"id": "as_1"}]

    def get_ads(self, act_id):
        return [{"id": "ad_1"}]

    def create_adset(self, act_id, **kwargs):
        return {"id": "adset_new"}

    def create_ad(self, act_id, **kwargs):
        return {"id": "ad_new"}

    def search_targeting(self, q):
        return [{"id": "int_1", "name": q}]

    def search_locations(self, q):
        return [{"key": "loc_1", "name": q}]


def _override(captured=None):
    def factory():
        def make(token):
            if captured is not None:
                captured["token"] = token
            return FakeGraph(token)

        return make

    return factory


def _connect_social(client, token="PAGETOKEN"):
    return client.post(
        "/api/v1/social",
        json={"type": "facebook", "name": "Page", "page_id": "pg1", "access_token": token},
    ).json()["id"]


def _connect_ad_account(client, token="ADTOK"):
    return client.post(
        "/api/v1/ad-accounts",
        json={"name": "Ads", "user_account_id": "act_1", "access_token": token},
    ).json()["id"]


def test_page_posts_passes_decrypted_token(client, session):
    register_confirm_login(client, session)
    cid = _connect_social(client)
    captured = {}
    app.dependency_overrides[get_graph_factory] = _override(captured)
    try:
        resp = client.get(f"/api/v1/social/{cid}/posts")
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 200
    assert resp.json()[0]["id"] == "post_1"
    assert captured["token"] == "PAGETOKEN"  # decrypted before reaching Graph


def test_create_page_post(client, session):
    register_confirm_login(client, session)
    cid = _connect_social(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        resp = client.post(f"/api/v1/social/{cid}/posts", json={"message": "Hello"})
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 201
    assert resp.json()["id"] == "new_post"


def test_photo_and_video_posts(client, session):
    register_confirm_login(client, session)
    cid = _connect_social(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        photo = client.post(f"/api/v1/social/{cid}/posts", json={"image_url": "http://img/x.jpg"})
        video = client.post(f"/api/v1/social/{cid}/posts", json={"video_url": "http://v/x.mp4"})
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert photo.json()["id"] == "photo_post"
    assert video.json()["id"] == "video_post"


def test_instagram_insights(client, session):
    register_confirm_login(client, session)
    cid = _connect_social(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        resp = client.get(f"/api/v1/social/{cid}/instagram-insights")
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 200
    assert resp.json()[0]["name"] == "reach"


def test_sync_lead_forms_imports_leads(client, session):
    register_confirm_login(client, session)
    cid = _connect_social(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        resp = client.post(f"/api/v1/social/{cid}/sync-leads")
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 200
    assert resp.json()["imported"] == 1
    leads = client.get("/api/v1/leads").json()["items"]
    assert any(le["email"] == "lead@example.com" for le in leads)


def test_create_ad_with_image_uploads_hash(client, session):
    register_confirm_login(client, session)
    aid = _connect_ad_account(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        resp = client.post(
            f"/api/v1/ad-accounts/{aid}/ads",
            json={
                "name": "Ad",
                "adset_id": "as_1",
                "page_id": "pg1",
                "message": "hi",
                "link": "http://x",
                "image_url": "http://img/x.jpg",
            },
        )
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 201
    assert resp.json()["id"] == "ad_new"


def test_page_posts_cross_tenant_404(client, session):
    register_confirm_login(client, session, email="a@acme.com")
    cid = _connect_social(client)
    other = TestClient(app)
    register_confirm_login(other, session, email="b@acme.com")
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        resp = other.get(f"/api/v1/social/{cid}/posts")
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert resp.status_code == 404


def test_ad_campaigns_list_and_create(client, session):
    register_confirm_login(client, session)
    aid = _connect_ad_account(client)
    captured = {}
    app.dependency_overrides[get_graph_factory] = _override(captured)
    try:
        listed = client.get(f"/api/v1/ad-accounts/{aid}/campaigns")
        created = client.post(
            f"/api/v1/ad-accounts/{aid}/campaigns",
            json={"name": "New", "objective": "LINK_CLICKS"},
        )
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert listed.status_code == 200
    assert listed.json()[0]["id"] == "camp_1"
    assert created.status_code == 201
    assert created.json()["id"] == "camp_new"
    assert captured["token"] == "ADTOK"


def test_create_adset_ad_and_search(client, session):
    register_confirm_login(client, session)
    aid = _connect_ad_account(client)
    app.dependency_overrides[get_graph_factory] = _override()
    try:
        adset = client.post(
            f"/api/v1/ad-accounts/{aid}/adsets",
            json={"name": "AS", "campaign_id": "camp_1", "daily_budget": 500},
        )
        ad = client.post(
            f"/api/v1/ad-accounts/{aid}/ads",
            json={
                "name": "Ad",
                "adset_id": "adset_new",
                "page_id": "pg1",
                "message": "hi",
                "link": "http://x",
            },
        )
        targeting = client.get(
            f"/api/v1/ad-accounts/{aid}/targeting-search", params={"q": "yoga"}
        )
        location = client.get(
            f"/api/v1/ad-accounts/{aid}/location-search", params={"q": "Toronto"}
        )
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert adset.status_code == 201 and adset.json()["id"] == "adset_new"
    assert ad.status_code == 201 and ad.json()["id"] == "ad_new"
    assert targeting.json()[0]["name"] == "yoga"
    assert location.json()[0]["name"] == "Toronto"
