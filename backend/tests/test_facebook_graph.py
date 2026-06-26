"""Graph client request construction + parsing (tasks 4.2/4.3/4.4), via a
mock HTTP transport — no live Facebook calls."""
import httpx
import pytest

from app.integrations.facebook.graph import GraphClient, GraphError


def _client(handler, **kw):
    transport = httpx.MockTransport(handler)
    return GraphClient(client=httpx.Client(transport=transport), **kw)


def test_get_ad_accounts_parses_data_and_sends_token():
    def handler(request):
        assert request.url.path.endswith("/me/adaccounts")
        assert request.url.params["access_token"] == "TOK"
        return httpx.Response(200, json={"data": [{"id": "act_1", "name": "A"}]})

    accounts = _client(handler, access_token="TOK").get_ad_accounts()
    assert accounts == [{"id": "act_1", "name": "A"}]


def test_create_campaign_posts_form_data():
    def handler(request):
        assert request.method == "POST"
        assert request.url.path.endswith("/act_1/campaigns")
        assert "objective=LINK_CLICKS" in request.content.decode()
        return httpx.Response(200, json={"id": "camp_1"})

    res = _client(handler, access_token="TOK").create_campaign("act_1", "Test", "LINK_CLICKS")
    assert res["id"] == "camp_1"


def test_version_is_pinned_in_url():
    captured = {}

    def handler(request):
        captured["url"] = str(request.url)
        return httpx.Response(200, json={"data": []})

    _client(handler, access_token="T", version="v19.0").get_pages()
    assert "/v19.0/me/accounts" in captured["url"]


def test_oauth_exchange_omits_our_access_token():
    def handler(request):
        assert "access_token" not in request.url.params
        assert request.url.params["code"] == "CODE"
        return httpx.Response(200, json={"access_token": "USER_TOKEN"})

    res = _client(handler).exchange_code_for_token("CODE", "http://cb")
    assert res["access_token"] == "USER_TOKEN"


def test_instagram_account_id_extracted():
    def handler(request):
        return httpx.Response(200, json={"instagram_business_account": {"id": "ig_1"}})

    assert _client(handler, access_token="T").get_instagram_account("page", "ptok") == "ig_1"


def test_error_payload_raises_graph_error():
    def handler(request):
        return httpx.Response(400, json={"error": {"message": "Invalid OAuth token"}})

    with pytest.raises(GraphError, match="Invalid OAuth token"):
        _client(handler, access_token="bad").get_me()
