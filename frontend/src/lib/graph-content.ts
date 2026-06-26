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

export const createPagePost = (connId: string, data: { message: string; link?: string }) =>
  apiFetch<{ id: string }>(`/social/${connId}/posts`, {
    method: "POST",
    body: JSON.stringify(data),
  });
