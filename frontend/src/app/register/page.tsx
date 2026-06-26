"use client";
import Link from "next/link";
import { useState } from "react";

import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { register } from "@/lib/auth";

export default function RegisterPage() {
  const [form, setForm] = useState({ email: "", password: "", account_name: "" });
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);
  const [busy, setBusy] = useState(false);

  function update(key: keyof typeof form) {
    return (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await register(form);
      setDone(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Something went wrong.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-md items-center px-6">
      <Card className="w-full">
        <h1 className="mb-6 text-2xl font-bold text-slate-900">Create your account</h1>
        {done ? (
          <p className="text-sm text-slate-700">
            Check your email to confirm your account, then{" "}
            <Link href="/login" className="font-medium text-brand hover:underline">
              sign in
            </Link>
            .
          </p>
        ) : (
          <form onSubmit={onSubmit} className="space-y-4">
            <Field label="Business name">
              <Input value={form.account_name} onChange={update("account_name")} />
            </Field>
            <Field label="Email">
              <Input type="email" value={form.email} onChange={update("email")} required />
            </Field>
            <Field label="Password">
              <Input
                type="password"
                autoComplete="new-password"
                value={form.password}
                onChange={update("password")}
                minLength={6}
                required
              />
            </Field>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <Button type="submit" disabled={busy} className="w-full">
              {busy ? "Creating…" : "Create account"}
            </Button>
          </form>
        )}
        <p className="mt-4 text-sm text-slate-600">
          Already have an account?{" "}
          <Link href="/login" className="font-medium text-brand hover:underline">
            Sign in
          </Link>
        </p>
      </Card>
    </main>
  );
}
