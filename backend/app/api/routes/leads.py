"""Leads CRUD + search, scoped to the caller's tenant."""
from datetime import datetime, timezone
from typing import List, Optional
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import or_
from sqlmodel import Session, select

from app.api.deps import get_current_account, require_active_account
from app.db.session import get_session
from app.models.account import Account
from app.models.lead import Lead
from app.schemas.lead import LeadCreate, LeadRead, LeadUpdate

router = APIRouter(prefix="/api/v1/leads", tags=["leads"])


def _get_owned_lead(session: Session, lead_id: UUID, account: Account) -> Lead:
    lead = session.get(Lead, lead_id)
    if lead is None or lead.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lead not found.")
    return lead


@router.get("", response_model=List[LeadRead])
def list_leads(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[Lead]:
    return session.exec(select(Lead).where(Lead.account_id == account.id)).all()


@router.get("/search", response_model=List[LeadRead])
def search_leads(
    name: Optional[str] = None,
    email: Optional[str] = None,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[Lead]:
    stmt = select(Lead).where(Lead.account_id == account.id)
    if name:
        like = f"%{name}%"
        stmt = stmt.where(
            or_(
                Lead.first_name.ilike(like),
                Lead.last_name.ilike(like),
                Lead.full_name.ilike(like),
            )
        )
    if email:
        stmt = stmt.where(Lead.email.ilike(f"%{email}%"))
    return session.exec(stmt).all()


@router.get("/{lead_id}", response_model=LeadRead)
def get_lead(
    lead_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Lead:
    return _get_owned_lead(session, lead_id, account)


@router.post("", response_model=LeadRead, status_code=status.HTTP_201_CREATED)
def create_lead(
    body: LeadCreate,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
) -> Lead:
    # Duplicate-email check is scoped to the tenant (old API checked globally — a bug).
    existing = session.exec(
        select(Lead).where(Lead.account_id == account.id, Lead.email == body.email)
    ).first()
    if existing is not None:
        raise HTTPException(status.HTTP_409_CONFLICT, "A lead with this email already exists.")
    lead = Lead(**body.model_dump(), account_id=account.id)
    session.add(lead)
    session.commit()
    session.refresh(lead)
    return lead


@router.put("/{lead_id}", response_model=LeadRead)
def update_lead(
    lead_id: UUID,
    body: LeadUpdate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Lead:
    lead = _get_owned_lead(session, lead_id, account)
    for key, value in body.model_dump(exclude_unset=True).items():
        setattr(lead, key, value)
    lead.modified_on = datetime.now(timezone.utc)
    session.add(lead)
    session.commit()
    session.refresh(lead)
    return lead


@router.delete("/{lead_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_lead(
    lead_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    lead = _get_owned_lead(session, lead_id, account)
    session.delete(lead)
    session.commit()
