"""Connected Facebook ad accounts, scoped to the caller's tenant."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import (
    GraphFactory,
    get_current_account,
    get_graph_factory,
    require_active_account,
)
from app.core import crypto
from app.db.session import get_session
from app.integrations.facebook.graph import GraphError
from app.models.account import Account
from app.models.ad_account import AdAccount
from app.schemas.ads import AdAccountCreate, AdAccountRead
from app.schemas.graph import AdCreateInput, AdsetCreateInput, CampaignCreateInput

router = APIRouter(prefix="/api/v1/ad-accounts", tags=["ads"])


@router.get("", response_model=List[AdAccountRead])
def list_ad_accounts(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[AdAccount]:
    return session.exec(
        select(AdAccount).where(AdAccount.account_id == account.id)
    ).all()


@router.post("", response_model=AdAccountRead, status_code=status.HTTP_201_CREATED)
def save_ad_account(
    body: AdAccountCreate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> AdAccount:
    data = body.model_dump()
    data["access_token"] = crypto.encrypt_optional(data.get("access_token"))
    data["secret"] = crypto.encrypt_optional(data.get("secret"))
    ad_account = AdAccount(**data, account_id=account.id)
    session.add(ad_account)
    session.commit()
    session.refresh(ad_account)
    return ad_account


# --- Graph-proxy: live campaigns/adsets/ads (task 8.7) --------------------
def _owned_ad_account(session: Session, conn_id: UUID, account: Account) -> AdAccount:
    acct = session.get(AdAccount, conn_id)
    if acct is None or acct.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Ad account not found.")
    return acct


def _ads_graph(acct: AdAccount, graph_factory: GraphFactory):
    token = crypto.decrypt(acct.access_token) if acct.access_token else None
    return graph_factory(token)


@router.get("/{conn_id}/campaigns")
def list_campaigns(
    conn_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).get_campaigns(acct.user_account_id)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.post("/{conn_id}/campaigns", status_code=status.HTTP_201_CREATED)
def create_campaign(
    conn_id: UUID,
    body: CampaignCreateInput,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).create_campaign(
            acct.user_account_id, body.name, body.objective, body.status
        )
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{conn_id}/adsets")
def list_adsets(
    conn_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).get_adsets(acct.user_account_id)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{conn_id}/ads")
def list_ads(
    conn_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).get_ads(acct.user_account_id)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


# --- ad-set / ad creation wizard ------------------------------------------
@router.post("/{conn_id}/adsets", status_code=status.HTTP_201_CREATED)
def create_adset(
    conn_id: UUID,
    body: AdsetCreateInput,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    targeting: dict = {
        "age_min": body.age_min,
        "age_max": body.age_max,
        "geo_locations": {"countries": body.country_codes},
    }
    if body.interest_ids:
        targeting["flexible_spec"] = [
            {"interests": [{"id": i} for i in body.interest_ids]}
        ]
    try:
        return _ads_graph(acct, graph_factory).create_adset(
            acct.user_account_id,
            name=body.name,
            campaign_id=body.campaign_id,
            daily_budget=body.daily_budget,
            optimization_goal=body.optimization_goal,
            billing_event=body.billing_event,
            targeting=targeting,
            status=body.status,
        )
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.post("/{conn_id}/ads", status_code=status.HTTP_201_CREATED)
def create_ad(
    conn_id: UUID,
    body: AdCreateInput,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    graph = _ads_graph(acct, graph_factory)
    link_data: dict = {"message": body.message, "link": body.link, "name": body.headline}
    if body.image_url:
        try:
            image_hash = graph.upload_ad_image(acct.user_account_id, body.image_url)
        except GraphError as exc:
            raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc
        if image_hash:
            link_data["image_hash"] = image_hash
        else:
            link_data["picture"] = body.image_url  # fallback: creative from URL
    creative = {"object_story_spec": {"page_id": body.page_id, "link_data": link_data}}
    try:
        return graph.create_ad(
            acct.user_account_id,
            name=body.name,
            adset_id=body.adset_id,
            creative=creative,
            status=body.status,
        )
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{conn_id}/targeting-search")
def targeting_search(
    conn_id: UUID,
    q: str,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).search_targeting(q)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{conn_id}/location-search")
def location_search(
    conn_id: UUID,
    q: str,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    acct = _owned_ad_account(session, conn_id, account)
    try:
        return _ads_graph(acct, graph_factory).search_locations(q)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc
