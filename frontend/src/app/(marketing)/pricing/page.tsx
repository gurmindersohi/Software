import type { Metadata } from "next";
import Link from "next/link";

import { PLANS } from "@/lib/plans";

export const metadata: Metadata = { title: "Pricing — Sohi" };

export default function PricingPage() {
  return (
    <main className="mx-auto max-w-6xl px-6 py-20">
      <h1 className="text-center text-3xl font-bold text-slate-900">
        Select your suitable pricing plan
      </h1>
      <p className="mt-3 text-center text-slate-600">Start free, upgrade when you grow.</p>

      <div className="mt-12 grid gap-6 md:grid-cols-3">
        {PLANS.map((plan) => (
          <div
            key={plan.id}
            className={
              "flex flex-col rounded-2xl border bg-white p-8 " +
              (plan.highlighted ? "border-brand shadow-lg ring-1 ring-brand" : "border-slate-200")
            }
          >
            {plan.highlighted && (
              <span className="mb-3 inline-block w-fit rounded-full bg-brand/10 px-3 py-1 text-xs font-medium text-brand">
                Most popular
              </span>
            )}
            <h2 className="text-xl font-semibold text-slate-900">{plan.name}</h2>
            <p className="mt-2 text-4xl font-bold text-slate-900">
              ${plan.price}
              <span className="text-base font-normal text-slate-500">/mo</span>
            </p>
            <ul className="mt-6 flex-1 space-y-2 text-sm text-slate-600">
              {plan.features.map((feature) => (
                <li key={feature} className="flex items-start gap-2">
                  <span className="mt-0.5 text-brand">✓</span>
                  {feature}
                </li>
              ))}
            </ul>
            <Link
              href={`/checkout?plan=${plan.id}`}
              className={
                "mt-8 rounded-md px-4 py-2 text-center text-sm font-medium " +
                (plan.highlighted
                  ? "bg-brand text-white hover:bg-brand-dark"
                  : "border border-slate-300 text-slate-700 hover:bg-slate-50")
              }
            >
              Choose {plan.name}
            </Link>
          </div>
        ))}
      </div>
    </main>
  );
}
