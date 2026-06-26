import { apiFetch } from "./api";

export interface MediaAsset {
  id: string;
  url: string;
  kind: string; // image | video | file
  content_type?: string | null;
}

export const listMedia = () => apiFetch<MediaAsset[]>("/media");

export async function uploadMedia(file: File): Promise<{ url: string; key: string }> {
  // multipart upload — not via apiFetch (which forces a JSON content-type)
  const form = new FormData();
  form.append("file", file);
  const res = await fetch("/api/v1/media/upload", {
    method: "POST",
    body: form,
    credentials: "include",
  });
  if (!res.ok) throw new Error("Upload failed");
  return res.json();
}
