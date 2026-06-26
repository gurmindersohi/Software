"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";

const TABS = [
  { href: "/portal/settings", label: "General" },
  { href: "/portal/settings/connections", label: "Connections" },
  { href: "/portal/settings/team", label: "Team" },
  { href: "/portal/settings/security", label: "Security" },
  { href: "/portal/settings/billing", label: "Billing" },
];

export default function SettingsLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold text-slate-900">Settings</h1>
      <nav className="mb-6 flex gap-1 border-b border-slate-200">
        {TABS.map((tab) => {
          const active = pathname === tab.href;
          return (
            <Link
              key={tab.href}
              href={tab.href}
              className={
                "px-4 py-2 text-sm font-medium " +
                (active
                  ? "border-b-2 border-brand text-brand"
                  : "text-slate-500 hover:text-slate-800")
              }
            >
              {tab.label}
            </Link>
          );
        })}
      </nav>
      {children}
    </div>
  );
}
