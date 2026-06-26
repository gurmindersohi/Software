import { apiFetch } from "./api";

export interface AdminAccount {
  id: string;
  account_name?: string | null;
  plan_name: string;
  is_account_paid: boolean;
  on_hold: boolean;
  is_deleted: boolean;
}

export interface AdminMetrics {
  accounts: number;
  paid_accounts: number;
  users: number;
  leads: number;
}

export const listAdminAccounts = () => apiFetch<AdminAccount[]>("/admin/accounts");

export const suspendAccount = (id: string) =>
  apiFetch<AdminAccount>(`/admin/accounts/${id}/suspend`, { method: "POST" });

export const unsuspendAccount = (id: string) =>
  apiFetch<AdminAccount>(`/admin/accounts/${id}/unsuspend`, { method: "POST" });

export const adminMetrics = () => apiFetch<AdminMetrics>("/admin/metrics");
