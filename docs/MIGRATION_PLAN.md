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
- [ ] **0.1 (S)** Monorepo scaffold (`/backend`, `/frontend`, `/infra`, `/docs`), root README, `.gitignore`, license.
- [ ] **0.2 (S)** Backend project: `uv`/`poetry`, FastAPI app skeleton, settings via `pydantic-settings` + `.env`, health endpoint.
- [ ] **0.3 (S)** `docker-compose` for local Postgres + Redis.
- [ ] **0.4 (S)** Lint/format/test tooling: `ruff`, `mypy`, `pytest`; pre-commit hooks.
- [ ] **0.5 (S)** CI pipeline (lint + test for backend & frontend).
- [ ] **0.6 (S)** **Rotate** Facebook + Stripe secrets; set up secret management; document required env vars.
- [ ] **0.7 (S)** Object storage (S3-compatible; MinIO in compose) for media — post images/videos, ad creatives, account logos. *(gap found in audit)*
- [ ] **0.8 (S)** Observability baseline: structured logging, error tracking (e.g. Sentry), and API rate limiting. *(gap found in audit)*

### Phase 1 — Data layer
- [ ] **1.1 (M)** Port 19 domain models → SQLModel tables; decide UUID vs string keys per entity.
- [ ] **1.2 (S)** Alembic init + initial migration generating the full schema.
- [ ] **1.3 (M)** Map SQL Server schema → Postgres types (decimal precision, datetime, bool, string IDs).
- [ ] **1.4 (M)** One-time **ETL** script: SQL Server → Postgres (preserve IDs, FK integrity, money precision).
- [ ] **1.5 (S)** Seed/reference data (Plans, etc.) + a smoke test verifying row counts vs source.

### Phase 2 — Auth (depends on 1)
- [ ] **2.1 (M)** Integrate **FastAPI-Users**; User model unifying the two old user tables.
- [ ] **2.2 (M)** **Port ASP.NET Identity PBKDF2** verifier so legacy password hashes validate; re-hash on next login.
- [ ] **2.3 (S)** JWT access + refresh in **httpOnly cookies**; logout/refresh endpoints.
- [ ] **2.4 (S)** Register + email-confirmation flow.
- [ ] **2.5 (S)** Forgot/reset password flow.
- [ ] **2.6 (S)** Pick + wire **email provider** (Resend/SendGrid); templates.
- [ ] **2.7 (M)** Account management: change/set password, change email, delete/export personal data.
- [ ] **2.8 (M)** *Optional/deferrable:* 2FA (TOTP) + recovery codes (port from Identity).
- [ ] **2.9 (S)** Roles/permissions model (ManageUsers, Roles screens depend on this).

### Phase 3 — Core API (depends on 1, 2)
Port each controller+repository to a FastAPI router with Pydantic schemas + tests:
- [ ] **3.1 (S)** Accounts router (get/create/update/get-by-name).
- [ ] **3.2 (S)** Settings router (get/update account).
- [ ] **3.3 (M)** Leads router (list/get/create/update/delete/search).
- [ ] **3.4 (S)** Social tokens router (save/get-by-platform/list/delete).
- [ ] **3.5 (S)** Ads accounts router (list/save).
- [ ] **3.6 (S)** Billing router (update plan/subscription).
- [ ] **3.7 (S)** Wire authz (ownership/role checks) + **multi-tenant account scoping** across routers.
- [ ] **3.8 (M)** **Plan/quota enforcement** — per-plan limits (seats, social sets, scheduled posts, ad allowance) enforced at the API. *(gap found in audit)*
- [ ] **3.9 (M)** **Trial & subscription lifecycle** — trial expiry, `OnHold`/suspension + reactivation, paid/unpaid state (ties to Stripe webhooks 4.6). *(gap found in audit)*

### Phase 4 — Integrations (depends on 2, 3)
- [ ] **4.1 (M)** Facebook OAuth connect flow (redirect URIs now hit FastAPI); store tokens **encrypted at rest**.
- [ ] **4.2 (L)** Facebook Graph **client service**, upgraded v10 → current: ad accounts, campaigns, adsets, ads, targeting, locations, images.
- [ ] **4.3 (L)** Facebook Graph for Social: pages, posts, create post, insights, analytics, images, videos.
- [ ] **4.4 (M)** Instagram Graph: posts, insights, publishing.
- [ ] **4.5 (M)** Stripe service: customer + subscription creation (port Checkout logic).
- [ ] **4.6 (S)** Stripe **webhook** endpoint (subscription lifecycle → Billing).
- [ ] **4.7 (M)** **Media upload pipeline** → object storage; used by social posts and ad creatives (replaces `AdImage`/`SocialImages`/`SaveFile`/logo flows). *(gap found in audit)*
- [ ] **4.8 (S)** *Optional:* Facebook **lead-gen forms** sync into Leads (if that's the current lead source). *(gap found in audit)*

### Phase 5 — Background jobs (depends on 3, 4)
- [ ] **5.1 (S)** arq worker + Redis wiring; deployment of the worker process.
- [ ] **5.2 (M)** Scheduled-post **Queue**: enqueue, schedule, publish via Graph, status tracking, retries.
- [ ] **5.3 (S)** Failure handling / dead-letter + admin visibility.

### Phase 6 — Frontend foundation (depends on 3 for the client)
- [ ] **6.1 (S)** Next.js (App Router) scaffold, TypeScript, ESLint/Prettier.
- [ ] **6.2 (S)** UI kit choice (e.g. Tailwind + shadcn/ui) + base layout/theme matching current look.
- [ ] **6.3 (S)** **Generate typed API client** from FastAPI OpenAPI (`orval`/`openapi-typescript`); data layer (TanStack Query).
- [ ] **6.4 (M)** Auth wiring: login/register/logout, cookie/session handling, route middleware, protected layouts.

### Phase 7 — Frontend: marketing site (depends on 6)
- [ ] **7.1 (S)** Home/Index. **7.2 (S)** About + Features + Contact. **7.3 (M)** Pricing + Stripe Checkout + Success.

### Phase 8 — Frontend: portal (depends on 6, and matching API/integration phases)
- [ ] **8.1 (S)** App shell: nav, menus, layouts.
- [ ] **8.2 (S)** Dashboard.
- [ ] **8.3 (M)** Leads (list, details, edit, search).
- [ ] **8.4 (L)** Settings area: General, Business, Accounts/Connect, Facebook/Instagram connect, ManageUsers/NewUser/Roles, Ads account connect.
- [ ] **8.5 (L)** Social/Facebook: Pages, Posts, CreatePost, Queue, Insights, Analytics, Images, Settings.
- [ ] **8.6 (M)** Social/Instagram: Posts, Queue, Insights.
- [ ] **8.7 (L)** Ads/Facebook: Campaigns/list, Adsets, Ads, Create flow (NewCampaign/NewAdSet/NewAd), targeting/location/age/images, Manage.

### Phase 9 — Cutover
- [ ] **9.1 (M)** Final data migration dry-run + verification.
- [ ] **9.2 (M)** Infra/deploy: Postgres host, containerized FastAPI + worker, Next.js (Vercel or container), Redis, secrets.
- [ ] **9.3 (S)** End-to-end parity QA against the old app.
- [ ] **9.4 (S)** DNS cutover + rollback plan.
- [ ] **9.5 (S)** Decommission .NET app; archive old repo state.

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
