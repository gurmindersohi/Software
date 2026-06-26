"""Centralized tenant-scoped data access.

Every tenant query goes through here, so isolation is enforced in ONE audited
place instead of relying on each route remembering `.where(account_id == ...)`.
This is the app-layer first line of defense; Postgres Row-Level Security is the
production backstop (see migrations / docs).
"""
from typing import List, Optional, Type, TypeVar
from uuid import UUID

from sqlalchemy import func
from sqlmodel import Session, SQLModel, select

from app.core.exceptions import NotFoundError

T = TypeVar("T", bound=SQLModel)


def owned_or_404(
    session: Session, model: Type[T], obj_id: UUID, account_id: UUID, *, name: str = "Resource"
) -> T:
    """Fetch a row by id, but only if it belongs to the tenant — else 404."""
    obj = session.get(model, obj_id)
    if obj is None or getattr(obj, "account_id", None) != account_id:
        raise NotFoundError(f"{name} not found.")
    return obj


def list_owned(session: Session, model: Type[T], account_id: UUID, extra=None) -> List[T]:
    stmt = select(model).where(model.account_id == account_id)
    if extra is not None:
        stmt = stmt.where(extra)
    return session.exec(stmt).all()


def count_owned(session: Session, model: Type[T], account_id: UUID, extra=None) -> int:
    stmt = select(func.count()).select_from(model).where(model.account_id == account_id)
    if extra is not None:
        stmt = stmt.where(extra)
    return session.scalar(stmt) or 0


def first_owned(session: Session, model: Type[T], account_id: UUID, extra) -> Optional[T]:
    stmt = select(model).where(model.account_id == account_id).where(extra)
    return session.exec(stmt).first()
