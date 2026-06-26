"""Persisted analytics: capture insight snapshots + time-series retrieval (Tier 3)."""
from app.api.deps import get_graph_factory
from app.main import app
from tests.conftest import register_confirm_login


class InsightGraph:
    def __init__(self, token=None):
        pass

    def get_page_insights(self, page_id, token):
        return [
            {"name": "page_impressions", "values": [{"value": 100}, {"value": 150}]},
            {"name": "page_engaged_users", "values": [{"value": 20}]},
        ]


def _override():
    def factory():
        def make(token):
            return InsightGraph(token)

        return make

    return factory


def _connect(client):
    return client.post(
        "/api/v1/social",
        json={"type": "facebook", "name": "P", "page_id": "pg1", "access_token": "T"},
    ).json()["id"]


def test_capture_and_analytics_timeseries(client, session):
    register_confirm_login(client, session)
    cid = _connect(client)

    app.dependency_overrides[get_graph_factory] = _override()
    try:
        cap = client.post(f"/api/v1/social/{cid}/capture-insights")
    finally:
        app.dependency_overrides.pop(get_graph_factory, None)
    assert cap.status_code == 200
    assert cap.json()["captured"] == 2

    series = client.get(f"/api/v1/social/{cid}/analytics").json()
    metrics = {s["metric"]: s["value"] for s in series}
    assert metrics["page_impressions"] == 150  # latest value stored
    assert metrics["page_engaged_users"] == 20

    filtered = client.get(
        f"/api/v1/social/{cid}/analytics", params={"metric": "page_impressions"}
    ).json()
    assert len(filtered) == 1
