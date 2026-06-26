"use client";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";

import { Button, Card } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { useCurrentUser } from "@/lib/hooks";
import { createSubscription } from "@/lib/payments";
import { getPlan, priceIdFor } from "@/lib/plans";

export default function CheckoutInner() {
  const params = useSearchParams();
  const router = useRouter();
  const plan = getPlan(params.get("plan") ?? "");
  const { data: user, isLoading } = useCurrentUser();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!plan) {
    return (
      <Card>
        <p className="text-slate-700">
          Unknown plan.{" "}
          <Link href="/pricing" className="text-brand hover:underline">
            Back to pricing
          </Link>
          .
        </p>
      </Card>
    );
  }

  if (isLoading) return <p className="text-slate-500">Loading…</p>;

  if (!user) {
    return (
      <Card>
        <p className="text-slate-700">
          Please{" "}
          <Link href="/register" className="text-brand hover:underline">
            create an account
          </Link>{" "}
          or{" "}
          <Link href="/login" className="text-brand hover:underline">
            sign in
          </Link>{" "}
          to subscribe to {plan.name}.
        </p>
      </Card>
    );
  }

  async function subscribe() {
    if (!plan) return;
    setError(null);
    const priceId = priceIdFor(plan.id);
    if (!priceId) {
      setError("Billing is not configured yet. Please contact support.");
      return;
    }
    setBusy(true);
    try {
      await createSubscription(priceId);
      router.push("/success");
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Could not start subscription.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <Card>
      <h1 className="text-2xl font-bold text-slate-900">Subscribe to {plan.name}</h1>
      <p className="mt-2 text-3xl font-bold text-slate-900">
        ${plan.price}
        <span className="text-base font-normal text-slate-500">/mo</span>
      </p>
      <ul className="mt-4 space-y-1 text-sm text-slate-600">
        {plan.features.map((feature) => (
          <li key={feature}>• {feature}</li>
        ))}
      </ul>
      {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
      <Button onClick={subscribe} disabled={busy} className="mt-6 w-full">
        {busy ? "Starting…" : `Subscribe — $${plan.price}/mo`}
      </Button>
      <p className="mt-3 text-xs text-slate-400">
        Secure card entry via Stripe Elements is wired during cutover (Phase 9).
      </p>
    </Card>
  );
}
