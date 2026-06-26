"use client";
import Link from "next/link";
import { usePathname } from "next/navigation";

const TABS = [
  { href: "/portal/social", label: "Queue" },
  { href: "/portal/social/pages", label: "Pages" },
];

export default function SocialLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold text-slate-900">Social</h1>
      <nav className="mb-6 flex gap-1 border-b border-slate-200">
        {TABS.map((tab) => {
          const active =
            tab.href === "/portal/social"
              ? pathname === tab.href
              : pathname.startsWith(tab.href);
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
