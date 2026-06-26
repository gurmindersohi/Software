"""Liveness / readiness endpoints."""
from fastapi import APIRouter, Depends, HTTPException, status
from pydantic import BaseModel
from sqlalchemy import text
from sqlmodel import Session

from app import __version__
from app.db.session import get_session

router = APIRouter(tags=["health"])


class HealthResponse(BaseModel):
    status: str = "ok"
    version: str


@router.get("/health", response_model=HealthResponse)
def health() -> HealthResponse:
    """Liveness — the process is up (no dependencies checked)."""
    return HealthResponse(status="ok", version=__version__)


@router.get("/readyz", response_model=HealthResponse)
def readyz(session: Session = Depends(get_session)) -> HealthResponse:
    """Readiness — verifies the database is reachable (for load balancers)."""
    try:
        session.execute(text("SELECT 1"))
    except Exception as exc:  # DB unreachable
        raise HTTPException(
            status.HTTP_503_SERVICE_UNAVAILABLE, "Database not reachable."
        ) from exc
    return HealthResponse(status="ready", version=__version__)
