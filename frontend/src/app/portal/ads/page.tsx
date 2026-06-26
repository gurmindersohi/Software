"use client";
import { useQuery } from "@tanstack/react-query";

import { Empty, PageHeader } from "@/components/portal";
import { Card } from "@/components/ui";
import { listAdAccounts } from "@/lib/connections";

export default function AdsPage() {
  const ads = useQuery({ queryKey: ["ad-accounts"], queryFn: listAdAccounts });

  return (
    <div>
      <PageHeader title="Ad accounts" />

      {ads.isLoading && <p className="text-slate-500">Loading…</p>}
      {ads.data && ads.data.length === 0 && (
        <Empty
          message="No ad accounts connected."
          cta={{ href: "/portal/settings/connections", label: "Connect Facebook" }}
        />
      )}

      {ads.data && ads.data.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {ads.data.map((account) => (
            <Card key={account.id}>
              <p className="font-medium text-slate-800">{account.name ?? "Ad account"}</p>
              <p className="text-sm text-slate-500">{account.user_account_id}</p>
            </Card>
          ))}
        </div>
      )}

      <p className="mt-6 text-sm text-slate-400">
        Campaign / ad-set / ad management (live Graph data) is the next slice — it needs the
        backend Graph-proxy read endpoints (see MIGRATION_PLAN 8.7).
      </p>
    </div>
  );
}
