"use client";
import { Card } from "@/components/ui";
import { useCurrentUser } from "@/lib/hooks";

export default function DashboardPage() {
  const { data: user, isLoading, isError } = useCurrentUser();

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>

      {isLoading && <p className="text-slate-500">Loading…</p>}
      {isError && <p className="text-red-600">Could not load your session.</p>}

      {user && (
        <Card className="max-w-md">
          <h2 className="mb-2 text-lg font-semibold text-slate-800">
            Welcome{user.first_name ? `, ${user.first_name}` : ""}
          </h2>
          <dl className="space-y-1 text-sm text-slate-600">
            <div className="flex justify-between">
              <dt>Email</dt>
              <dd>{user.email}</dd>
            </div>
            <div className="flex justify-between">
              <dt>Roles</dt>
              <dd>{user.roles.join(", ") || "—"}</dd>
            </div>
            <div className="flex justify-between">
              <dt>Email confirmed</dt>
              <dd>{user.email_confirmed ? "Yes" : "No"}</dd>
            </div>
          </dl>
        </Card>
      )}

      <p className="text-sm text-slate-500">
        Leads, Social, Ads, and Settings screens are built in Phase 8.
      </p>
    </div>
  );
}
