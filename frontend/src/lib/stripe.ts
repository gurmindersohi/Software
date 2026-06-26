import { loadStripe, type Stripe } from "@stripe/stripe-js";

// Publishable key is not secret; exposed at build time. Null when unconfigured
// (the checkout degrades gracefully).
const key = process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY;

export const stripePromise: Promise<Stripe | null> | null = key ? loadStripe(key) : null;
