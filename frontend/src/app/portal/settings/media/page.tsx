"use client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { Empty } from "@/components/portal";
import { Card } from "@/components/ui";
import { listMedia, uploadMedia } from "@/lib/media";

export default function MediaPage() {
  const queryClient = useQueryClient();
  const media = useQuery({ queryKey: ["media"], queryFn: listMedia });
  const [busy, setBusy] = useState(false);

  async function onFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setBusy(true);
    try {
      await uploadMedia(file);
      await queryClient.invalidateQueries({ queryKey: ["media"] });
    } finally {
      setBusy(false);
      e.target.value = "";
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <label className="cursor-pointer rounded-md bg-brand px-4 py-2 text-sm font-medium text-white hover:bg-brand-dark">
          {busy ? "Uploading…" : "Upload media"}
          <input type="file" className="hidden" onChange={onFile} disabled={busy} />
        </label>
        <span className="text-sm text-slate-500">Images and videos for posts &amp; ads.</span>
      </div>

      {media.data && media.data.length === 0 && <Empty message="No media uploaded yet." />}

      <div className="grid gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {media.data?.map((asset) => (
          <Card key={asset.id} className="p-2">
            {asset.kind === "image" ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={asset.url} alt="" className="h-32 w-full rounded object-cover" />
            ) : (
              <div className="flex h-32 items-center justify-center rounded bg-slate-100 text-sm text-slate-500">
                {asset.kind}
              </div>
            )}
          </Card>
        ))}
      </div>
    </div>
  );
}
