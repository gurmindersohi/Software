"use client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { getAccount, updateAccount, type AccountUpdate } from "@/lib/account";
import { ApiError } from "@/lib/api";

const FIELDS: { key: keyof AccountUpdate; label: string }[] = [
  { key: "account_name", label: "Business name" },
  { key: "email", label: "Email" },
  { key: "phone", label: "Phone" },
  { key: "address", label: "Address" },
  { key: "city", label: "City" },
  { key: "province", label: "Province" },
  { key: "country", label: "Country" },
  { key: "postal_code", label: "Postal code" },
];

export default function GeneralSettingsPage() {
  const queryClient = useQueryClient();
  const { data: account } = useQuery({ queryKey: ["account"], queryFn: getAccount });
  const [form, setForm] = useState<Record<string, string>>({});
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (account) {
      const seed: Record<string, string> = {};
      for (const f of FIELDS) seed[f.key] = (account[f.key] as string) ?? "";
      setForm(seed);
    }
  }, [account]);

  async function save(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSaved(false);
    setBusy(true);
    try {
      await updateAccount(form as AccountUpdate);
      await queryClient.invalidateQueries({ queryKey: ["account"] });
      setSaved(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <Card className="max-w-xl">
      <form onSubmit={save} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          {FIELDS.map((f) => (
            <Field key={f.key} label={f.label}>
              <Input
                value={form[f.key] ?? ""}
                onChange={(e) => setForm((s) => ({ ...s, [f.key]: e.target.value }))}
              />
            </Field>
          ))}
        </div>
        {error && <ErrorNote>{error}</ErrorNote>}
        {saved && <p className="text-sm text-green-600">Saved.</p>}
        <Button type="submit" disabled={busy}>
          {busy ? "Saving…" : "Save changes"}
        </Button>
      </form>
    </Card>
  );
}
