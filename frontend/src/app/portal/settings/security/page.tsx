"use client";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { disableTwoFactor, enableTwoFactor, setupTwoFactor } from "@/lib/auth";
import { useCurrentUser } from "@/lib/hooks";

export default function SecurityPage() {
  const queryClient = useQueryClient();
  const { data: user } = useCurrentUser();

  const [secret, setSecret] = useState<string | null>(null);
  const [uri, setUri] = useState<string | null>(null);
  const [code, setCode] = useState("");
  const [recovery, setRecovery] = useState<string[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function startSetup() {
    setError(null);
    setBusy(true);
    try {
      const res = await setupTwoFactor();
      setSecret(res.secret);
      setUri(res.otpauth_uri);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not start setup.");
    } finally {
      setBusy(false);
    }
  }

  async function confirmEnable(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const res = await enableTwoFactor(code);
      setRecovery(res.recovery_codes);
      setSecret(null);
      setCode("");
      await queryClient.invalidateQueries({ queryKey: ["me"] });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Invalid code.");
    } finally {
      setBusy(false);
    }
  }

  async function disable(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await disableTwoFactor(code);
      setCode("");
      await queryClient.invalidateQueries({ queryKey: ["me"] });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Invalid code.");
    } finally {
      setBusy(false);
    }
  }

  if (recovery) {
    return (
      <Card className="max-w-lg">
        <h2 className="text-lg font-semibold text-slate-800">Two-factor enabled ✓</h2>
        <p className="mt-2 text-sm text-slate-600">
          Save these recovery codes somewhere safe — each works once if you lose your device.
        </p>
        <ul className="mt-4 grid grid-cols-2 gap-2 font-mono text-sm">
          {recovery.map((c) => (
            <li key={c} className="rounded bg-slate-100 px-2 py-1">
              {c}
            </li>
          ))}
        </ul>
      </Card>
    );
  }

  return (
    <Card className="max-w-lg space-y-4">
      <div>
        <h2 className="text-lg font-semibold text-slate-800">Two-factor authentication</h2>
        <p className="text-sm text-slate-500">
          Status: {user?.two_factor_enabled ? "Enabled" : "Disabled"}
        </p>
      </div>

      {user?.two_factor_enabled ? (
        <form onSubmit={disable} className="space-y-3">
          <Field label="Enter a code to disable">
            <Input value={code} onChange={(e) => setCode(e.target.value)} required />
          </Field>
          {error && <ErrorNote>{error}</ErrorNote>}
          <Button type="submit" disabled={busy} className="bg-red-600 hover:bg-red-700">
            {busy ? "Disabling…" : "Disable 2FA"}
          </Button>
        </form>
      ) : secret ? (
        <form onSubmit={confirmEnable} className="space-y-3">
          <p className="text-sm text-slate-600">
            Add this secret to your authenticator app, then enter the 6-digit code:
          </p>
          <code className="block break-all rounded bg-slate-100 px-3 py-2 text-sm">{secret}</code>
          {uri && (
            <a href={uri} className="text-xs text-brand hover:underline">
              Open in authenticator
            </a>
          )}
          <Field label="Code">
            <Input value={code} onChange={(e) => setCode(e.target.value)} required />
          </Field>
          {error && <ErrorNote>{error}</ErrorNote>}
          <Button type="submit" disabled={busy}>
            {busy ? "Enabling…" : "Verify & enable"}
          </Button>
        </form>
      ) : (
        <>
          {error && <ErrorNote>{error}</ErrorNote>}
          <Button onClick={startSetup} disabled={busy}>
            {busy ? "…" : "Enable two-factor"}
          </Button>
        </>
      )}
    </Card>
  );
}
