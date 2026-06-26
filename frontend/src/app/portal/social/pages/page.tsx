"use client";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";

import { Empty } from "@/components/portal";
import { Card } from "@/components/ui";
import { listSocialConnections } from "@/lib/connections";

export default function SocialPagesPage() {
  const connections = useQuery({ queryKey: ["social"], queryFn: listSocialConnections });

  if (connections.isLoading) return <p className="text-slate-500">Loading…</p>;

  if (!connections.data || connections.data.length === 0) {
    return (
      <Empty
        message="No pages connected."
        cta={{ href: "/portal/settings/connections", label: "Connect a page" }}
      />
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {connections.data.map((conn) => (
        <Link key={conn.id} href={`/portal/social/pages/${conn.id}`}>
          <Card className="transition hover:border-brand">
            <p className="font-medium text-slate-800">{conn.name ?? conn.page_id}</p>
            <p className="text-xs uppercase text-slate-400">{conn.type}</p>
          </Card>
        </Link>
      ))}
    </div>
  );
}
