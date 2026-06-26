# Migration Plan: .NET (Blazor + Web API) → FastAPI + Next.js

> Status: **planning / backlog** — no code yet. This is the task breakdown.

## Locked decisions

| Area | Decision |
|---|---|
| Strategy | **Big-bang rewrite** (build full new stack, cut over once) |
| Backend | **Python + FastAPI** |
| Database | **PostgreSQL** (migrate data from SQL Server) |
| ORM | **SQLModel** + **Alembic** migrations |
| Auth | **Roll our own** — FastAPI-Users + JWT in **httpOnly cookies**; port ASP.NET Identity PBKDF2 hashes so existing passwords keep working |
| Background jobs | **arq + Redis** (scheduled-post queue) |
| Frontend | **Next.js (App Router)** — whole app (marketing + portal) |
| Contract | **OpenAPI-first** — generate a typed TS client for Next.js |
| Repo | **Monorepo**: `/backend` (FastAPI), `/frontend` (Next.js), `/infra`, `/docs` |
| Scope | Analyze everything, migrate as **small tasks** (this doc) |

---

## What exists today (inventory)

**Backend — `Sohi.Api` (6 controllers / repositories):**
- **Account** — get by id, update (PUT), create (POST), get by name
- **Ads** — get all ad accounts by account, save ad account
- **Billing** — update account (plan/subscription)
- **Leads** — list, get one, create, update, delete, search
- **Settings** — get account, update account
- **Social** — save token, get token by platform, get all tokens, delete account

**Backend logic living in `Sohi.Web` (must move into FastAPI):**
- **Facebook Graph integration** — `Services/Social/SocialService.cs`, `Services/Ads/AdAccountService.cs` (Graph **v10.0** — deprecated, rewrite vs current API)
- **Stripe** — `Pages/Website/Checkout.cshtml.cs`, `Pricing.cshtml.cs` (customer + subscription creation)
- Typed HttpClients that proxy to the API (Account/Ad/Billing/Lead/Settings/Social services)

**Domain models (19):** Account, Ad, AdAccount, AdImage, Adset, Campaign, Checkout, FacebookLocation, Lead, ModelBase, PageInsights, Plan, Post, PostInsights, Profile, SaveFile, SocialMedia, Targeting, Values. (Note: string IDs on some entities; a `FixDecimalPrecision` migration exists — watch money/decimal fields.)

**Auth surface (ASP.NET Identity — ~35 pages):** Register, RegisterConfirmation, Login, Logout, LoginWith2fa, LoginWithRecoveryCode, ForgotPassword/Reset/Confirmation, ResendEmailConfirmation, ConfirmEmail/Change, ExternalLogin, FreeTrial, Lockout, AccessDenied, and full **Manage** area (ChangePassword, SetPassword, Email, 2FA/Authenticator, RecoveryCodes, ExternalLogins, PersonalData export/delete).

**Frontend — marketing site (8 pages):** Home/Index, About, Features, Pricing, Checkout, Success, Contact.

**Frontend — portal (~56 components):**
- **Dashboard**
- **Leads** — LeadList, LeadDetails, DisplayLead, EditLead
- **Settings** — General, Business, Accounts, Connect, Facebook, Instagram, ManageUsers, NewUser, DisplayUser, Roles, Ads (AdAccounts, ConnectAdAccounts, FacebookAds)
- **Social** — Connect, Create, SocialVideos; **Facebook** (Pages, Posts, CreatePost, Queue, Insights, Analytics, SocialImages, Settings); **Instagram** (Posts, Queue, Insights)
- **Ads/Facebook** — Campaigns, CampaignsList, Adsets, Ads, Create, Manage; components: NewCampaign, NewAdSet, NewAd, AdImages, DetailedTargeting, Location, Age

**Two databases today:** app data (`AppDbContext` in API) + identity/users (`SohiWebContext` in Web). Plan unifies into one Postgres DB.

⚠️ **Security:** real Facebook `ClientSecret` and Stripe keys are committed in `appsettings.json` (now in git history). **Rotate them** as part of cutover; never carry them into the new repo.

---

## Migration backlog (small tasks, by phase)

Sizes: **S** ≈ <½ day, **M** ≈ ½–1 day, **L** ≈ 1–2 days. Dependencies noted.

### Phase 0 — Foundations
- [x] **0.1 (S)** Monorepo scaffold (`/backend`, `/frontend`, `/infra`, `/docs`), root README, `.gitignore`, license.
- [x] **0.2 (S)** Backend project: FastAPI app skeleton, settings via `pydantic-settings` + `.env`, health endpoint.
- [x] **0.3 (S)** `docker-compose` for local Postgres + Redis.
- [x] **0.4 (S)** Lint/format/test tooling: `ruff`, `mypy`, `pytest`; pre-commit hooks.
- [x] **0.5 (S)** CI pipeline (lint + test for backend & frontend).
- [~] **0.6 (S)** Secret management: `.env.example` + rotation note added. ⚠️ **Actual rotation of the leaked Facebook/Stripe keys is a manual user action — still pending.**
- [x] **0.7 (S)** Object storage (S3-compatible; MinIO in compose) for media — infra + settings done; upload pipeline is task 4.7.
- [x] **0.8 (S)** Observability: request logging + in-process **rate limiting** middleware + CORS. (Sentry/error-tracking left as an optional add-on.)

### Phase 1 — Data layer
- [x] **1.1 (M)** Port domain models → SQLModel tables (Account, Plan, Lead, SocialMedia, AdAccount; Graph DTOs → Pydantic schemas); UUID keys for internal entities, string for external Meta IDs.
- [x] **1.2 (S)** Alembic init + initial migration generating the full schema (verified: 5 tables, FKs, Numeric(18,2)).
- [~] **1.3 (M)** Type mapping: decimal precision, datetime, bool, UUID done. Full SQL-Server-specific parity finalised with the ETL (1.4).
- [N/A] **1.4 (M)** ETL script exists (`backend/scripts/etl_sqlserver_to_postgres.py`) but **not needed — no legacy data to import** (fresh start).
- [x] **1.5 (S)** Seed (`backend/scripts/seed.py`): the 3 Plans + an optional `--demo` owner account. **App booted on SQLite and smoke-tested over HTTP** (login → me → plan → lead CRUD; 401 gate; /docs).

### Phase 2 — Auth (depends on 1)
> **Decision change:** implemented auth as our **own focused module (JWT + PBKDF2)** rather than
> pulling in **FastAPI-Users**. Rationale: FastAPI-Users is async-only and would force converting
> the whole sync data layer to async right now, and the legacy ASP.NET hash + rehash-on-login is
> custom work regardless. Fewer deps, fully unit-tested offline. Revisit if the stack goes async.
- [x] **2.1 (M)** User + Role models (unifies old `IdentityUser` + `IdentityRole`); register creates a tenant Account + Owner role.
- [x] **2.2 (M)** **ASP.NET Identity PBKDF2 verifier** ported (v2 0x00 + v3 0x01) with transparent re-hash on login. *(Kept for safety, but **not exercised** — no legacy users to import; new signups use the modern `pbkdf2_sha256` format.)*
- [x] **2.3 (S)** JWT access + refresh in **httpOnly cookies**; `/refresh` + `/logout` endpoints.
- [x] **2.4 (S)** Register + email-confirmation (`/register`, `/verify-email`).
- [x] **2.5 (S)** Forgot/reset password (`/forgot-password`, `/reset-password`).
- [x] **2.6 (S)** Email: pluggable sender — `console` + real **Resend** & **SendGrid** providers (httpx, request-building unit-tested).
- [x] **2.7 (M)** Account management: change-password, **change-email** (re-confirm), and **GDPR export + delete** (PII scrub) — all with frontend (Settings → Security).
- [x] **2.8 (M)** **2FA** — hand-rolled RFC 6238 TOTP + single-use recovery codes; two-step login (challenge cookie → `/2fa/verify`); Settings → Security UI.
- [x] **2.9 (S)** Roles/permissions model (`Role` + `user_roles` link table).

### Phase 3 — Core API (depends on 1, 2)
Port each controller+repository to a FastAPI router with Pydantic schemas + tests:
- [x] **3.1/3.2 (S)** Account router — `GET/PUT /account` + `GET /plans/{name}`. (Old Account & Settings controllers were near-identical → merged.)
- [x] **3.3 (M)** Leads router — list / get / create / update / delete / `GET /leads/search`.
- [x] **3.4 (S)** Social tokens router — list / get-by-platform / save / delete. **Read schemas never return `access_token`/`secret`.**
- [x] **3.5 (S)** Ad-accounts router — list / save (token omitted from read schema).
- [x] **3.6 (S)** Billing router — `GET/PUT /billing` on the current Account.
- [x] **3.7 (S)** `get_current_account` dependency → **multi-tenant scoping on every route**; the tenant comes from the JWT, not a client-supplied id (security upgrade over the old API). Cross-tenant read/delete isolation is unit-tested.
- [x] **3.8 (M)** **Plan/quota enforcement** — `core/quotas.py` enforces seats / social sets / scheduled posts per tier on create (402 over limit); `Account.plan_name` set at subscription.
- [x] **3.9 (M)** **Trial & suspension enforcement** — `require_active_account` gates mutations (402 on `on_hold` or expired trial); webhook drives the state.

### Phase 4 — Integrations (depends on 2, 3)
> Externals can't be hit from the dev box, so live calls are structured behind mockable
> clients and **unit-tested offline** (mock HTTP transport / patched SDK). Marked `[~]` where
> the live path still needs a CI integration test against real Facebook/Stripe sandboxes.
- [x] **4.1 (M)** Facebook OAuth `connect`/`callback`; **token encryption at rest** (Fernet, `app/core/crypto.py`) wired into social + ad-account saves and the OAuth page import.
- [~] **4.2 (L)** Graph **client** (`integrations/facebook/graph.py`), pinned to v19.0: ad accounts, campaigns (+create), adsets, ads, targeting & geo search. Request/parse unit-tested; live calls need sandbox creds.
- [~] **4.3 (L)** Graph social: pages, page posts, create post, post & page insights. Unit-tested via mock transport.
- [~] **4.4 (M)** Instagram Graph: business-account lookup, media list, container create + publish.
- [x] **4.5 (M)** Stripe service: customer + subscription creation (`integrations/stripe_service.py`).
- [x] **4.6 (S)** Stripe **webhook** → drives Account billing state (also advances **3.9** lifecycle: paid/`on_hold`).
- [x] **4.7 (M)** **Media upload** (`POST /media/upload`) → pluggable object storage (local FS / S3-MinIO, lazy boto3).
- [x] **4.8 (S)** Facebook **lead-gen sync** — `POST /social/{id}/sync-leads` imports `/leadgen_forms` submissions into the Leads table (dedupe by email); "Sync Facebook leads" button in the UI.

### Phase 5 — Background jobs (depends on 3, 4)
- [x] **5.1 (S)** arq `WorkerSettings` (`app/worker/`), Redis from `REDIS_URL`; run via `arq app.worker.settings.WorkerSettings`. Cron sweep enqueues due posts.
- [x] **5.2 (M)** Scheduled-post **Queue**: `ScheduledPost` table + tenant-scoped CRUD; `select_due_posts` sweep → `publish_scheduled_post` job → Graph publish (FB feed + IG two-step); status tracking. Publish/selection are **pure functions, unit-tested without Redis**.
- [x] **5.3 (S)** Retries + **dead-letter** (`attempts`/`MAX_ATTEMPTS` → `failed`), `last_error` captured, status/attempts surfaced via `GET` for the Queue UI.

### Phase 6 — Frontend foundation (depends on 3 for the client)
- [x] **6.1 (S)** Next.js 14 (App Router) + TypeScript (strict) + ESLint. `npm run build` passes (tsc + lint clean). Pinned to patched `next@14.2.35`.
- [x] **6.2 (S)** Tailwind + a small base UI kit (`components/ui.tsx`: Button/Input/Card/Field) + portal shell (sidebar nav + header).
- [x] **6.3 (S)** **Typed API client** generated from the backend OpenAPI dump via `openapi-typescript` (`npm run gen:api` → `src/lib/api-types.ts`); data layer on **TanStack Query**.
- [x] **6.4 (M)** Auth wiring: login/register/logout + `useCurrentUser`; **BFF proxy** (`next.config` rewrites `/api/*` → FastAPI) so httpOnly cookies stay first-party; `middleware.ts` guards `/portal/*`.

### Phase 7 — Frontend: marketing site (depends on 6)
- [x] **7.1 (S)** Home/Index — hero + feature highlights under a shared `(marketing)` layout (header nav + footer).
- [x] **7.2 (S)** About + Features (8 original bullets) + Contact (form).
- [x] **7.3 (M)** Pricing (3 real tiers $24/$99/$299) → `/checkout?plan=` → **Stripe Elements** card capture (subscription with `default_incomplete` → PaymentIntent `client_secret` → `<PaymentElement>` → `/success`).
- Verified: `next build` (13 routes), `tsc --noEmit` clean, `next lint` clean.

### Phase 8 — Frontend: portal (depends on 6, and matching API/integration phases)
> The screens backed by **existing REST endpoints** are built and verified.
> **Backend unblock done:** Graph-proxy routes + team endpoints now exist (see *8.b* below),
> so the remaining live-Graph + team **frontend screens** are unblocked (UI is the remaining work).

> **8.b — Backend unblock (done):** added tenant-scoped Graph-proxy routes — `GET/POST
> /social/{id}/posts`, `GET /social/{id}/insights`, `GET/POST /ad-accounts/{id}/campaigns`,
> `GET /ad-accounts/{id}/adsets|ads` (decrypt token → `GraphClient`, injectable for tests) — and
> **team management**: `GET/POST /team`, `DELETE /team/{id}`, `GET/POST /roles` (Owner-gated).
> 67 backend tests pass; OpenAPI now 36 paths; frontend types regenerated.
- [x] **8.1 (S)** App shell: sidebar nav + header + logout; settings sub-nav.
- [x] **8.2 (S)** Dashboard — real data (lead/connection/ad-account/scheduled counts + account/trial status).
- [x] **8.3 (M)** Leads — list, **search**, create, edit, delete (full CRUD on `/leads`).
- [x] **8.4 (L)** Settings — General/Business, Connections (Connect Facebook OAuth + disconnect), Billing, **Team** (members table, invite, remove, roles list/create) wired to `/team` + `/roles`.
- [x] **8.5 (L)** Social/Facebook — **Queue** + **Pages** (per-page: create-post, recent posts, insights) on the Graph-proxy routes; graceful "needs live connection" states.
- [x] **8.6 (M)** Social/Instagram — Queue (platform-aware) + the Pages content view works for IG connections too (same endpoints).
- [x] **8.7 (L)** Ads/Facebook — campaigns list/create **+ full ad-set/ad creation wizard**: budget, age, countries, **interest targeting search**, page-creative ad builder (backend `create_adset`/`create_ad` + targeting/location search routes).
- Verified: `next build` (**24 routes**), `tsc --noEmit` clean, `next lint` clean.

### Phase 9 — Cutover
> Artifacts are built and verified; **execution requires your environment** (real DB,
> rotated secrets, deploy target). Full runbook: [`CUTOVER.md`](CUTOVER.md).
- [A] **9.1 (M)** ETL script `backend/scripts/etl_sqlserver_to_postgres.py` — maps all legacy/Identity tables, **preserves password hashes**, **encrypts tokens at load**, keeps GUIDs + Numeric(18,2); `--dry-run` + row-count verification. (compiles + ruff clean; run against real DBs at cutover.)
- [A] **9.2 (M)** `backend/Dockerfile` + `frontend/Dockerfile` (Next standalone) + `infra/docker-compose.prod.yml` (db/redis/api/worker/frontend, api runs migrations) + `infra/.env.production.example` (consolidated, rotated-secret manifest).
- [A] **9.3 (S)** Parity QA smoke-test checklist (CUTOVER §3).
- [A] **9.4 (S)** DNS cutover + **rollback** plan — old DB is read-only during ETL, so rollback is a DNS repoint (CUTOVER §4).
- [A] **9.5 (S)** Decommission steps + `legacy-dotnet-final` tag guidance (CUTOVER §5).

*Legend: [x] done · [~] partial · [A] artifact ready, execution pending your environment.*

---

## Critical-path / risk callouts
1. **Auth password port (2.2)** — gates a no-reset migration; validate against real hashes early.
2. **Facebook Graph rewrite (4.2–4.4)** — biggest unknown; v10→current has breaking changes. Spike first.
3. **Data ETL (1.4)** — money/decimal precision and string-ID preservation.
4. **Scheduled posts (5.2)** — net-new infra Blazor Server hid behind its live connection.
5. **Secrets (0.6)** — rotate before anything else touches Facebook/Stripe.

## Suggested build order
0 → 1 → 2 → 3 → 4 → 5 (backend complete & testable via OpenAPI) → 6 → 7/8 in parallel → 9.
Frontend can start at Phase 6 as soon as the relevant routers exist.

---

## Completeness audit (vs. business analysis)
Cross-checked the backlog against `BUSINESS_PLAN.md` and the full feature inventory. The original plan covered the controllers, models, auth, integrations, jobs, and all screens. The following **gaps were found and added**:
- **0.7** Object storage for media (images/videos/creatives/logos) — was implicit, now explicit.
- **0.8** Observability + rate limiting — operational baseline was missing.
- **3.8** Plan/quota enforcement — the per-tier limits (seats, social sets, post/ad caps) are a core monetization feature and weren't tasked.
- **3.9** Trial & subscription lifecycle — trial expiry / `OnHold` / suspension state machine wasn't tasked.
- **4.7** Media upload pipeline — the `AdImage`/`SocialImages`/`SaveFile` flows needed an explicit task.
- **4.8** Optional Facebook lead-gen forms sync — clarifies the Leads data source.

With these additions the backlog is considered **feature-complete** against the current app. Remaining open scope questions are product decisions (e.g. whether 2FA — 2.8 — ships in v1), not missing tasks.

### Parity port (delete-readiness audit)
A line-by-line audit of the .NET code surfaced 6 features that lived **only** in the old app; all are now ported (backend + frontend, tested):
1. **FB photo/video posting** (`/photos`, graph-video `/videos`) — wired into posts + the scheduler.
2. **FB ad images** (`/adimages` → `image_hash`) in the ad creative builder.
3. **Instagram insights** endpoint + UI.
4. **Lead-gen sync** into the Leads table.
5. **Change email** + **GDPR export/delete**.
6. **Media library** (`MediaAsset` + browse grid).
Verified non-gaps: external/social login (only cookie auth was configured) and a real scheduler (the old "Queue" had none — the new arq worker is a net gain). **The `Sohi.*` projects can be deleted once the new app is deployed & validated** (tag `legacy-dotnet-final` first).
