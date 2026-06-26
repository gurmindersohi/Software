import { apiFetch } from "./api";

export interface Lead {
  id: string;
  account_id: string;
  first_name?: string | null;
  last_name?: string | null;
  full_name?: string | null;
  email: string;
  primary_phone?: string | null;
  secondary_phone?: string | null;
  gender?: string | null;
  city?: string | null;
  province?: string | null;
  country?: string | null;
  postal_code?: string | null;
  lead_source?: string | null;
  is_email_allowed: boolean;
  is_phone_call_allowed: boolean;
  is_text_allowed: boolean;
  is_member: boolean;
}

export type LeadInput = Partial<Omit<Lead, "id" | "account_id">> & { email: string };

export const listLeads = () => apiFetch<Lead[]>("/leads");

export const searchLeads = (name: string, email: string) => {
  const params = new URLSearchParams();
  if (name) params.set("name", name);
  if (email) params.set("email", email);
  return apiFetch<Lead[]>(`/leads/search?${params.toString()}`);
};

export const getLead = (id: string) => apiFetch<Lead>(`/leads/${id}`);

export const createLead = (data: LeadInput) =>
  apiFetch<Lead>("/leads", { method: "POST", body: JSON.stringify(data) });

export const updateLead = (id: string, data: Partial<LeadInput>) =>
  apiFetch<Lead>(`/leads/${id}`, { method: "PUT", body: JSON.stringify(data) });

export const deleteLead = (id: string) =>
  apiFetch<void>(`/leads/${id}`, { method: "DELETE" });
