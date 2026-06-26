"""Connected social accounts (tokens), scoped to the caller's tenant."""
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
from app.core.exceptions import NotFoundError
from app.core.quotas import enforce_quota
from app.core.tenancy import first_owned, list_owned, owned_or_404
from app.db.session import get_session
from app.integrations.facebook.graph import GraphError
from app.models.account import Account
from app.models.lead import Lead
from app.models.social import SocialMedia
from app.schemas.graph import PagePostInput
from app.schemas.social import SocialMediaCreate, SocialMediaRead

router = APIRouter(prefix="/api/v1/social", tags=["social"])


@router.get("", response_model=List[SocialMediaRead])
def list_tokens(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[SocialMedia]:
    return list_owned(session, SocialMedia, account.id)


@router.get("/{platform}", response_model=SocialMediaRead)
def get_token_by_platform(
    platform: str,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> SocialMedia:
    row = first_owned(session, SocialMedia, account.id, SocialMedia.type == platform)
    if row is None:
        raise NotFoundError("No connection for that platform.")
    return row


@router.post("", response_model=SocialMediaRead, status_code=status.HTTP_201_CREATED)
def save_token(
    body: SocialMediaCreate,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
) -> SocialMedia:
    enforce_quota(
        session,
        plan_name=account.plan_name,
        account_id=account.id,
        resource="social_sets",
        model=SocialMedia,
    )
    data = body.model_dump()
    data["access_token"] = crypto.encrypt_optional(data.get("access_token"))
    data["secret"] = crypto.encrypt_optional(data.get("secret"))
    token = SocialMedia(**data, account_id=account.id)
    session.add(token)
    session.commit()
    session.refresh(token)
    return token


@router.delete("/{token_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_token(
    token_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    token = owned_or_404(session, SocialMedia, token_id, account.id, name="Connection")
    session.delete(token)
    session.commit()


# --- Graph-proxy: live page content/insights (tasks 8.5/8.6) --------------
def _owned_connection(session: Session, connection_id: UUID, account: Account) -> SocialMedia:
    return owned_or_404(session, SocialMedia, connection_id, account.id, name="Connection")


def _page_graph(conn: SocialMedia, graph_factory: GraphFactory):
    token = crypto.decrypt(conn.access_token) if conn.access_token else None
    return graph_factory(token), token


@router.get("/{connection_id}/posts")
def page_posts(
    connection_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    conn = _owned_connection(session, connection_id, account)
    graph, token = _page_graph(conn, graph_factory)
    try:
        return graph.get_page_posts(conn.page_id, token)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{connection_id}/insights")
def page_insights(
    connection_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    conn = _owned_connection(session, connection_id, account)
    graph, token = _page_graph(conn, graph_factory)
    try:
        return graph.get_page_insights(conn.page_id, token)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.get("/{connection_id}/instagram-insights")
def instagram_insights(
    connection_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    conn = _owned_connection(session, connection_id, account)
    graph, token = _page_graph(conn, graph_factory)
    try:
        return graph.get_instagram_insights(conn.page_id, token)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc


@router.post("/{connection_id}/sync-leads")
def sync_lead_forms(
    connection_id: UUID,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
) -> dict:
    """Pull Facebook lead-gen form submissions into the Leads table (task 4.8)."""
    conn = _owned_connection(session, connection_id, account)
    graph, token = _page_graph(conn, graph_factory)
    try:
        forms = graph.get_lead_forms(conn.page_id, token)
        imported = 0
        for form in forms:
            for entry in graph.get_leads(form.get("id"), token):
                fields = {
                    f.get("name"): (f.get("values") or [None])[0]
                    for f in entry.get("field_data", [])
                }
                email = fields.get("email")
                if not email:
                    continue
                exists = session.exec(
                    select(Lead).where(Lead.account_id == account.id, Lead.email == email)
                ).first()
                if exists:
                    continue
                session.add(
                    Lead(
                        account_id=account.id,
                        email=email,
                        first_name=fields.get("first_name"),
                        last_name=fields.get("last_name"),
                        full_name=fields.get("full_name"),
                        primary_phone=fields.get("phone_number"),
                        lead_source="facebook_leadgen",
                    )
                )
                imported += 1
        session.commit()
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc
    return {"imported": imported}


@router.post("/{connection_id}/posts", status_code=status.HTTP_201_CREATED)
def create_page_post(
    connection_id: UUID,
    body: PagePostInput,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
    graph_factory: GraphFactory = Depends(get_graph_factory),
):
    conn = _owned_connection(session, connection_id, account)
    graph, token = _page_graph(conn, graph_factory)
    try:
        if body.video_url:
            return graph.create_video_post(conn.page_id, token, body.video_url, body.message)
        if body.image_url:
            return graph.create_photo_post(conn.page_id, token, body.image_url, body.message)
        return graph.create_post(conn.page_id, token, body.message, body.link)
    except GraphError as exc:
        raise HTTPException(status.HTTP_502_BAD_GATEWAY, str(exc)) from exc
