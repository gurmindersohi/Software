"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { Empty, ErrorNote } from "@/components/portal";
import { Button, Card, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { clientReportUrl, createClient, deleteClient, listClients } from "@/lib/clients";

export default function ClientsPage() {
  const queryClient = useQueryClient();
  const clients = useQuery({ queryKey: ["clients"], queryFn: listClients });
  const [name, setName] = useState("");

  const add = useMutation({
    mutationFn: () => createClient(name),
    onSuccess: async () => {
      setName("");
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
    },
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteClient(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
    },
  });

  return (
    <div className="max-w-2xl space-y-6">
      <Card>
        <h2 className="mb-1 text-lg font-semibold text-slate-800">Clients</h2>
        <p className="mb-4 text-sm text-slate-500">
          Workspaces for the businesses you manage. Assign pages, posts, and leads to a client and
          download per-client reports.
        </p>
        <form
          className="flex gap-2"
          onSubmit={(e) => {
            e.preventDefault();
            add.mutate();
          }}
        >
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="New client name"
            required
          />
          <Button type="submit" disabled={add.isPending}>
            {add.isPending ? "Adding…" : "Add client"}
          </Button>
        </form>
        {add.isError && (
          <ErrorNote>
            {add.error instanceof ApiError ? add.error.message : "Could not add client."}
          </ErrorNote>
        )}
      </Card>

      {clients.data && clients.data.length === 0 && <Empty message="No clients yet." />}

      <div className="space-y-2">
        {clients.data?.map((c) => (
          <Card key={c.id} className="flex items-center justify-between">
            <span className="font-medium text-slate-800">{c.name}</span>
            <div className="flex items-center gap-4 text-sm">
              <a href={clientReportUrl(c.id)} className="text-brand hover:underline">
                Report (PDF)
              </a>
              <button
                className="text-red-600 hover:underline"
                onClick={() => remove.mutate(c.id)}
              >
                Delete
              </button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
