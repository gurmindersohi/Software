// Minimal Tailwind UI primitives (the Phase 6.2 base kit; can be swapped for
// shadcn/ui later). Kept tiny and dependency-free.
import * as React from "react";

export function Button({
  className = "",
  ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={
        "inline-flex items-center justify-center rounded-md bg-brand px-4 py-2 " +
        "text-sm font-medium text-white transition hover:bg-brand-dark " +
        "disabled:cursor-not-allowed disabled:opacity-50 " +
        className
      }
      {...props}
    />
  );
}

export const Input = React.forwardRef<
  HTMLInputElement,
  React.InputHTMLAttributes<HTMLInputElement>
>(function Input({ className = "", ...props }, ref) {
  return (
    <input
      ref={ref}
      className={
        "w-full rounded-md border border-slate-300 px-3 py-2 text-sm " +
        "outline-none focus:border-brand focus:ring-1 focus:ring-brand " +
        className
      }
      {...props}
    />
  );
});

export function Card({ children, className = "" }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={"rounded-xl border border-slate-200 bg-white p-6 shadow-sm " + className}>
      {children}
    </div>
  );
}

export function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block space-y-1">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      {children}
    </label>
  );
}
