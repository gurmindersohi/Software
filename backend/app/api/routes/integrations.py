"""Facebook/Instagram OAuth connect flow (task 4.1).

`/connect` (authed) returns the Meta OAuth dialog URL with the tenant id in
`state`. `/callback` (unauthed — Meta redirects here) exchanges the code,
upgrades to a long-lived token, fetches the user's pages, and stores each as a
SocialMedia connection with its **page token encrypted at rest**.
"""
from urllib.parse import urlencode
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, Query, status
from sqlmodel import Session

from app.api.deps import get_current_account
from app.core import crypto
from app.core.config import settings
from app.db.session import get_session
from app.integrations.facebook.graph import GraphClient, GraphError
from app.models.account import Account
from app.models.social import SocialMedia
from app.schemas.integrations import ConnectionResult, FacebookConnectResponse

router = APIRouter(prefix="/api/v1/integrations/facebook", tags=["integrations"])


def get_graph_client() -> GraphClient:
    """Dependency so tests can inject a fake Graph client."""
    return GraphClient()


@router.get("/connect", response_model=FacebookConnectResponse)
def facebook_connect(
    account: Account = Depends(get_current_account),
) -> FacebookConnectResponse:
    if not settings.facebook_app_id:
        raise HTTPException(status.HTTP_503_SERVICE_UNAVAILABLE, "Facebook is not configured.")
    params = {
        "client_id": settings.facebook_app_id,
        "redirect_uri": settings.facebook_redirect_url,
        "scope": settings.facebook_scopes,
        "state": str(account.id),
        "response_type": "code",
    }
    url = (
        f"https://www.facebook.com/{settings.facebook_graph_version}"
        f"/dialog/oauth?{urlencode(params)}"
    )
    return FacebookConnectResponse(authorize_url=url)


@router.get("/callback", response_model=ConnectionResult)
def facebook_callback(
    code: str = Query(...),
    state: str = Query(...),
    session: Session = Depends(get_session),
    client: GraphClient = Depends(get_graph_client),
) -> ConnectionResult:
    try:
        account_id = UUID(state)
    except ValueError as exc:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid state.") from exc

    account = session.get(Account, account_id)
    if account is None:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Account not found.")

    try:
        token_resp = client.exchange_code_for_token(code, settings.facebook_redirect_url)
        user_token = token_resp["access_token"]
        long_lived = client.get_long_lived_token(user_token)
        client.access_token = long_lived.get("access_token", user_token)
        pages = client.get_pages()
    except (GraphError, KeyError) as exc:
        raise HTTPException(
            status.HTTP_502_BAD_GATEWAY, f"Facebook connection failed: {exc}"
        ) from exc

    count = 0
    for page in pages:
        session.add(
            SocialMedia(
                page_id=page.get("id"),
                name=page.get("name"),
                type="facebook",
                access_token=crypto.encrypt_optional(page.get("access_token")),
                account_id=account.id,
            )
        )
        count += 1
    session.commit()
    return ConnectionResult(detail="Facebook connected.", pages_connected=count)
