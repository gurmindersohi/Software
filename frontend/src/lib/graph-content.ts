import { apiFetch } from "./api";

export interface PagePost {
  id: string;
  message?: string;
  created_time?: string;
  full_picture?: string;
}

export interface InsightValue {
  value?: number;
  end_time?: string;
}

export interface PageInsight {
  name?: string;
  title?: string;
  values?: InsightValue[];
}

export const getPagePosts = (connId: string) => apiFetch<PagePost[]>(`/social/${connId}/posts`);

export const getPageInsights = (connId: string) =>
  apiFetch<PageInsight[]>(`/social/${connId}/insights`);

export const createPagePost = (
  connId: string,
  data: { message?: string; link?: string; image_url?: string; video_url?: string },
) =>
  apiFetch<{ id: string }>(`/social/${connId}/posts`, {
    method: "POST",
    body: JSON.stringify(data),
  });

export const getInstagramInsights = (connId: string) =>
  apiFetch<PageInsight[]>(`/social/${connId}/instagram-insights`);

export const syncLeads = (connId: string) =>
  apiFetch<{ imported: number }>(`/social/${connId}/sync-leads`, { method: "POST" });

export interface Snapshot {
  metric: string;
  value: number;
  captured_at: string;
}

export const captureInsights = (connId: string) =>
  apiFetch<{ captured: number }>(`/social/${connId}/capture-insights`, { method: "POST" });

export const getAnalytics = (connId: string) =>
  apiFetch<Snapshot[]>(`/social/${connId}/analytics`);
