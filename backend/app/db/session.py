"""Database engine and session management."""
from collections.abc import Iterator

from sqlalchemy.engine import Engine
from sqlmodel import Session, create_engine

from app.core.config import settings


def _make_engine(url: str) -> Engine:
    # check_same_thread is a SQLite-only flag (used for tests / lightweight dev).
    connect_args = {"check_same_thread": False} if url.startswith("sqlite") else {}
    return create_engine(url, echo=settings.debug, connect_args=connect_args)


engine = _make_engine(settings.database_url)


def get_session() -> Iterator[Session]:
    """FastAPI dependency that yields a transactional session."""
    with Session(engine) as session:
        yield session
