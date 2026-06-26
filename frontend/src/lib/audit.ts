import { apiFetch } from "./api";
import type { Page } from "./pagination";

export interface AuditEntry {
  id: string;
  action: string;
  detail?: string | null;
  user_id?: string | null;
  created_on: string;
}

export const listAuditLogs = () => apiFetch<Page<AuditEntry>>("/audit-logs");
