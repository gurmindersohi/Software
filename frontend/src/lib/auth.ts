import { apiFetch } from "./api";

export interface User {
  id: string;
  email: string;
  first_name?: string | null;
  last_name?: string | null;
  email_confirmed: boolean;
  two_factor_enabled: boolean;
  is_superuser: boolean;
  account_id?: string | null;
  roles: string[];
}

export interface RegisterInput {
  email: string;
  password: string;
  account_name?: string;
  first_name?: string;
}

export interface LoginResponse {
  two_factor_required: boolean;
  user: User | null;
}

export function login(email: string, password: string): Promise<LoginResponse> {
  return apiFetch<LoginResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function verifyTwoFactor(code: string): Promise<User> {
  return apiFetch<User>("/auth/2fa/verify", {
    method: "POST",
    body: JSON.stringify({ code }),
  });
}

export function setupTwoFactor(): Promise<{ secret: string; otpauth_uri: string }> {
  return apiFetch("/auth/2fa/setup", { method: "POST" });
}

export function enableTwoFactor(code: string): Promise<{ recovery_codes: string[] }> {
  return apiFetch("/auth/2fa/enable", { method: "POST", body: JSON.stringify({ code }) });
}

export function disableTwoFactor(code: string): Promise<{ detail: string }> {
  return apiFetch("/auth/2fa/disable", { method: "POST", body: JSON.stringify({ code }) });
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

export function changeEmail(newEmail: string, password: string): Promise<User> {
  return apiFetch<User>("/auth/change-email", {
    method: "POST",
    body: JSON.stringify({ new_email: newEmail, password }),
  });
}

export function exportPersonalData(): Promise<Record<string, unknown>> {
  return apiFetch("/auth/personal-data");
}

export function deleteAccount(password: string): Promise<{ detail: string }> {
  return apiFetch("/auth/delete-account", {
    method: "POST",
    body: JSON.stringify({ password }),
  });
}

export function signOutEverywhere(): Promise<{ detail: string }> {
  return apiFetch("/auth/sign-out-everywhere", { method: "POST" });
}
