# Business Plan — Social Media & Ads Manager (codename "Sohi")

> Derived from analysis of the existing application (marketing site, pricing tiers, data model, and feature set). This documents what the software is trying to do and frames it as a business.

## 1. Executive summary

**Sohi is a B2B SaaS platform that lets businesses and agencies manage their Facebook & Instagram presence — both organic social and paid advertising — from a single dashboard.** Users connect their social accounts and ad accounts, then create and schedule posts across multiple profiles, monitor real-time insights, and build/launch/manage Facebook & Instagram ad campaigns (with reusable saved audiences) — without juggling Meta's native tools.

The product already exists as a working .NET application (Blazor + Web API) and is being re-platformed to a modern Python (FastAPI) + Next.js stack (see `MIGRATION_PLAN.md`). This document captures the business model the software encodes.

## 2. Problem

Small businesses, marketers, and agencies that run social + paid campaigns on Meta face:
- **Tool sprawl** — separate flows for posting (Meta Business Suite) and ads (Ads Manager), each complex and built for power users.
- **Multi-account pain** — agencies manage many client pages/ad accounts and must context-switch constantly.
- **No reuse** — audiences, post templates, and creatives are rebuilt repeatedly.
- **Scheduling gaps** — coordinating a content calendar across Facebook and Instagram is manual.

## 3. Product & value proposition

**One dashboard for organic + paid Meta marketing.** Core capabilities (already built into the app):

| Pillar | Capabilities |
|---|---|
| **Social management** | Create posts on multiple accounts in one click; schedule posts (Facebook + Instagram) via a queue; manage connected pages/profiles; upload images & videos |
| **Insights** | Real-time page & post insights/analytics |
| **Advertising** | Create Facebook/Instagram ads — campaigns, ad sets, ads; build & **reuse targeted audiences** (detailed targeting, location, age); manage ad accounts; real-time ad insights |
| **Team / agency** | Multi-user accounts, user roles & management, per-account limits |
| **Billing** | Free trial → subscription (Stripe), tiered plans |

**Value prop:** *"Run your social media and ads in one place — post, schedule, target, and measure across Facebook and Instagram, with reusable audiences and team access."*

## 4. Target customers

- **SMBs** running their own Facebook/Instagram marketing who find Meta's native tools overwhelming.
- **Marketing agencies / freelancers** managing multiple client accounts (the multi-user, multi-account model and "Unlimited" tier point squarely at this segment).
- **In-house marketing teams** needing collaboration and a shared content/ads workflow.

## 5. Business model & pricing

Subscription SaaS with a **free trial** (account has `TrialExpiry`, `IsAccountPaid`, `OnHold` states) converting to paid plans billed via **Stripe**. Current tiers (from the live pricing page):

| Plan | Price | Users | Social sets | Scheduled posts | Ads | Support |
|---|---|---|---|---|---|---|
| **Basic** | $24/mo | 1 | 3 | 30 | — | Email |
| **Premium** | $99/mo | 3 | 5 | 70 | 3 Facebook ads | Email |
| **Unlimited** | $299/mo | Unlimited | Unlimited | Unlimited | Unlimited | Priority email |

**Monetization levers:** seat count, number of connected social sets, scheduled-post volume, and ad-campaign allowance — enforced per plan. Annual billing and add-ons are natural future levers.

## 6. Competitive landscape

- **Broad schedulers:** Hootsuite, Buffer, Sprout Social, Later — strong on organic scheduling/analytics, **weaker on integrated ad creation**.
- **Ad-centric tools:** AdEspresso, Revealbot — strong on ads, light on organic.
- **Meta native:** Business Suite + Ads Manager — free but fragmented and complex.

**Differentiation:** Sohi combines organic scheduling **and** ad campaign creation with reusable audiences in one affordable, agency-friendly tool — a niche between "just scheduling" and "just ads."

## 7. Go-to-market (proposed)

- **Self-serve PLG:** free trial → in-product conversion; pricing page already supports this.
- **Agency channel:** target agencies with the Unlimited tier (multi-client, unlimited seats).
- **Content/SEO:** how-to content on Meta advertising; integrations directory presence.
- **Roadmap-led expansion:** the codebase already hints at **Google Ads** and **LinkedIn Ads** (commented-out plan features) as expansion surfaces.

## 8. Product roadmap (business view)

1. **Re-platform** to FastAPI + Next.js (current effort) — stability, velocity, modern UX.
2. **Harden Meta integration** — upgrade Graph API (current version), reliability of scheduled publishing.
3. **Plan/quota enforcement & billing lifecycle** — trials, dunning, upgrades/downgrades.
4. **Expand channels** — Google Ads, LinkedIn Ads (already signposted in pricing).
5. **Analytics depth** — cross-channel reporting, exportable client reports (agency value).

## 9. Key risks

- **Platform dependency** — Meta Graph API changes, app review, and rate limits are existential; must track API versions and permissions (the app is currently on a deprecated Graph version).
- **Compliance** — handling user tokens and personal data (encryption at rest, GDPR export/delete).
- **Competition & pricing pressure** from well-funded incumbents.
- **Single-channel concentration** — mitigated by the Google/LinkedIn expansion roadmap.

## 10. How the software maps to the business (traceability)

| Business capability | Where it lives in code |
|---|---|
| Multi-tenant accounts, trials, billing state | `Account` model (`TrialExpiry`, `IsAccountPaid`, `OnHold`, `CustomerId`, `SubscriptionId`) |
| Tiered plans | `Plan` model + Pricing page; Stripe in Checkout |
| Social connections | `SocialMedia` / `Profile` (tokens per page/profile) |
| Posting & scheduling | Social/Facebook + Instagram pages, **Queue** |
| Insights | `PageInsights`, `PostInsights` |
| Ads | `Campaign`, `Adset`, `Ad`, `AdAccount`, `Targeting`, `AdImage`, `FacebookLocation` |
| Leads | `Lead` model + Leads portal |
| Teams | Multi-user accounts, ManageUsers, Roles |

---
*See `MIGRATION_PLAN.md` for the technical re-platforming backlog.*
