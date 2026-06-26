import Link from "next/link";

const HIGHLIGHTS = [
  "Create posts on multiple accounts with one click",
  "Schedule posts on Facebook and Instagram",
  "Create Facebook & Instagram ads",
  "Build and reuse targeted audiences",
  "Real-time social and ads insights",
  "Manage your team in one place",
];

export default function HomePage() {
  return (
    <main>
      <section className="mx-auto max-w-4xl px-6 py-24 text-center">
        <h1 className="text-5xl font-bold tracking-tight text-slate-900">
          Social Media &amp; Ads Manager
        </h1>
        <p className="mx-auto mt-6 max-w-2xl text-lg text-slate-600">
          Post, schedule, target, and measure across Facebook and Instagram — organic and
          paid — from a single dashboard, with reusable audiences and team access.
        </p>
        <div className="mt-8 flex justify-center gap-3">
          <Link
            href="/register"
            className="rounded-md bg-brand px-6 py-3 text-sm font-medium text-white hover:bg-brand-dark"
          >
            Get started for free
          </Link>
          <Link
            href="/pricing"
            className="rounded-md border border-slate-300 px-6 py-3 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            See pricing
          </Link>
        </div>
      </section>

      <section className="border-t border-slate-200 bg-white">
        <div className="mx-auto max-w-5xl px-6 py-16">
          <h2 className="text-center text-2xl font-bold text-slate-900">Everything in one place</h2>
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {HIGHLIGHTS.map((text) => (
              <div key={text} className="rounded-xl border border-slate-200 p-5">
                <p className="text-slate-700">{text}</p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </main>
  );
}
