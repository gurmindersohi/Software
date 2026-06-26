import { apiFetch } from "./api";
import type { Page } from "./pagination";

export interface ScheduledPost {
  id: string;
  account_id: string;
  client_id?: string | null;
  social_media_id: string;
  platform: string;
  message?: string | null;
  link?: string | null;
  image_url?: string | null;
  scheduled_at: string;
  status: string;
  requires_approval: boolean;
  approval_status: string;
  attempts: number;
  last_error?: string | null;
  external_post_id?: string | null;
}

export interface ScheduledPostInput {
  social_media_id: string;
  client_id?: string;
  platform: string;
  message?: string;
  link?: string;
  image_url?: string;
  scheduled_at: string;
  requires_approval?: boolean;
}

export const listScheduledPosts = () => apiFetch<Page<ScheduledPost>>("/scheduled-posts");

export const createScheduledPost = (data: ScheduledPostInput) =>
  apiFetch<ScheduledPost>("/scheduled-posts", {
    method: "POST",
    body: JSON.stringify(data),
  });

export const cancelScheduledPost = (id: string) =>
  apiFetch<void>(`/scheduled-posts/${id}`, { method: "DELETE" });

export const approveScheduledPost = (id: string) =>
  apiFetch<ScheduledPost>(`/scheduled-posts/${id}/approve`, { method: "POST" });

export const rejectScheduledPost = (id: string) =>
  apiFetch<ScheduledPost>(`/scheduled-posts/${id}/reject`, { method: "POST" });
