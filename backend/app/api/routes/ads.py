"""Connected Facebook ad accounts, scoped to the caller's tenant."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import GraphFactory, get_current_account, get_graph_factory
from app.core import crypto
from app.db.session import get_session
from app.integrations.facebook.graph import GraphError
from app.models.account import Account
from app.models.ad_account import AdAccount
from app.schemas.ads import AdAccountCreate, AdAccountRead
from app.schemas.graph import CampaignCreateInput

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
    account: Account = Depends(get_current_account),
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
