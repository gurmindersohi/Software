"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { login, verifyTwoFactor } from "@/lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [needs2fa, setNeeds2fa] = useState(false);
  const [code, setCode] = useState("");

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const result = await login(email, password);
      if (result.two_factor_required) {
        setNeeds2fa(true);
      } else {
        router.push("/portal");
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Something went wrong.");
    } finally {
      setBusy(false);
    }
  }

  async function onVerify(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await verifyTwoFactor(code);
      router.push("/portal");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Invalid code.");
    } finally {
      setBusy(false);
    }
  }

  if (needs2fa) {
    return (
      <main className="mx-auto flex min-h-screen max-w-md items-center px-6">
        <Card className="w-full">
          <h1 className="mb-2 text-2xl font-bold text-slate-900">Two-factor code</h1>
          <p className="mb-6 text-sm text-slate-600">
            Enter the 6-digit code from your authenticator app (or a recovery code).
          </p>
          <form onSubmit={onVerify} className="space-y-4">
            <Field label="Code">
              <Input
                value={code}
                onChange={(e) => setCode(e.target.value)}
                autoFocus
                required
              />
            </Field>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <Button type="submit" disabled={busy} className="w-full">
              {busy ? "Verifying…" : "Verify"}
            </Button>
          </form>
        </Card>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-md items-center px-6">
      <Card className="w-full">
        <h1 className="mb-6 text-2xl font-bold text-slate-900">Sign in</h1>
        <form onSubmit={onSubmit} className="space-y-4">
          <Field label="Email">
            <Input
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </Field>
          <Field label="Password">
            <Input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </Field>
          {error && <p className="text-sm text-red-600">{error}</p>}
          <Button type="submit" disabled={busy} className="w-full">
            {busy ? "Signing in…" : "Sign in"}
          </Button>
        </form>
        <p className="mt-4 text-sm text-slate-600">
          No account?{" "}
          <Link href="/register" className="font-medium text-brand hover:underline">
            Create one
          </Link>
        </p>
      </Card>
    </main>
  );
}
