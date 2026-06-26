"""Clients/workspaces (Tier 2) — an agency manages many clients. Owner-gated mutations."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, Response, status
from sqlmodel import Session

from app.api.deps import get_current_account, require_owner
from app.core.tenancy import list_owned, owned_or_404
from app.db.session import get_session
from app.models.account import Account
from app.models.client import Client
from app.models.user import User
from app.schemas.client import ClientCreate, ClientRead, ClientUpdate
from app.services.reports import client_report_pdf

router = APIRouter(prefix="/api/v1/clients", tags=["clients"])


@router.get("", response_model=List[ClientRead])
def list_clients(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[Client]:
    return list_owned(session, Client, account.id, Client.is_deleted == False)  # noqa: E712


@router.post("", response_model=ClientRead, status_code=status.HTTP_201_CREATED)
def create_client(
    body: ClientCreate,
    _: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Client:
    client = Client(account_id=account.id, name=body.name)
    session.add(client)
    session.commit()
    session.refresh(client)
    return client


@router.get("/{client_id}", response_model=ClientRead)
def get_client(
    client_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Client:
    return owned_or_404(session, Client, client_id, account.id, name="Client")


@router.put("/{client_id}", response_model=ClientRead)
def update_client(
    client_id: UUID,
    body: ClientUpdate,
    _: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Client:
    client = owned_or_404(session, Client, client_id, account.id, name="Client")
    if body.name is not None:
        client.name = body.name
    session.add(client)
    session.commit()
    session.refresh(client)
    return client


@router.get("/{client_id}/report")
def client_report(
    client_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Response:
    client = owned_or_404(session, Client, client_id, account.id, name="Client")
    pdf = client_report_pdf(session, account, client)
    return Response(
        content=pdf,
        media_type="application/pdf",
        headers={"Content-Disposition": f'attachment; filename="report-{client.name}.pdf"'},
    )


@router.delete("/{client_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_client(
    client_id: UUID,
    _: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    client = owned_or_404(session, Client, client_id, account.id, name="Client")
    client.is_deleted = True
    session.add(client)
    session.commit()
