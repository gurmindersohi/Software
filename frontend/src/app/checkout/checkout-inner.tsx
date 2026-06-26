"use client";
import {
  Elements,
  PaymentElement,
  useElements,
  useStripe,
} from "@stripe/react-stripe-js";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { useCurrentUser } from "@/lib/hooks";
import { createSubscription } from "@/lib/payments";
import { getPlan, priceIdFor } from "@/lib/plans";
import { stripePromise } from "@/lib/stripe";

function PaymentForm() {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function pay(e: React.FormEvent) {
    e.preventDefault();
    if (!stripe || !elements) return;
    setBusy(true);
    setError(null);
    const result = await stripe.confirmPayment({
      elements,
      confirmParams: { return_url: `${window.location.origin}/success` },
    });
    if (result.error) {
      setError(result.error.message ?? "Payment failed.");
      setBusy(false);
    }
  }

  return (
    <form onSubmit={pay} className="mt-6 space-y-4">
      <PaymentElement />
      {error && <ErrorNote>{error}</ErrorNote>}
      <Button type="submit" disabled={!stripe || busy} className="w-full">
        {busy ? "Processing…" : "Pay & subscribe"}
      </Button>
    </form>
  );
}

export default function CheckoutInner() {
  const params = useSearchParams();
  const plan = getPlan(params.get("plan") ?? "");
  const { data: user, isLoading } = useCurrentUser();
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

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

  async function start() {
    if (!plan) return;
    setError(null);
    const priceId = priceIdFor(plan.id);
    if (!priceId || !stripePromise) {
      setError("Billing isn't configured yet. Please contact support.");
      return;
    }
    setBusy(true);
    try {
      const res = await createSubscription(priceId, plan.id);
      if (!res.client_secret) {
        setError("Could not start payment.");
        return;
      }
      setClientSecret(res.client_secret);
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

      {!clientSecret ? (
        <>
          {error && <ErrorNote>{error}</ErrorNote>}
          <Button onClick={start} disabled={busy} className="mt-6 w-full">
            {busy ? "Starting…" : "Continue to payment"}
          </Button>
        </>
      ) : stripePromise ? (
        <Elements stripe={stripePromise} options={{ clientSecret }}>
          <PaymentForm />
        </Elements>
      ) : null}
    </Card>
  );
}
