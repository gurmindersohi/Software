"""Audit trail recorder (Tier 4). Call AFTER an action commits — it persists
one audit row in its own commit, independent of the caller's transaction."""
from typing import Optional
from uuid import UUID

from sqlmodel import Session

from app.models.audit_log import AuditLog


def record(
    session: Session,
    *,
    action: str,
    account_id: Optional[UUID] = None,
    user_id: Optional[UUID] = None,
    detail: Optional[str] = None,
) -> None:
    session.add(
        AuditLog(action=action, account_id=account_id, user_id=user_id, detail=detail)
    )
    session.commit()
