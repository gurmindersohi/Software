# Sohi Backend (FastAPI)

Re-platform of the .NET `Sohi.Api` + the backend logic that lived in `Sohi.Web`
(Facebook Graph, Stripe). See [`../docs/MIGRATION_PLAN.md`](../docs/MIGRATION_PLAN.md).

## Stack
- **FastAPI** + **SQLModel** (Pydantic v2)
- **PostgreSQL** via psycopg3, **Alembic** migrations
- **Redis** + **arq** for background jobs (scheduled posts) — Phase 5
- **MinIO/S3** for media — Phase 4.7

## Quickstart (Postgres, via Docker)
```bash
docker compose -f ../infra/docker-compose.yml up -d   # Postgres + Redis + MinIO
cp .env.example .env                                  # fill in rotated secrets
python -m venv .venv && source .venv/bin/activate
pip install -e ".[dev]"
alembic upgrade head
python scripts/seed.py --demo                         # plans + demo@sohi.app / demo1234
uvicorn app.main:app --reload
```

## Quickstart (no Docker — SQLite)
The app defaults to a local SQLite file, so you can run the API with nothing else:
```bash
python -m venv .venv && source .venv/bin/activate
pip install -e ".[dev]"
export DATABASE_URL="sqlite:///./sohi_dev.db"
alembic upgrade head
python scripts/seed.py --demo
uvicorn app.main:app --reload
```
(The arq worker still needs Redis; everything else works on SQLite.)

Open http://localhost:8000/docs for the OpenAPI UI, http://localhost:8000/health for liveness.

## Tests
```bash
pytest            # uses an in-memory SQLite DB, no Postgres required
ruff check . && mypy app
```

## Layout
```
app/
  core/      config (pydantic-settings)
  db/        engine + session
  models/    SQLModel tables  (Account, Plan, Lead, SocialMedia, AdAccount)
  schemas/   Pydantic API/transport shapes (Facebook Graph DTOs, etc.)
  api/routes routers
migrations/  Alembic
tests/
```

## Model mapping note
The old solution had 19 C# classes but only a handful were persisted (EF `DbSet`s:
Accounts, Plans, Leads, SocialMediaAccounts, AdAccounts). The Facebook-shaped
classes (Campaign, Adset, Ad, AdImage, Targeting, FacebookLocation, Post,
PageInsights, PostInsights, Values, Profile, Checkout) mirror Graph API payloads
and are modelled as **Pydantic schemas**, not DB tables.
