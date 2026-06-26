import type { Metadata } from "next";

export const metadata: Metadata = { title: "About — Sohi" };

const TEAM = [
  { name: "Engineering", role: "Software Engineer" },
  { name: "Engineering", role: "Software Engineer" },
  { name: "Quality", role: "Software Testing Engineer" },
];

export default function AboutPage() {
  return (
    <main className="mx-auto max-w-4xl px-6 py-20">
      <h1 className="text-3xl font-bold text-slate-900">Our business</h1>
      <p className="mt-4 max-w-2xl text-slate-600">
        Sohi helps small businesses and agencies manage their Facebook and Instagram
        presence — organic and paid — without juggling Meta&apos;s native tools. One dashboard
        to post, schedule, target, and measure.
      </p>

      <h2 className="mt-12 text-2xl font-bold text-slate-900">Our team</h2>
      <div className="mt-6 grid gap-6 sm:grid-cols-3">
        {TEAM.map((member, i) => (
          <div key={i} className="rounded-xl border border-slate-200 bg-white p-6 text-center">
            <div className="mx-auto mb-3 h-16 w-16 rounded-full bg-slate-100" />
            <p className="font-medium text-slate-800">{member.name}</p>
            <p className="text-sm text-slate-500">{member.role}</p>
          </div>
        ))}
      </div>
    </main>
  );
}
