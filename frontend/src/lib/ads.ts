import { apiFetch } from "./api";

export interface Campaign {
  id: string;
  name?: string;
  objective?: string;
  status?: string;
}

export interface NamedEntity {
  id: string;
  name?: string;
}

export const getCampaigns = (adId: string) =>
  apiFetch<Campaign[]>(`/ad-accounts/${adId}/campaigns`);

export const createCampaign = (
  adId: string,
  data: { name: string; objective?: string; status?: string },
) =>
  apiFetch<Campaign>(`/ad-accounts/${adId}/campaigns`, {
    method: "POST",
    body: JSON.stringify(data),
  });

export const getAdsets = (adId: string) => apiFetch<NamedEntity[]>(`/ad-accounts/${adId}/adsets`);

export const getAds = (adId: string) => apiFetch<NamedEntity[]>(`/ad-accounts/${adId}/ads`);
