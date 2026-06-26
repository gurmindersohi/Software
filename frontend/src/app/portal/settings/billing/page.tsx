"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";

import { Button, Card } from "@/components/ui";
import { getBilling } from "@/lib/account";

export default function BillingSettingsPage() {
  const { data: billing, isLoading } = useQuery({ queryKey: ["billing"], queryFn: getBilling });

  if (isLoading) return <p className="text-slate-500">Loading…</p>;

  const paid = billing?.is_account_paid;

  return (
    <Card className="max-w-lg space-y-4">
      <div>
        <p className="text-sm text-slate-500">Status</p>
        <p className="text-lg font-semibold text-slate-900">
          {paid ? "Active subscription" : billing?.on_hold ? "On hold" : "Trial / free"}
        </p>
      </div>
      <dl className="space-y-1 text-sm text-slate-600">
        <div className="flex justify-between">
          <dt>Customer</dt>
          <dd>{billing?.customer_id ?? "—"}</dd>
        </div>
        <div className="flex justify-between">
          <dt>Subscription</dt>
          <dd>{billing?.subscription_id ?? "—"}</dd>
        </div>
        {billing?.trial_expiry && (
          <div className="flex justify-between">
            <dt>Trial expires</dt>
            <dd>{new Date(billing.trial_expiry).toLocaleDateString()}</dd>
          </div>
        )}
      </dl>
      {!paid && (
        <Link href="/pricing">
          <Button>Choose a plan</Button>
        </Link>
      )}
    </Card>
  );
}
