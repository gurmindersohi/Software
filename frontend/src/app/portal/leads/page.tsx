"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";

import { Button, Card, Input } from "@/components/ui";
import { Empty, PageHeader } from "@/components/portal";
import { listLeads, searchLeads, type Lead } from "@/lib/leads";

export default function LeadsPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [active, setActive] = useState(false);

  const all = useQuery({ queryKey: ["leads"], queryFn: listLeads, enabled: !active });
  const results = useQuery({
    queryKey: ["leads", "search", name, email],
    queryFn: () => searchLeads(name, email),
    enabled: active,
  });

  const query = active ? results : all;
  const leads: Lead[] = query.data ?? [];

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

      {query.isLoading && <p className="text-slate-500">Loading…</p>}
      {!query.isLoading && leads.length === 0 && (
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
    </div>
  );
}
