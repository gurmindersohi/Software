"""Audit log viewer (Tier 4) — Owner-gated, account-scoped."""
from fastapi import APIRouter, Depends, Query
from sqlmodel import Session, select

from app.api.deps import get_current_account, require_owner
from app.core.tenancy import count_owned
from app.db.session import get_session
from app.models.account import Account
from app.models.audit_log import AuditLog
from app.models.user import User
from app.schemas.audit import AuditLogRead
from app.schemas.pagination import Page

router = APIRouter(prefix="/api/v1/audit-logs", tags=["audit"])


@router.get("", response_model=Page[AuditLogRead])
def list_audit_logs(
    limit: int = Query(50, ge=1, le=200),
    offset: int = Query(0, ge=0),
    _: User = Depends(require_owner),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Page[AuditLogRead]:
    total = count_owned(session, AuditLog, account.id)
    items = session.exec(
        select(AuditLog)
        .where(AuditLog.account_id == account.id)
        .order_by(AuditLog.created_on.desc())
        .limit(limit)
        .offset(offset)
    ).all()
    return Page(items=items, total=total, limit=limit, offset=offset)
