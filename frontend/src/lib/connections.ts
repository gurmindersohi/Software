import { apiFetch } from "./api";

export interface SocialConnection {
  id: string;
  page_id?: string | null;
  name?: string | null;
  image?: string | null;
  type?: string | null;
  email?: string | null;
  account_id: string;
}

export interface AdAccountConnection {
  id: string;
  user_account_id?: string | null;
  name?: string | null;
  type?: string | null;
  account_id: string;
}

export const listSocialConnections = () => apiFetch<SocialConnection[]>("/social");

export const deleteSocialConnection = (id: string) =>
  apiFetch<void>(`/social/${id}`, { method: "DELETE" });

export const listAdAccounts = () => apiFetch<AdAccountConnection[]>("/ad-accounts");

export const getFacebookConnectUrl = () =>
  apiFetch<{ authorize_url: string }>("/integrations/facebook/connect");
