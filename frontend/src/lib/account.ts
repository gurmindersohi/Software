import { apiFetch } from "./api";

export interface Account {
  id: string;
  account_name?: string | null;
  account_type?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  city?: string | null;
  province?: string | null;
  country?: string | null;
  postal_code?: string | null;
  logo?: string | null;
  trial_expiry?: string | null;
  is_account_paid: boolean;
  on_hold: boolean;
}

export type AccountUpdate = Partial<
  Pick<
    Account,
    | "account_name"
    | "account_type"
    | "email"
    | "phone"
    | "address"
    | "city"
    | "province"
    | "country"
    | "postal_code"
    | "logo"
  >
>;

export const getAccount = () => apiFetch<Account>("/account");

export const updateAccount = (data: AccountUpdate) =>
  apiFetch<Account>("/account", { method: "PUT", body: JSON.stringify(data) });

export interface Billing {
  customer_id?: string | null;
  subscription_id?: string | null;
  is_account_paid: boolean;
  on_hold: boolean;
  trial_expiry?: string | null;
}

export const getBilling = () => apiFetch<Billing>("/billing");
