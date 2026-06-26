"use client";
import { useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";

import { logout } from "@/lib/auth";
import { useCurrentUser } from "@/lib/hooks";

const NAV = [
  { href: "/portal", label: "Dashboard" },
  { href: "/portal/leads", label: "Leads" },
  { href: "/portal/social", label: "Social" },
  { href: "/portal/ads", label: "Ads" },
  { href: "/portal/settings", label: "Settings" },
];

export default function PortalLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: user } = useCurrentUser();

  async function onLogout() {
    await logout();
    queryClient.clear();
    router.push("/login");
  }

  return (
    <div className="flex min-h-screen">
      <aside className="w-56 shrink-0 border-r border-slate-200 bg-white">
        <div className="px-5 py-4 text-lg font-bold text-brand">Sohi</div>
        <nav className="flex flex-col gap-1 px-3">
          {NAV.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="rounded-md px-3 py-2 text-sm text-slate-700 hover:bg-slate-100"
            >
              {item.label}
            </Link>
          ))}
        </nav>
      </aside>
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3">
          <span className="text-sm text-slate-500">{user?.email ?? ""}</span>
          <button
            onClick={onLogout}
            className="text-sm font-medium text-slate-600 hover:text-slate-900"
          >
            Sign out
          </button>
        </header>
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  );
}
