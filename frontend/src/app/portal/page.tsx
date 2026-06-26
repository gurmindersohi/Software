"use client";
import { useQuery } from "@tanstack/react-query";

import { Card } from "@/components/ui";
import { getAccount } from "@/lib/account";
import { listAdAccounts, listSocialConnections } from "@/lib/connections";
import { useCurrentUser } from "@/lib/hooks";
import { listLeads } from "@/lib/leads";
import { listScheduledPosts } from "@/lib/scheduled";

function Stat({ label, value }: { label: string; value: number | string }) {
  return (
    <Card>
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-1 text-3xl font-bold text-slate-900">{value}</p>
    </Card>
  );
}

export default function DashboardPage() {
  const { data: user } = useCurrentUser();
  const account = useQuery({ queryKey: ["account"], queryFn: getAccount });
  const leads = useQuery({ queryKey: ["leads"], queryFn: () => listLeads() });
  const social = useQuery({ queryKey: ["social"], queryFn: listSocialConnections });
  const ads = useQuery({ queryKey: ["ad-accounts"], queryFn: listAdAccounts });
  const scheduled = useQuery({ queryKey: ["scheduled"], queryFn: listScheduledPosts });

  const trial = account.data?.trial_expiry
    ? new Date(account.data.trial_expiry).toLocaleDateString()
    : null;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900">
        Welcome{user?.first_name ? `, ${user.first_name}` : ""}
      </h1>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Stat label="Leads" value={leads.data?.total ?? "—"} />
        <Stat label="Connected pages" value={social.data?.length ?? "—"} />
        <Stat label="Ad accounts" value={ads.data?.length ?? "—"} />
        <Stat label="Scheduled posts" value={scheduled.data?.total ?? "—"} />
      </div>

      <Card className="max-w-lg">
        <h2 className="mb-2 text-lg font-semibold text-slate-800">
          {account.data?.account_name ?? "Your account"}
        </h2>
        <dl className="space-y-1 text-sm text-slate-600">
          <div className="flex justify-between">
            <dt>Plan status</dt>
            <dd>{account.data?.is_account_paid ? "Paid" : "Trial / free"}</dd>
          </div>
          {trial && (
            <div className="flex justify-between">
              <dt>Trial expires</dt>
              <dd>{trial}</dd>
            </div>
          )}
          <div className="flex justify-between">
            <dt>On hold</dt>
            <dd>{account.data?.on_hold ? "Yes" : "No"}</dd>
          </div>
        </dl>
      </Card>
    </div>
  );
}
