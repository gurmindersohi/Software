import type { Metadata } from "next";

export const metadata: Metadata = { title: "Features — Sohi" };

const FEATURES = [
  "Create posts on multiple accounts with just one click",
  "Schedule posts on Facebook and Instagram",
  "Manage social media accounts",
  "Get real-time insights",
  "Create Ads on Facebook and Instagram",
  "Get real-time ads insights",
  "Create your targeted audience and reuse it",
  "Reach millions of people every day",
];

export default function FeaturesPage() {
  return (
    <main className="mx-auto max-w-4xl px-6 py-20">
      <h1 className="text-3xl font-bold text-slate-900">Features</h1>
      <p className="mt-3 text-slate-600">
        Everything you need to run organic social and paid ads on Meta.
      </p>
      <ul className="mt-10 grid gap-4 sm:grid-cols-2">
        {FEATURES.map((feature) => (
          <li
            key={feature}
            className="flex items-start gap-3 rounded-lg border border-slate-200 bg-white p-4"
          >
            <span className="mt-0.5 text-brand">✓</span>
            <span className="text-slate-700">{feature}</span>
          </li>
        ))}
      </ul>
    </main>
  );
}
