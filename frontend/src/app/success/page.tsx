import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = { title: "Success — Sohi" };

export default function SuccessPage() {
  return (
    <main className="mx-auto max-w-md px-6 py-24 text-center">
      <div className="text-5xl">🎉</div>
      <h1 className="mt-4 text-3xl font-bold text-slate-900">You&apos;re all set</h1>
      <p className="mt-3 text-slate-600">Your subscription is active. Welcome aboard!</p>
      <Link
        href="/portal"
        className="mt-8 inline-block rounded-md bg-brand px-6 py-3 text-sm font-medium text-white hover:bg-brand-dark"
      >
        Go to dashboard
      </Link>
    </main>
  );
}
