import { apiFetch } from "./api";

export interface TeamMember {
  id: string;
  email: string;
  first_name?: string | null;
  last_name?: string | null;
  email_confirmed: boolean;
  is_deleted: boolean;
  roles: string[];
}

export interface Role {
  id: string;
  name: string;
}

export const listTeam = () => apiFetch<TeamMember[]>("/team");

export const inviteMember = (data: { email: string; first_name?: string; role?: string }) =>
  apiFetch<TeamMember>("/team", { method: "POST", body: JSON.stringify(data) });

export const removeMember = (id: string) => apiFetch<void>(`/team/${id}`, { method: "DELETE" });

export const listRoles = () => apiFetch<Role[]>("/roles");

export const createRole = (name: string) =>
  apiFetch<Role>("/roles", { method: "POST", body: JSON.stringify({ name }) });
