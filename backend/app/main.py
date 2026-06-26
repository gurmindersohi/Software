"""FastAPI application entrypoint."""
from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app import __version__
from app.api.routes import (
    account,
    admin,
    ads,
    audit,
    auth,
    billing,
    clients,
    health,
    integrations,
    leads,
    media,
    payments,
    scheduled_posts,
    social,
    team,
    twofa,
)
from app.core.config import settings
from app.core.exceptions import AppError
from app.core.middleware import RateLimitMiddleware, RequestLoggingMiddleware

# Error tracking — only active when SENTRY_DSN is configured.
if settings.sentry_dsn:
    import sentry_sdk

    sentry_sdk.init(
        dsn=settings.sentry_dsn,
        environment=settings.environment,
        traces_sample_rate=0.1,
    )

app = FastAPI(
    title=settings.app_name,
    version=__version__,
    description="Social Media & Ads Manager API (FastAPI re-platform of Sohi.Api).",
)

@app.exception_handler(AppError)
async def _app_error_handler(request: Request, exc: AppError) -> JSONResponse:
    """Translate domain exceptions to HTTP at the edge."""
    return JSONResponse({"detail": exc.detail}, status_code=exc.status_code)


app.add_middleware(RequestLoggingMiddleware)
app.add_middleware(RateLimitMiddleware, limit_per_minute=settings.rate_limit_per_minute)
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
app.include_router(clients.router)
app.include_router(leads.router)
app.include_router(social.router)
app.include_router(ads.router)
app.include_router(billing.router)
app.include_router(integrations.router)
app.include_router(payments.router)
app.include_router(media.router)
app.include_router(scheduled_posts.router)
app.include_router(team.router)
app.include_router(twofa.router)
app.include_router(audit.router)
app.include_router(admin.router)


@app.get("/", tags=["health"])
def root() -> dict:
    return {"app": settings.app_name, "version": __version__, "docs": "/docs"}
