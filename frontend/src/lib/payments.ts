import { apiFetch } from "./api";

export interface SubscriptionResult {
  subscription_id: string;
  status: string;
}

export function createSubscription(priceId: string): Promise<SubscriptionResult> {
  return apiFetch<SubscriptionResult>("/payments/create-subscription", {
    method: "POST",
    body: JSON.stringify({ price_id: priceId }),
  });
}
