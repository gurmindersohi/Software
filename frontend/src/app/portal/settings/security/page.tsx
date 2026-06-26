"use client";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import {
  changeEmail,
  deleteAccount,
  disableTwoFactor,
  enableTwoFactor,
  exportPersonalData,
  setupTwoFactor,
} from "@/lib/auth";
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
    <div className="max-w-lg space-y-6">
    <Card className="space-y-4">
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

      <ChangeEmailCard />
      <PrivacyCard />
    </div>
  );
}

function ChangeEmailCard() {
  const queryClient = useQueryClient();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [msg, setMsg] = useState<string | null>(null);
  const [err, setErr] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setMsg(null);
    setBusy(true);
    try {
      await changeEmail(email, password);
      setMsg("Email updated — check your inbox to confirm the new address.");
      setEmail("");
      setPassword("");
      await queryClient.invalidateQueries({ queryKey: ["me"] });
    } catch (e2) {
      setErr(e2 instanceof ApiError ? e2.message : "Could not change email.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <Card className="space-y-4">
      <h2 className="text-lg font-semibold text-slate-800">Change email</h2>
      <form onSubmit={submit} className="space-y-3">
        <Field label="New email">
          <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </Field>
        <Field label="Current password">
          <Input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </Field>
        {err && <ErrorNote>{err}</ErrorNote>}
        {msg && <p className="text-sm text-green-600">{msg}</p>}
        <Button type="submit" disabled={busy}>
          {busy ? "Saving…" : "Update email"}
        </Button>
      </form>
    </Card>
  );
}

function PrivacyCard() {
  const router = useRouter();
  const [password, setPassword] = useState("");
  const [err, setErr] = useState<string | null>(null);

  async function exportData() {
    const data = await exportPersonalData();
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "my-data.json";
    a.click();
    URL.revokeObjectURL(url);
  }

  async function remove(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    if (!confirm("Permanently delete your account? This cannot be undone.")) return;
    try {
      await deleteAccount(password);
      router.push("/login");
    } catch (e2) {
      setErr(e2 instanceof ApiError ? e2.message : "Could not delete account.");
    }
  }

  return (
    <Card className="space-y-4">
      <h2 className="text-lg font-semibold text-slate-800">Privacy</h2>
      <Button onClick={exportData} className="bg-slate-600 hover:bg-slate-700">
        Download my data
      </Button>
      <form onSubmit={remove} className="space-y-3 border-t border-slate-200 pt-4">
        <p className="text-sm text-slate-600">Delete your account and scrub personal data.</p>
        <Field label="Confirm password">
          <Input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </Field>
        {err && <ErrorNote>{err}</ErrorNote>}
        <Button type="submit" className="bg-red-600 hover:bg-red-700">
          Delete account
        </Button>
      </form>
    </Card>
  );
}
