"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useState } from "react";

import { ErrorNote, PageHeader, StatusBadge } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { createCampaign, getCampaigns } from "@/lib/ads";
import { listAdAccounts } from "@/lib/connections";

const OBJECTIVES = ["LINK_CLICKS", "REACH", "CONVERSIONS", "POST_ENGAGEMENT"];

export default function AdAccountPage() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [objective, setObjective] = useState(OBJECTIVES[0]);

  const accounts = useQuery({ queryKey: ["ad-accounts"], queryFn: listAdAccounts });
  const account = accounts.data?.find((a) => a.id === id);

  const campaigns = useQuery({
    queryKey: ["campaigns", id],
    queryFn: () => getCampaigns(id),
    retry: false,
  });

  const create = useMutation({
    mutationFn: () => createCampaign(id, { name, objective }),
    onSuccess: async () => {
      setName("");
      await queryClient.invalidateQueries({ queryKey: ["campaigns", id] });
    },
  });

  return (
    <div className="space-y-6">
      <PageHeader title={account?.name ?? "Ad account"} />

      <Card>
        <h3 className="mb-3 font-medium text-slate-800">New campaign</h3>
        <form
          className="flex flex-wrap items-end gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            create.mutate();
          }}
        >
          <Field label="Name">
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </Field>
          <label className="text-sm">
            <span className="text-slate-600">Objective</span>
            <select
              className="block rounded-md border border-slate-300 px-3 py-2 text-sm"
              value={objective}
              onChange={(e) => setObjective(e.target.value)}
            >
              {OBJECTIVES.map((o) => (
                <option key={o} value={o}>
                  {o}
                </option>
              ))}
            </select>
          </label>
          <Button type="submit" disabled={create.isPending}>
            {create.isPending ? "Creating…" : "Create (paused)"}
          </Button>
        </form>
        {create.isError && (
          <ErrorNote>
            {create.error instanceof ApiError ? create.error.message : "Could not create campaign."}
          </ErrorNote>
        )}
      </Card>

      <section>
        <h3 className="mb-3 font-medium text-slate-800">Campaigns</h3>
        {campaigns.isError && (
          <p className="text-sm text-slate-500">
            Couldn&apos;t load campaigns. This needs a live ad-account connection.
          </p>
        )}
        {campaigns.data && campaigns.data.length === 0 && (
          <p className="text-sm text-slate-500">No campaigns yet.</p>
        )}
        <div className="space-y-2">
          {campaigns.data?.map((campaign) => (
            <Card key={campaign.id} className="flex items-center justify-between">
              <div>
                <p className="font-medium text-slate-800">{campaign.name}</p>
                <p className="text-xs text-slate-400">{campaign.objective}</p>
              </div>
              {campaign.status && <StatusBadge status={campaign.status.toLowerCase()} />}
            </Card>
          ))}
        </div>
      </section>
    </div>
  );
}
