"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { PageHeader } from "@/components/portal";
import { Card } from "@/components/ui";
import {
  adminMetrics,
  listAdminAccounts,
  suspendAccount,
  unsuspendAccount,
} from "@/lib/admin";

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <Card>
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-1 text-3xl font-bold text-slate-900">{value}</p>
    </Card>
  );
}

export default function AdminPage() {
  const queryClient = useQueryClient();
  const metrics = useQuery({ queryKey: ["admin-metrics"], queryFn: adminMetrics });
  const accounts = useQuery({ queryKey: ["admin-accounts"], queryFn: listAdminAccounts });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["admin-accounts"] });
  const suspend = useMutation({ mutationFn: suspendAccount, onSuccess: invalidate });
  const unsuspend = useMutation({ mutationFn: unsuspendAccount, onSuccess: invalidate });

  return (
    <div className="space-y-6">
      <PageHeader title="Admin" />

      {metrics.data && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <Metric label="Accounts" value={metrics.data.accounts} />
          <Metric label="Paid accounts" value={metrics.data.paid_accounts} />
          <Metric label="Users" value={metrics.data.users} />
          <Metric label="Leads" value={metrics.data.leads} />
        </div>
      )}

      <Card className="overflow-x-auto p-0">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-slate-200 text-slate-500">
            <tr>
              <th className="px-4 py-3">Account</th>
              <th className="px-4 py-3">Plan</th>
              <th className="px-4 py-3">Paid</th>
              <th className="px-4 py-3">On hold</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            {accounts.data?.map((a) => (
              <tr key={a.id} className="border-b border-slate-100">
                <td className="px-4 py-3 font-medium text-slate-800">{a.account_name ?? "—"}</td>
                <td className="px-4 py-3 text-slate-600">{a.plan_name}</td>
                <td className="px-4 py-3">{a.is_account_paid ? "Yes" : "No"}</td>
                <td className="px-4 py-3">{a.on_hold ? "Yes" : "No"}</td>
                <td className="px-4 py-3 text-right">
                  {a.on_hold ? (
                    <button
                      className="text-sm text-green-600 hover:underline"
                      onClick={() => unsuspend.mutate(a.id)}
                    >
                      Unsuspend
                    </button>
                  ) : (
                    <button
                      className="text-sm text-red-600 hover:underline"
                      onClick={() => suspend.mutate(a.id)}
                    >
                      Suspend
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
    </div>
  );
}
