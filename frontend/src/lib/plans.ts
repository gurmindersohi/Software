// Subscription tiers — mirrors the original pricing page. Stripe price IDs are
// injected at build time (price IDs are not secret) and consumed at checkout.
export type PlanId = "basic" | "premium" | "unlimited";

export interface PlanTier {
  id: PlanId;
  name: string;
  price: number;
  features: string[];
  highlighted?: boolean;
}

export const PLANS: PlanTier[] = [
  {
    id: "basic",
    name: "Basic",
    price: 24,
    features: ["1 user", "3 social sets", "30 scheduled posts", "30 posts per profile", "Email support"],
  },
  {
    id: "premium",
    name: "Premium",
    price: 99,
    highlighted: true,
    features: [
      "3 users",
      "5 social sets",
      "70 scheduled posts",
      "70 posts per profile",
      "3 Facebook ads",
      "Email support",
    ],
  },
  {
    id: "unlimited",
    name: "Unlimited",
    price: 299,
    features: [
      "Unlimited users",
      "Unlimited social sets",
      "Unlimited scheduled posts",
      "Unlimited posts",
      "Unlimited Facebook ads",
      "Priority email support",
    ],
  },
];

export function getPlan(id: string): PlanTier | undefined {
  return PLANS.find((p) => p.id === id);
}

export function priceIdFor(id: PlanId): string | undefined {
  const map: Record<PlanId, string | undefined> = {
    basic: process.env.NEXT_PUBLIC_STRIPE_PRICE_BASIC,
    premium: process.env.NEXT_PUBLIC_STRIPE_PRICE_PREMIUM,
    unlimited: process.env.NEXT_PUBLIC_STRIPE_PRICE_UNLIMITED,
  };
  return map[id];
}
