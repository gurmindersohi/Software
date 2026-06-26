"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";

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
            <Link key={account.id} href={`/portal/ads/${account.id}`}>
              <Card className="transition hover:border-brand">
                <p className="font-medium text-slate-800">{account.name ?? "Ad account"}</p>
                <p className="text-sm text-slate-500">{account.user_account_id}</p>
                <p className="mt-2 text-xs text-brand">Manage campaigns →</p>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
