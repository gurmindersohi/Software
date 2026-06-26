"""List pagination + filtering (Tier 1)."""
from tests.conftest import register_confirm_login


def test_leads_pagination_and_total(client, session):
    register_confirm_login(client, session)
    for i in range(7):
        client.post("/api/v1/leads", json={"email": f"lead{i}@example.com", "first_name": "L"})

    first = client.get("/api/v1/leads", params={"limit": 3, "offset": 0}).json()
    assert first["total"] == 7
    assert len(first["items"]) == 3
    assert first["limit"] == 3 and first["offset"] == 0

    last = client.get("/api/v1/leads", params={"limit": 3, "offset": 6}).json()
    assert len(last["items"]) == 1  # 7th lead
    assert last["total"] == 7


def test_leads_source_filter(client, session):
    register_confirm_login(client, session)
    client.post("/api/v1/leads", json={"email": "a@example.com", "lead_source": "web"})
    client.post("/api/v1/leads", json={"email": "b@example.com", "lead_source": "facebook"})

    web = client.get("/api/v1/leads", params={"source": "web"}).json()
    assert web["total"] == 1
    assert web["items"][0]["email"] == "a@example.com"


def test_limit_is_capped(client, session):
    register_confirm_login(client, session)
    # limit above the cap (200) is rejected by validation
    assert client.get("/api/v1/leads", params={"limit": 9999}).status_code == 422
