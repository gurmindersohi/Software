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

export interface AdsetInput {
  name: string;
  campaign_id: string;
  daily_budget: number; // minor units (cents)
  optimization_goal?: string;
  age_min?: number;
  age_max?: number;
  country_codes?: string[];
  interest_ids?: string[];
}

export interface AdInput {
  name: string;
  adset_id: string;
  page_id: string;
  message: string;
  link: string;
  headline?: string;
  image_url?: string;
}

export interface TargetingResult {
  id: string;
  name?: string;
  audience_size?: string;
}

export const createAdset = (adId: string, data: AdsetInput) =>
  apiFetch<NamedEntity>(`/ad-accounts/${adId}/adsets`, {
    method: "POST",
    body: JSON.stringify(data),
  });

export const createAd = (adId: string, data: AdInput) =>
  apiFetch<NamedEntity>(`/ad-accounts/${adId}/ads`, {
    method: "POST",
    body: JSON.stringify(data),
  });

export const searchTargeting = (adId: string, q: string) =>
  apiFetch<TargetingResult[]>(`/ad-accounts/${adId}/targeting-search?q=${encodeURIComponent(q)}`);
