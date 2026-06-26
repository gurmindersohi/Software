"use client";
import { useState } from "react";

import { Button, Field, Input } from "@/components/ui";
import { ErrorNote } from "@/components/portal";
import { ApiError } from "@/lib/api";
import type { LeadInput } from "@/lib/leads";

const TEXT_FIELDS: { key: keyof LeadInput; label: string; type?: string; required?: boolean }[] = [
  { key: "first_name", label: "First name" },
  { key: "last_name", label: "Last name" },
  { key: "email", label: "Email", type: "email", required: true },
  { key: "primary_phone", label: "Phone" },
  { key: "city", label: "City" },
  { key: "province", label: "Province" },
  { key: "country", label: "Country" },
  { key: "lead_source", label: "Lead source" },
];

const CONSENTS: { key: keyof LeadInput; label: string }[] = [
  { key: "is_email_allowed", label: "Email allowed" },
  { key: "is_phone_call_allowed", label: "Calls allowed" },
  { key: "is_text_allowed", label: "Texts allowed" },
  { key: "is_member", label: "Member" },
];

export function LeadForm({
  initial,
  submitLabel,
  onSubmit,
}: {
  initial?: Partial<LeadInput>;
  submitLabel: string;
  onSubmit: (data: LeadInput) => Promise<unknown>;
}) {
  const [form, setForm] = useState<Record<string, unknown>>({ ...initial });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await onSubmit(form as LeadInput);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save lead.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={submit} className="max-w-xl space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        {TEXT_FIELDS.map((f) => (
          <Field key={f.key} label={f.label}>
            <Input
              type={f.type ?? "text"}
              required={f.required}
              value={(form[f.key] as string) ?? ""}
              onChange={(e) => setForm((s) => ({ ...s, [f.key]: e.target.value }))}
            />
          </Field>
        ))}
      </div>
      <div className="flex flex-wrap gap-4">
        {CONSENTS.map((c) => (
          <label key={c.key} className="flex items-center gap-2 text-sm text-slate-700">
            <input
              type="checkbox"
              checked={Boolean(form[c.key])}
              onChange={(e) => setForm((s) => ({ ...s, [c.key]: e.target.checked }))}
            />
            {c.label}
          </label>
        ))}
      </div>
      {error && <ErrorNote>{error}</ErrorNote>}
      <Button type="submit" disabled={busy}>
        {busy ? "Saving…" : submitLabel}
      </Button>
    </form>
  );
}
