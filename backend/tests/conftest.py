"""Pytest fixtures: in-memory SQLite DB + FastAPI TestClient (no Postgres needed)."""
import pytest
from fastapi.testclient import TestClient
from sqlmodel import Session, SQLModel, create_engine
from sqlmodel.pool import StaticPool

import app.models  # noqa: F401  -- registers tables on SQLModel.metadata
from app.db.session import get_session
from app.main import app


@pytest.fixture(name="engine")
def engine_fixture():
    engine = create_engine(
        "sqlite://",
        connect_args={"check_same_thread": False},
        poolclass=StaticPool,
    )
    SQLModel.metadata.create_all(engine)
    yield engine
    SQLModel.metadata.drop_all(engine)


@pytest.fixture(name="session")
def session_fixture(engine):
    with Session(engine) as session:
        yield session


@pytest.fixture(name="client")
def client_fixture(session):
    def _get_session_override():
        yield session

    app.dependency_overrides[get_session] = _get_session_override
    yield TestClient(app)
    app.dependency_overrides.clear()
