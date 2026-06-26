import Link from "next/link";

import { Button } from "@/components/ui";

// Landing placeholder. The full marketing site is Phase 7.
export default function Home() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-6 px-6 text-center">
      <h1 className="text-4xl font-bold tracking-tight text-slate-900">
        Social Media &amp; Ads Manager
      </h1>
      <p className="max-w-xl text-lg text-slate-600">
        Post, schedule, target, and measure across Facebook and Instagram — with
        reusable audiences and team access.
      </p>
      <div className="flex gap-3">
        <Link href="/register">
          <Button>Get started free</Button>
        </Link>
        <Link href="/login">
          <Button className="bg-slate-700 hover:bg-slate-800">Sign in</Button>
        </Link>
      </div>
    </main>
  );
}
