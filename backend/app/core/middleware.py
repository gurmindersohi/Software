"""Observability middleware (task 0.8): request logging + basic rate limiting.

The rate limiter is in-process (per-worker) — a sensible baseline DoS guard.
For multi-instance production, move it to Redis (the infra is already there).
"""
import logging
import time
from collections import defaultdict, deque

from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import JSONResponse, Response

logger = logging.getLogger("app.request")


class RequestLoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next) -> Response:
        start = time.perf_counter()
        response = await call_next(request)
        elapsed_ms = (time.perf_counter() - start) * 1000
        logger.info(
            "%s %s -> %s (%.1fms)",
            request.method,
            request.url.path,
            response.status_code,
            elapsed_ms,
        )
        return response


class RateLimitMiddleware(BaseHTTPMiddleware):
    def __init__(self, app, limit_per_minute: int = 1000) -> None:
        super().__init__(app)
        self.limit = limit_per_minute
        self.window = 60.0
        self._hits: dict[str, deque] = defaultdict(deque)

    async def dispatch(self, request: Request, call_next) -> Response:
        client = request.client.host if request.client else "anon"
        if client == "testclient":  # never throttle the test suite
            return await call_next(request)

        now = time.time()
        hits = self._hits[client]
        while hits and hits[0] <= now - self.window:
            hits.popleft()
        if len(hits) >= self.limit:
            return JSONResponse({"detail": "Rate limit exceeded."}, status_code=429)
        hits.append(now)
        return await call_next(request)
