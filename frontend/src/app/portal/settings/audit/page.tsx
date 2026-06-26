"use client";
import { useQuery } from "@tanstack/react-query";

import { Empty } from "@/components/portal";
import { Card } from "@/components/ui";
import { listAuditLogs } from "@/lib/audit";

export default function AuditPage() {
  const logs = useQuery({ queryKey: ["audit-logs"], queryFn: listAuditLogs });

  if (logs.isLoading) return <p className="text-slate-500">Loading…</p>;
  if (!logs.data || logs.data.items.length === 0)
    return <Empty message="No activity recorded yet." />;

  return (
    <Card className="overflow-x-auto p-0">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-slate-200 text-slate-500">
          <tr>
            <th className="px-4 py-3">Action</th>
            <th className="px-4 py-3">Detail</th>
            <th className="px-4 py-3">When</th>
          </tr>
        </thead>
        <tbody>
          {logs.data.items.map((entry) => (
            <tr key={entry.id} className="border-b border-slate-100">
              <td className="px-4 py-3 font-mono text-xs text-slate-700">{entry.action}</td>
              <td className="px-4 py-3 text-slate-600">{entry.detail ?? "—"}</td>
              <td className="px-4 py-3 text-slate-500">
                {new Date(entry.created_on).toLocaleString()}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </Card>
  );
}
