"""Meta Graph API client (tasks 4.2/4.3/4.4/4.8).

Replaces the old Blazor `SocialService`/`AdAccountService` logic that called
Graph **v10.0** (deprecated). Version is pinned via settings (default v19.0).

All HTTP goes through a single `_request` helper and an injectable
`httpx.Client`, so request construction and response parsing are unit-tested
with a mock transport (no live calls). Methods that hit the live API are thin.
"""
from typing import List, Optional

import httpx

from app.core.config import settings

GRAPH_HOST = "https://graph.facebook.com"


class GraphError(Exception):
    """Raised when Graph returns an error payload or non-2xx status."""


class GraphClient:
    def __init__(
        self,
        access_token: Optional[str] = None,
        *,
        version: Optional[str] = None,
        client: Optional[httpx.Client] = None,
    ) -> None:
        self.version = version or settings.facebook_graph_version
        self.base_url = f"{GRAPH_HOST}/{self.version}"
        self.access_token = access_token
        self._client = client or httpx.Client(timeout=30.0)

    # --- core -------------------------------------------------------------
    def _request(self, method, path, *, params=None, data=None, token=None):
        query = dict(params or {})
        effective = token if token is not None else self.access_token
        if effective:
            query.setdefault("access_token", effective)
        resp = self._client.request(method, f"{self.base_url}{path}", params=query, data=data)
        try:
            payload = resp.json()
        except ValueError as exc:
            raise GraphError(f"Non-JSON response ({resp.status_code})") from exc
        if resp.status_code >= 400 or (isinstance(payload, dict) and "error" in payload):
            err = payload.get("error", {}) if isinstance(payload, dict) else {}
            raise GraphError(err.get("message", f"Graph error {resp.status_code}"))
        return payload

    @staticmethod
    def _data(payload) -> List[dict]:
        return payload.get("data", []) if isinstance(payload, dict) else []

    # --- OAuth ------------------------------------------------------------
    def exchange_code_for_token(self, code: str, redirect_uri: str) -> dict:
        return self._request(
            "GET",
            "/oauth/access_token",
            params={
                "client_id": settings.facebook_app_id,
                "client_secret": settings.facebook_app_secret,
                "redirect_uri": redirect_uri,
                "code": code,
            },
            token=None,
        )

    def get_long_lived_token(self, short_token: str) -> dict:
        return self._request(
            "GET",
            "/oauth/access_token",
            params={
                "grant_type": "fb_exchange_token",
                "client_id": settings.facebook_app_id,
                "client_secret": settings.facebook_app_secret,
                "fb_exchange_token": short_token,
            },
            token=None,
        )

    # --- identity / pages -------------------------------------------------
    def get_me(self, fields: str = "id,name,email") -> dict:
        return self._request("GET", "/me", params={"fields": fields})

    def get_pages(self) -> List[dict]:
        return self._data(
            self._request("GET", "/me/accounts", params={"fields": "id,name,access_token,picture"})
        )

    # --- ads --------------------------------------------------------------
    def get_ad_accounts(self) -> List[dict]:
        return self._data(
            self._request("GET", "/me/adaccounts", params={"fields": "id,name", "limit": 100})
        )

    def get_campaigns(self, ad_account_id: str) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                f"/{ad_account_id}/campaigns",
                params={"fields": "id,name,objective,status,start_time"},
            )
        )

    def create_campaign(
        self, ad_account_id: str, name: str, objective: str, status: str = "PAUSED"
    ) -> dict:
        return self._request(
            "POST",
            f"/{ad_account_id}/campaigns",
            data={
                "name": name,
                "objective": objective,
                "status": status,
                "special_ad_categories": "[]",
            },
        )

    def get_adsets(self, ad_account_id: str) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                f"/{ad_account_id}/adsets",
                params={"fields": "id,name,status,daily_budget,campaign_id"},
            )
        )

    def get_ads(self, ad_account_id: str) -> List[dict]:
        return self._data(
            self._request("GET", f"/{ad_account_id}/ads", params={"fields": "id,name,status"})
        )

    def search_targeting(self, query: str, type_: str = "adinterest") -> List[dict]:
        return self._data(
            self._request("GET", "/search", params={"type": type_, "q": query, "limit": 25})
        )

    def search_locations(self, query: str) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                "/search",
                params={"type": "adgeolocation", "q": query, "location_types": '["city"]'},
            )
        )

    # --- social posts / insights -----------------------------------------
    def get_page_posts(self, page_id: str, page_token: str) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                f"/{page_id}/posts",
                params={"fields": "id,message,created_time,full_picture"},
                token=page_token,
            )
        )

    def create_post(
        self, page_id: str, page_token: str, message: str, link: Optional[str] = None
    ) -> dict:
        data = {"message": message}
        if link:
            data["link"] = link
        return self._request("POST", f"/{page_id}/feed", data=data, token=page_token)

    def get_post_insights(
        self, post_id: str, page_token: str, metrics: str = "post_impressions,post_clicks"
    ) -> List[dict]:
        return self._data(
            self._request(
                "GET", f"/{post_id}/insights", params={"metric": metrics}, token=page_token
            )
        )

    def get_page_insights(
        self,
        page_id: str,
        page_token: str,
        metrics: str = "page_impressions,page_engaged_users",
        period: str = "day",
    ) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                f"/{page_id}/insights",
                params={"metric": metrics, "period": period},
                token=page_token,
            )
        )

    # --- instagram --------------------------------------------------------
    def get_instagram_account(self, page_id: str, page_token: str) -> Optional[str]:
        payload = self._request(
            "GET", f"/{page_id}", params={"fields": "instagram_business_account"}, token=page_token
        )
        ig = payload.get("instagram_business_account") if isinstance(payload, dict) else None
        return ig.get("id") if ig else None

    def get_instagram_media(self, ig_user_id: str, token: str) -> List[dict]:
        return self._data(
            self._request(
                "GET",
                f"/{ig_user_id}/media",
                params={"fields": "id,caption,media_type,media_url,timestamp"},
                token=token,
            )
        )

    def create_instagram_media(
        self, ig_user_id: str, token: str, image_url: str, caption: str = ""
    ) -> dict:
        return self._request(
            "POST",
            f"/{ig_user_id}/media",
            data={"image_url": image_url, "caption": caption},
            token=token,
        )

    def publish_instagram_media(self, ig_user_id: str, token: str, creation_id: str) -> dict:
        return self._request(
            "POST", f"/{ig_user_id}/media_publish", data={"creation_id": creation_id}, token=token
        )

    # --- lead-gen (task 4.8) ---------------------------------------------
    def get_lead_forms(self, page_id: str, page_token: str) -> List[dict]:
        return self._data(
            self._request("GET", f"/{page_id}/leadgen_forms", token=page_token)
        )

    def get_leads(self, form_id: str, page_token: str) -> List[dict]:
        return self._data(
            self._request("GET", f"/{form_id}/leads", token=page_token)
        )

    def close(self) -> None:
        self._client.close()
