# Sohi Frontend (Next.js)

Next.js (App Router) + TypeScript + Tailwind. Talks to the FastAPI backend.

## Quickstart
```bash
cp .env.local.example .env.local      # point BACKEND_URL at the API
npm install
npm run gen:api                       # regenerate typed client from openapi.json
npm run dev                           # http://localhost:3000  (backend on :8000)
```

## How it fits together
- **BFF proxy** — `next.config.mjs` rewrites `/api/*` to the backend. The browser
  only ever calls the same origin, so the backend's **httpOnly auth cookies**
  pass through as first-party (SameSite=Lax friendly). No tokens in JS.
- **Auth** — `src/lib/auth.ts` (login/register/logout/me); `src/middleware.ts`
  guards `/portal/*` by checking the `access_token` cookie.
- **Data** — TanStack Query (`src/app/providers.tsx`, `src/lib/hooks.ts`).
- **Types** — `npm run gen:api` runs `openapi-typescript` over `openapi.json`
  (dumped from FastAPI) into `src/lib/api-types.ts`. Re-dump the spec with:
  `cd ../backend && python -c "import json,app.main as m; json.dump(m.app.openapi(), open('../frontend/openapi.json','w'), indent=2)"`

## Scripts
`dev` · `build` · `start` · `lint` · `typecheck` · `gen:api`

## Routes
- `/` landing (full marketing site = Phase 7)
- `/login`, `/register`
- `/portal` dashboard (Leads/Social/Ads/Settings screens = Phase 8)
