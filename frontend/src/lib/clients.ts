import { apiFetch } from "./api";

export interface Client {
  id: string;
  name: string;
}

export const listClients = () => apiFetch<Client[]>("/clients");

export const createClient = (name: string) =>
  apiFetch<Client>("/clients", { method: "POST", body: JSON.stringify({ name }) });

export const deleteClient = (id: string) =>
  apiFetch<void>(`/clients/${id}`, { method: "DELETE" });

// PDF report — opened directly (proxied to the backend, cookies included).
export const clientReportUrl = (id: string) => `/api/v1/clients/${id}/report`;
