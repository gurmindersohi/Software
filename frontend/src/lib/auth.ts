import { apiFetch } from "./api";

export interface User {
  id: string;
  email: string;
  first_name?: string | null;
  last_name?: string | null;
  email_confirmed: boolean;
  account_id?: string | null;
  roles: string[];
}

export interface RegisterInput {
  email: string;
  password: string;
  account_name?: string;
  first_name?: string;
}

export function login(email: string, password: string): Promise<User> {
  return apiFetch<User>("/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function register(data: RegisterInput): Promise<User> {
  return apiFetch<User>("/auth/register", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function logout(): Promise<{ detail: string }> {
  return apiFetch<{ detail: string }>("/auth/logout", { method: "POST" });
}

export function getMe(): Promise<User> {
  return apiFetch<User>("/auth/me");
}
