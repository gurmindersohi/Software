"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";

import { Button, Card, Input } from "@/components/ui";
import { Empty, PageHeader } from "@/components/portal";
import { listLeads, searchLeads, type Lead } from "@/lib/leads";

const PAGE_SIZE = 20;

export default function LeadsPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [active, setActive] = useState(false);
  const [offset, setOffset] = useState(0);

  const all = useQuery({
    queryKey: ["leads", offset],
    queryFn: () => listLeads({ limit: PAGE_SIZE, offset }),
    enabled: !active,
  });
  const results = useQuery({
    queryKey: ["leads", "search", name, email],
    queryFn: () => searchLeads(name, email),
    enabled: active,
  });

  const isLoading = active ? results.isLoading : all.isLoading;
  const leads: Lead[] = active ? (results.data ?? []) : (all.data?.items ?? []);
  const total = active ? leads.length : (all.data?.total ?? 0);

  return (
    <div>
      <PageHeader
        title="Leads"
        action={
          <Link href="/portal/leads/new">
            <Button>New lead</Button>
          </Link>
        }
      />

      <Card className="mb-4">
        <form
          className="flex flex-wrap items-end gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            setOffset(0);
            setActive(Boolean(name || email));
          }}
        >
          <label className="flex-1">
            <span className="text-sm text-slate-600">Name</span>
            <Input value={name} onChange={(e) => setName(e.target.value)} />
          </label>
          <label className="flex-1">
            <span className="text-sm text-slate-600">Email</span>
            <Input value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
          <Button type="submit">Search</Button>
          {active && (
            <button
              type="button"
              className="text-sm text-slate-500 hover:text-slate-800"
              onClick={() => {
                setName("");
                setEmail("");
                setActive(false);
              }}
            >
              Clear
            </button>
          )}
        </form>
      </Card>

      {isLoading && <p className="text-slate-500">Loading…</p>}
      {!isLoading && leads.length === 0 && (
        <Empty message="No leads yet." cta={{ href: "/portal/leads/new", label: "Add your first lead" }} />
      )}

      {leads.length > 0 && (
        <Card className="overflow-x-auto p-0">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-200 text-slate-500">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Phone</th>
                <th className="px-4 py-3">City</th>
              </tr>
            </thead>
            <tbody>
              {leads.map((lead) => (
                <tr key={lead.id} className="border-b border-slate-100 hover:bg-slate-50">
                  <td className="px-4 py-3">
                    <Link href={`/portal/leads/${lead.id}`} className="font-medium text-brand hover:underline">
                      {[lead.first_name, lead.last_name].filter(Boolean).join(" ") || "—"}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-slate-600">{lead.email}</td>
                  <td className="px-4 py-3 text-slate-600">{lead.primary_phone ?? "—"}</td>
                  <td className="px-4 py-3 text-slate-600">{lead.city ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      {!active && total > 0 && (
        <div className="mt-3 flex items-center justify-between text-sm text-slate-500">
          <span>
            {offset + 1}–{Math.min(offset + PAGE_SIZE, total)} of {total}
          </span>
          <div className="flex gap-2">
            <button
              className="rounded-md border border-slate-300 px-3 py-1 disabled:opacity-40"
              disabled={offset === 0}
              onClick={() => setOffset(Math.max(0, offset - PAGE_SIZE))}
            >
              Previous
            </button>
            <button
              className="rounded-md border border-slate-300 px-3 py-1 disabled:opacity-40"
              disabled={offset + PAGE_SIZE >= total}
              onClick={() => setOffset(offset + PAGE_SIZE)}
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
