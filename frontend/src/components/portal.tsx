import Link from "next/link";
import * as React from "react";

export function PageHeader({
  title,
  action,
}: {
  title: string;
  action?: React.ReactNode;
}) {
  return (
    <div className="mb-6 flex items-center justify-between">
      <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
      {action}
    </div>
  );
}

export function StatusBadge({ status }: { status: string }) {
  const colors: Record<string, string> = {
    pending: "bg-amber-100 text-amber-700",
    queued: "bg-blue-100 text-blue-700",
    published: "bg-green-100 text-green-700",
    failed: "bg-red-100 text-red-700",
  };
  return (
    <span
      className={
        "rounded-full px-2 py-0.5 text-xs font-medium " +
        (colors[status] ?? "bg-slate-100 text-slate-600")
      }
    >
      {status}
    </span>
  );
}

export function Empty({ message, cta }: { message: string; cta?: { href: string; label: string } }) {
  return (
    <div className="rounded-xl border border-dashed border-slate-300 p-10 text-center text-slate-500">
      <p>{message}</p>
      {cta && (
        <Link href={cta.href} className="mt-3 inline-block text-sm font-medium text-brand hover:underline">
          {cta.label}
        </Link>
      )}
    </div>
  );
}

export function ErrorNote({ children }: { children: React.ReactNode }) {
  return <p className="text-sm text-red-600">{children}</p>;
}

export function Sparkline({ values }: { values: number[] }) {
  if (values.length === 0) return null;
  const w = 140;
  const h = 36;
  const max = Math.max(...values, 1);
  const min = Math.min(...values, 0);
  const range = max - min || 1;
  const points = values
    .map((v, i) => {
      const x = values.length === 1 ? w : (i / (values.length - 1)) * w;
      const y = h - ((v - min) / range) * h;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(" ");
  return (
    <svg width={w} height={h} className="text-brand" aria-hidden>
      <polyline points={points} fill="none" stroke="currentColor" strokeWidth="1.5" />
    </svg>
  );
}
