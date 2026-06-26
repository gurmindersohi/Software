"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { listSocialConnections } from "@/lib/connections";
import {
  createPagePost,
  getInstagramInsights,
  getPageInsights,
  getPagePosts,
  syncLeads,
} from "@/lib/graph-content";

function GraphError({ what }: { what: string }) {
  return (
    <p className="text-sm text-slate-500">
      Couldn&apos;t load {what}. This needs a live Facebook connection — once a real page is
      connected, it populates from the Graph API.
    </p>
  );
}

export default function PageContent() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [message, setMessage] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [videoUrl, setVideoUrl] = useState("");

  const connections = useQuery({ queryKey: ["social"], queryFn: listSocialConnections });
  const page = connections.data?.find((c) => c.id === id);
  const isInstagram = page?.type === "instagram";

  const sync = useMutation({ mutationFn: () => syncLeads(id) });
  const igInsights = useQuery({
    queryKey: ["ig-insights", id],
    queryFn: () => getInstagramInsights(id),
    enabled: isInstagram,
    retry: false,
  });

  const posts = useQuery({ queryKey: ["page-posts", id], queryFn: () => getPagePosts(id), retry: false });
  const insights = useQuery({
    queryKey: ["page-insights", id],
    queryFn: () => getPageInsights(id),
    retry: false,
  });

  const publish = useMutation({
    mutationFn: () =>
      createPagePost(id, {
        message,
        image_url: imageUrl || undefined,
        video_url: videoUrl || undefined,
      }),
    onSuccess: async () => {
      setMessage("");
      setImageUrl("");
      setVideoUrl("");
      await queryClient.invalidateQueries({ queryKey: ["page-posts", id] });
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">{page?.name ?? "Page"}</h2>
        <Button
          className="bg-slate-600 hover:bg-slate-700"
          disabled={sync.isPending}
          onClick={() => sync.mutate()}
        >
          {sync.isPending
            ? "Syncing…"
            : sync.isSuccess
              ? `Imported ${sync.data.imported} leads`
              : "Sync Facebook leads"}
        </Button>
      </div>

      <Card>
        <h3 className="mb-3 font-medium text-slate-800">Create a post</h3>
        <form
          className="space-y-3"
          onSubmit={(e) => {
            e.preventDefault();
            publish.mutate();
          }}
        >
          <Field label="Message">
            <textarea
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              rows={3}
              value={message}
              onChange={(e) => setMessage(e.target.value)}
            />
          </Field>
          <div className="grid gap-3 sm:grid-cols-2">
            <Field label="Image URL (optional)">
              <Input
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
                placeholder="https://…"
              />
            </Field>
            <Field label="Video URL (optional)">
              <Input
                value={videoUrl}
                onChange={(e) => setVideoUrl(e.target.value)}
                placeholder="https://…"
              />
            </Field>
          </div>
          {publish.isError && (
            <ErrorNote>
              {publish.error instanceof ApiError ? publish.error.message : "Could not publish."}
            </ErrorNote>
          )}
          {publish.isSuccess && <p className="text-sm text-green-600">Posted.</p>}
          <Button type="submit" disabled={publish.isPending}>
            {publish.isPending ? "Posting…" : "Publish now"}
          </Button>
        </form>
      </Card>

      <section>
        <h3 className="mb-3 font-medium text-slate-800">Recent posts</h3>
        {posts.isError && <GraphError what="posts" />}
        {posts.data && posts.data.length === 0 && <p className="text-sm text-slate-500">No posts.</p>}
        <div className="space-y-3">
          {posts.data?.map((post) => (
            <Card key={post.id}>
              <p className="text-slate-700">{post.message ?? "(no text)"}</p>
              {post.created_time && (
                <p className="mt-1 text-xs text-slate-400">
                  {new Date(post.created_time).toLocaleString()}
                </p>
              )}
            </Card>
          ))}
        </div>
      </section>

      <section>
        <h3 className="mb-3 font-medium text-slate-800">Insights</h3>
        {insights.isError && <GraphError what="insights" />}
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {insights.data?.map((metric) => (
            <Card key={metric.name}>
              <p className="text-sm text-slate-500">{metric.title ?? metric.name}</p>
              <p className="mt-1 text-2xl font-bold text-slate-900">
                {metric.values?.[metric.values.length - 1]?.value ?? "—"}
              </p>
            </Card>
          ))}
        </div>
      </section>

      {isInstagram && (
        <section>
          <h3 className="mb-3 font-medium text-slate-800">Instagram insights</h3>
          {igInsights.isError && <GraphError what="Instagram insights" />}
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {igInsights.data?.map((metric) => (
              <Card key={metric.name}>
                <p className="text-sm text-slate-500">{metric.title ?? metric.name}</p>
                <p className="mt-1 text-2xl font-bold text-slate-900">
                  {metric.values?.[metric.values.length - 1]?.value ?? "—"}
                </p>
              </Card>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
