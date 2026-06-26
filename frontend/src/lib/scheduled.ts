import { apiFetch } from "./api";

export interface ScheduledPost {
  id: string;
  account_id: string;
  social_media_id: string;
  platform: string;
  message?: string | null;
  link?: string | null;
  image_url?: string | null;
  scheduled_at: string;
  status: string;
  attempts: number;
  last_error?: string | null;
  external_post_id?: string | null;
}

export interface ScheduledPostInput {
  social_media_id: string;
  platform: string;
  message?: string;
  link?: string;
  image_url?: string;
  scheduled_at: string;
}

export const listScheduledPosts = () => apiFetch<ScheduledPost[]>("/scheduled-posts");

export const createScheduledPost = (data: ScheduledPostInput) =>
  apiFetch<ScheduledPost>("/scheduled-posts", {
    method: "POST",
    body: JSON.stringify(data),
  });

export const cancelScheduledPost = (id: string) =>
  apiFetch<void>(`/scheduled-posts/${id}`, { method: "DELETE" });
