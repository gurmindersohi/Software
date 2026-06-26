"""Centralized tenant-scoped data access.

Isolation is enforced **at the application layer**: every tenant query funnels
through this module, so the scoping lives in one auditable place instead of each
route remembering `.where(account_id == ...)`. Database-level Row-Level Security
would be a stronger backstop but is intentionally out of scope for this project.
"""
from typing import List, Optional, Tuple, Type, TypeVar
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


def page_owned(
    session: Session,
    model: Type[T],
    account_id: UUID,
    *,
    limit: int,
    offset: int,
    extra=None,
) -> Tuple[List[T], int]:
    """Return (items, total) for a tenant-scoped, optionally-filtered page."""
    total = count_owned(session, model, account_id, extra)
    stmt = select(model).where(model.account_id == account_id)
    if extra is not None:
        stmt = stmt.where(extra)
    items = session.exec(stmt.limit(limit).offset(offset)).all()
    return items, total
