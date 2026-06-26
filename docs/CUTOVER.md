# Cutover Runbook — .NET → FastAPI + Next.js (Phase 9)

Big-bang cutover. Read top to bottom; the data migration and DNS switch are the
irreversible steps — rehearse them in staging first. Artifacts referenced here:
`backend/Dockerfile`, `frontend/Dockerfile`, `infra/docker-compose.prod.yml`,
`infra/.env.production.example`, `backend/scripts/etl_sqlserver_to_postgres.py`.

## 0. Pre-flight (do these first)
- [ ] **Rotate every secret** (Facebook app secret, Stripe keys, any others) — the
      old values leaked into git history. Fill `infra/.env.production`.
- [ ] Generate `TOKEN_ENCRYPTION_KEY`:
      `python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"`
- [ ] Provision Postgres, Redis, object storage (S3/MinIO), email provider.
- [ ] Update Facebook app: OAuth redirect → `https://api.<domain>/api/v1/integrations/facebook/callback`.
- [ ] Create Stripe products/prices; set `NEXT_PUBLIC_STRIPE_PRICE_*`; add the webhook
      endpoint → `https://api.<domain>/api/v1/payments/webhook`.
- [ ] Confirm the legacy password-hash port against **real samples** from
      `AspNetUsers.PasswordHash` (task 2.2) before relying on no-reset login.

## 1. Data migration (task 9.1)
1. Stand up the target schema: `alembic upgrade head` against the prod Postgres.
2. **Dry run** (validates + prints source→target row counts, writes nothing):
   ```bash
   export SOURCE_DATABASE_URL="mssql+pyodbc://user:pass@host/Sohi?driver=ODBC+Driver+18+for+SQL+Server"
   export DATABASE_URL="postgresql+psycopg://...prod..."
   export TOKEN_ENCRYPTION_KEY="...same key the app will use..."
   python scripts/etl_sqlserver_to_postgres.py --dry-run
   ```
3. Freeze writes on the old app (maintenance mode) to avoid drift.
4. **Real run:** `python scripts/etl_sqlserver_to_postgres.py` — commits only if all
   row counts match. Tokens are encrypted at load; password hashes copied verbatim.
5. Spot-check: log in as a migrated user (hash upgrades transparently); confirm a
   few leads / connections / plans by hand.

## 2. Deploy (task 9.2)
```bash
docker compose -f infra/docker-compose.prod.yml --env-file infra/.env.production up -d --build
```
Brings up: Postgres, Redis, **api** (runs `alembic upgrade head` then uvicorn),
**worker** (`arq app.worker.settings.WorkerSettings`), **frontend** (Next standalone).
For real prod, replace db/redis/storage with managed services and deploy the
frontend to Vercel (set `BACKEND_URL`) if preferred.

- [ ] `GET https://api.<domain>/health` → ok
- [ ] `https://api.<domain>/docs` loads
- [ ] Worker logs show the `enqueue_due_posts` cron firing

## 3. Parity QA (task 9.3) — smoke test against the old app
- [ ] Register → email confirm → login (cookie set, `/portal` reachable)
- [ ] Legacy user logs in with old password (no reset)
- [ ] Leads: list / search / create / edit / delete
- [ ] Connect Facebook (OAuth round-trip) → page appears under Settings → Connections
- [ ] Schedule a post → worker publishes at the due time → status `published`
- [ ] Stripe: subscribe → webhook flips `is_account_paid`; failed payment → `on_hold`
- [ ] Media upload returns a URL
- [ ] Tenant isolation: a second account cannot see the first's data

## 4. DNS cutover + rollback (task 9.4)
- [ ] Lower DNS TTL (e.g. 300s) a day before.
- [ ] Point `app.<domain>` → frontend, `api.<domain>` → backend.
- [ ] Watch logs/error rate for 30–60 min.
- **Rollback:** repoint DNS to the old .NET app (still running, in maintenance
  mode). Because the old DB is untouched (we read from it, never wrote), reverting
  is just a DNS change. Keep the old stack warm for at least one billing cycle.

## 5. Decommission (task 9.5)
- [ ] Old app idle ≥ 1–2 weeks with no rollback needed.
- [ ] Final backup/snapshot of the SQL Server DB; archive it.
- [ ] Stop the .NET app; remove the `Sohi.Api` / `Sohi.Web` / `Sohi.Models` projects
      (or tag the repo state `legacy-dotnet-final` first).
- [ ] Revoke any old service credentials not already rotated.

## Outstanding before/at cutover (tracked in MIGRATION_PLAN)
- Backend Graph-proxy routes + ad-create + team-management endpoints (unblocks the
  remaining portal screens 8.4–8.7).
- Stripe Elements/Checkout Session for real card capture (Phase 7 note).
- Plan/quota enforcement (3.8) and trial/suspension enforcement (3.9).
- Real email provider wiring (2.6); 2FA (2.8); CI integration tests for live Graph (4.2–4.4).
- Frontend runtime tests (Playwright/Vitest).
