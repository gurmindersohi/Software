"use client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { Empty, ErrorNote, StatusBadge } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { listSocialConnections } from "@/lib/connections";
import {
  cancelScheduledPost,
  createScheduledPost,
  listScheduledPosts,
} from "@/lib/scheduled";

export default function SocialQueuePage() {
  const queryClient = useQueryClient();
  const connections = useQuery({ queryKey: ["social"], queryFn: listSocialConnections });
  const posts = useQuery({ queryKey: ["scheduled"], queryFn: listScheduledPosts });

  const [connId, setConnId] = useState("");
  const [message, setMessage] = useState("");
  const [when, setWhen] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function schedule(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    const conn = connections.data?.find((c) => c.id === connId);
    if (!conn) {
      setError("Pick a connected page.");
      return;
    }
    setBusy(true);
    try {
      await createScheduledPost({
        social_media_id: conn.id,
        platform: conn.type ?? "facebook",
        message,
        scheduled_at: new Date(when).toISOString(),
      });
      setMessage("");
      setWhen("");
      await queryClient.invalidateQueries({ queryKey: ["scheduled"] });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not schedule post.");
    } finally {
      setBusy(false);
    }
  }

  const hasConnections = (connections.data?.length ?? 0) > 0;

  return (
    <div>
      {!hasConnections ? (
        <Empty
          message="Connect a Facebook or Instagram page to schedule posts."
          cta={{ href: "/portal/settings/connections", label: "Connect a page" }}
        />
      ) : (
        <Card className="mb-6">
          <form onSubmit={schedule} className="space-y-4">
            <Field label="Page">
              <select
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                value={connId}
                onChange={(e) => setConnId(e.target.value)}
                required
              >
                <option value="">Select a page…</option>
                {connections.data?.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name ?? c.page_id} ({c.type})
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Message">
              <textarea
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                rows={3}
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                required
              />
            </Field>
            <Field label="Publish at">
              <Input
                type="datetime-local"
                value={when}
                onChange={(e) => setWhen(e.target.value)}
                required
              />
            </Field>
            {error && <ErrorNote>{error}</ErrorNote>}
            <Button type="submit" disabled={busy}>
              {busy ? "Scheduling…" : "Schedule post"}
            </Button>
          </form>
        </Card>
      )}

      {posts.isLoading && <p className="text-slate-500">Loading…</p>}
      {posts.data && posts.data.items.length === 0 && <Empty message="No scheduled posts yet." />}

      {posts.data && posts.data.items.length > 0 && (
        <Card className="overflow-x-auto p-0">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-200 text-slate-500">
              <tr>
                <th className="px-4 py-3">Message</th>
                <th className="px-4 py-3">When</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {posts.data.items.map((post) => (
                <tr key={post.id} className="border-b border-slate-100">
                  <td className="max-w-xs truncate px-4 py-3">{post.message}</td>
                  <td className="px-4 py-3 text-slate-600">
                    {new Date(post.scheduled_at).toLocaleString()}
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={post.status} />
                  </td>
                  <td className="px-4 py-3 text-right">
                    {post.status !== "published" && (
                      <button
                        className="text-sm text-red-600 hover:underline"
                        onClick={async () => {
                          await cancelScheduledPost(post.id);
                          await queryClient.invalidateQueries({ queryKey: ["scheduled"] });
                        }}
                      >
                        Cancel
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  );
}
