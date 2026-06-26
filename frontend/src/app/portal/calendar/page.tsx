"use client";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";

import { PageHeader } from "@/components/portal";
import { Button } from "@/components/ui";
import { listScheduledPosts, type ScheduledPost } from "@/lib/scheduled";

const DOW = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const STATUS_COLOR: Record<string, string> = {
  pending: "bg-amber-100 text-amber-700",
  queued: "bg-blue-100 text-blue-700",
  published: "bg-green-100 text-green-700",
  failed: "bg-red-100 text-red-700",
};

export default function CalendarPage() {
  const posts = useQuery({ queryKey: ["scheduled"], queryFn: listScheduledPosts });
  const [month, setMonth] = useState(() => {
    const d = new Date();
    return new Date(d.getFullYear(), d.getMonth(), 1);
  });

  // Group posts by YYYY-MM-DD of their local scheduled date.
  const byDay = new Map<string, ScheduledPost[]>();
  for (const p of posts.data?.items ?? []) {
    const key = new Date(p.scheduled_at).toDateString();
    byDay.set(key, [...(byDay.get(key) ?? []), p]);
  }

  const firstDow = month.getDay();
  const daysInMonth = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate();
  const cells: (Date | null)[] = [
    ...Array(firstDow).fill(null),
    ...Array.from({ length: daysInMonth }, (_, i) => new Date(month.getFullYear(), month.getMonth(), i + 1)),
  ];

  return (
    <div>
      <PageHeader
        title="Content calendar"
        action={
          <div className="flex items-center gap-3">
            <Button
              className="bg-slate-600 hover:bg-slate-700"
              onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() - 1, 1))}
            >
              ‹
            </Button>
            <span className="text-sm font-medium text-slate-700">
              {month.toLocaleString(undefined, { month: "long", year: "numeric" })}
            </span>
            <Button
              className="bg-slate-600 hover:bg-slate-700"
              onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1))}
            >
              ›
            </Button>
          </div>
        }
      />

      <div className="grid grid-cols-7 gap-px overflow-hidden rounded-lg border border-slate-200 bg-slate-200 text-sm">
        {DOW.map((d) => (
          <div key={d} className="bg-slate-50 px-2 py-2 text-center text-xs font-medium text-slate-500">
            {d}
          </div>
        ))}
        {cells.map((date, i) => (
          <div key={i} className="min-h-[96px] bg-white p-1.5">
            {date && (
              <>
                <div className="mb-1 text-xs text-slate-400">{date.getDate()}</div>
                <div className="space-y-1">
                  {(byDay.get(date.toDateString()) ?? []).map((p) => (
                    <div
                      key={p.id}
                      className={
                        "truncate rounded px-1.5 py-0.5 text-xs " +
                        (STATUS_COLOR[p.status] ?? "bg-slate-100 text-slate-600")
                      }
                      title={p.message ?? ""}
                    >
                      {p.message || "(post)"}
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
