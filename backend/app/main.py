"""FastAPI application entrypoint."""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app import __version__
from app.api.routes import (
    account,
    ads,
    auth,
    billing,
    health,
    integrations,
    leads,
    media,
    payments,
    scheduled_posts,
    social,
)
from app.core.config import settings

app = FastAPI(
    title=settings.app_name,
    version=__version__,
    description="Social Media & Ads Manager API (FastAPI re-platform of Sohi.Api).",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[settings.frontend_origin],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(health.router)
app.include_router(auth.router)
app.include_router(account.router)
app.include_router(leads.router)
app.include_router(social.router)
app.include_router(ads.router)
app.include_router(billing.router)
app.include_router(integrations.router)
app.include_router(payments.router)
app.include_router(media.router)
app.include_router(scheduled_posts.router)


@app.get("/", tags=["health"])
def root() -> dict:
    return {"app": settings.app_name, "version": __version__, "docs": "/docs"}
