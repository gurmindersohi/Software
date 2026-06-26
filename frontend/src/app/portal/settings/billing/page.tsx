"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";

import { Button, Card } from "@/components/ui";
import { getBilling, getUsage, type UsageItem } from "@/lib/account";
import { ApiError } from "@/lib/api";
import { openBillingPortal } from "@/lib/payments";

function UsageBar({ label, item }: { label: string; item: UsageItem }) {
  const pct = item.limit ? Math.min(100, Math.round((item.used / item.limit) * 100)) : 0;
  return (
    <div>
      <div className="flex justify-between text-sm">
        <span className="text-slate-600">{label}</span>
        <span className="text-slate-500">
          {item.used} / {item.limit ?? "∞"}
        </span>
      </div>
      {item.limit !== null && (
        <div className="mt-1 h-2 rounded-full bg-slate-100">
          <div
            className={"h-2 rounded-full " + (pct >= 100 ? "bg-red-500" : "bg-brand")}
            style={{ width: `${pct}%` }}
          />
        </div>
      )}
    </div>
  );
}

export default function BillingSettingsPage() {
  const billing = useQuery({ queryKey: ["billing"], queryFn: getBilling });
  const usage = useQuery({ queryKey: ["usage"], queryFn: getUsage });
  const [busy, setBusy] = useState(false);

  async function manage() {
    setBusy(true);
    try {
      const { url } = await openBillingPortal();
      window.location.href = url;
    } catch (e) {
      alert(
        e instanceof ApiError && e.status === 409
          ? "Subscribe to a plan first."
          : "Billing portal isn't available yet.",
      );
      setBusy(false);
    }
  }

  if (billing.isLoading) return <p className="text-slate-500">Loading…</p>;
  const paid = billing.data?.is_account_paid;

  return (
    <div className="max-w-lg space-y-6">
      <Card className="space-y-4">
        <div>
          <p className="text-sm text-slate-500">Status</p>
          <p className="text-lg font-semibold text-slate-900">
            {paid ? "Active subscription" : billing.data?.on_hold ? "On hold" : "Trial / free"}
          </p>
        </div>
        {billing.data?.trial_expiry && (
          <p className="text-sm text-slate-600">
            Trial expires {new Date(billing.data.trial_expiry).toLocaleDateString()}
          </p>
        )}
        <div className="flex gap-3">
          {billing.data?.customer_id ? (
            <Button onClick={manage} disabled={busy}>
              {busy ? "Opening…" : "Manage billing"}
            </Button>
          ) : (
            <Link href="/pricing">
              <Button>Choose a plan</Button>
            </Link>
          )}
        </div>
      </Card>

      {usage.data && (
        <Card className="space-y-4">
          <h2 className="text-lg font-semibold text-slate-800">
            Usage <span className="text-sm font-normal text-slate-400">({usage.data.plan})</span>
          </h2>
          <UsageBar label="Team seats" item={usage.data.seats} />
          <UsageBar label="Connected pages" item={usage.data.social_sets} />
          <UsageBar label="Scheduled posts" item={usage.data.scheduled_posts} />
        </Card>
      )}
    </div>
  );
}
