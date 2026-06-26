"use client";
import { useMutation } from "@tanstack/react-query";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import {
  createAd,
  createAdset,
  searchTargeting,
  type Campaign,
  type TargetingResult,
} from "@/lib/ads";
import type { SocialConnection } from "@/lib/connections";

function errMsg(e: unknown) {
  return e instanceof ApiError ? e.message : "Something went wrong.";
}

export function AdSetForm({
  adAccountId,
  campaigns,
  onCreated,
}: {
  adAccountId: string;
  campaigns: Campaign[];
  onCreated: (id: string) => void;
}) {
  const [name, setName] = useState("");
  const [campaignId, setCampaignId] = useState("");
  const [budget, setBudget] = useState("10");
  const [ageMin, setAgeMin] = useState(18);
  const [ageMax, setAgeMax] = useState(65);
  const [countries, setCountries] = useState("US");
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<TargetingResult[]>([]);
  const [interests, setInterests] = useState<TargetingResult[]>([]);

  const create = useMutation({
    mutationFn: () =>
      createAdset(adAccountId, {
        name,
        campaign_id: campaignId,
        daily_budget: Math.round(parseFloat(budget || "0") * 100),
        age_min: ageMin,
        age_max: ageMax,
        country_codes: countries.split(",").map((c) => c.trim()).filter(Boolean),
        interest_ids: interests.map((i) => i.id),
      }),
    onSuccess: (res) => onCreated(res.id),
  });

  async function runSearch() {
    if (!query) return;
    try {
      setResults(await searchTargeting(adAccountId, query));
    } catch {
      setResults([]);
    }
  }

  return (
    <Card>
      <h3 className="mb-3 font-medium text-slate-800">Create ad set</h3>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          create.mutate();
        }}
        className="space-y-3"
      >
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Name">
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </Field>
          <label className="text-sm">
            <span className="text-slate-600">Campaign</span>
            <select
              className="block w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              value={campaignId}
              onChange={(e) => setCampaignId(e.target.value)}
              required
            >
              <option value="">Select…</option>
              {campaigns.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </label>
          <Field label="Daily budget ($)">
            <Input
              type="number"
              min="1"
              step="0.01"
              value={budget}
              onChange={(e) => setBudget(e.target.value)}
            />
          </Field>
          <Field label="Countries (comma)">
            <Input value={countries} onChange={(e) => setCountries(e.target.value)} />
          </Field>
          <Field label="Age min">
            <Input
              type="number"
              value={ageMin}
              onChange={(e) => setAgeMin(Number(e.target.value))}
            />
          </Field>
          <Field label="Age max">
            <Input
              type="number"
              value={ageMax}
              onChange={(e) => setAgeMax(Number(e.target.value))}
            />
          </Field>
        </div>

        <div>
          <span className="text-sm text-slate-600">Interest targeting</span>
          <div className="mt-1 flex gap-2">
            <Input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search interests…"
            />
            <Button type="button" onClick={runSearch} className="bg-slate-600 hover:bg-slate-700">
              Search
            </Button>
          </div>
          {results.length > 0 && (
            <ul className="mt-2 space-y-1 text-sm">
              {results.map((r) => (
                <li key={r.id}>
                  <button
                    type="button"
                    className="text-brand hover:underline"
                    onClick={() => {
                      if (!interests.find((i) => i.id === r.id)) setInterests([...interests, r]);
                      setResults([]);
                      setQuery("");
                    }}
                  >
                    + {r.name}
                  </button>
                </li>
              ))}
            </ul>
          )}
          {interests.length > 0 && (
            <div className="mt-2 flex flex-wrap gap-2">
              {interests.map((i) => (
                <span key={i.id} className="rounded-full bg-brand/10 px-3 py-1 text-xs text-brand">
                  {i.name}
                  <button
                    type="button"
                    className="ml-1"
                    onClick={() => setInterests(interests.filter((x) => x.id !== i.id))}
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          )}
        </div>

        {create.isError && <ErrorNote>{errMsg(create.error)}</ErrorNote>}
        {create.isSuccess && <p className="text-sm text-green-600">Ad set created (paused).</p>}
        <Button type="submit" disabled={create.isPending}>
          {create.isPending ? "Creating…" : "Create ad set"}
        </Button>
      </form>
    </Card>
  );
}

export function AdForm({
  adAccountId,
  pages,
  defaultAdsetId,
}: {
  adAccountId: string;
  pages: SocialConnection[];
  defaultAdsetId: string;
}) {
  const [name, setName] = useState("");
  const [adsetId, setAdsetId] = useState(defaultAdsetId);
  const [pageId, setPageId] = useState("");
  const [message, setMessage] = useState("");
  const [link, setLink] = useState("");
  const [headline, setHeadline] = useState("");
  const [imageUrl, setImageUrl] = useState("");

  const create = useMutation({
    mutationFn: () =>
      createAd(adAccountId, {
        name,
        adset_id: adsetId || defaultAdsetId,
        page_id: pageId,
        message,
        link,
        headline: headline || undefined,
        image_url: imageUrl || undefined,
      }),
  });

  return (
    <Card>
      <h3 className="mb-3 font-medium text-slate-800">Create ad</h3>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          create.mutate();
        }}
        className="space-y-3"
      >
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Name">
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </Field>
          <Field label="Ad set id">
            <Input
              value={adsetId}
              onChange={(e) => setAdsetId(e.target.value)}
              placeholder={defaultAdsetId || "adset id"}
              required
            />
          </Field>
          <label className="text-sm">
            <span className="text-slate-600">Page</span>
            <select
              className="block w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              value={pageId}
              onChange={(e) => setPageId(e.target.value)}
              required
            >
              <option value="">Select…</option>
              {pages.map((p) => (
                <option key={p.id} value={p.page_id ?? ""}>
                  {p.name ?? p.page_id}
                </option>
              ))}
            </select>
          </label>
          <Field label="Headline">
            <Input value={headline} onChange={(e) => setHeadline(e.target.value)} />
          </Field>
        </div>
        <Field label="Primary text">
          <textarea
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            rows={2}
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            required
          />
        </Field>
        <Field label="Link">
          <Input
            type="url"
            value={link}
            onChange={(e) => setLink(e.target.value)}
            placeholder="https://…"
            required
          />
        </Field>
        <Field label="Image URL (optional)">
          <Input
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            placeholder="https://… (uploaded to the ad account)"
          />
        </Field>
        {create.isError && <ErrorNote>{errMsg(create.error)}</ErrorNote>}
        {create.isSuccess && <p className="text-sm text-green-600">Ad created (paused).</p>}
        <Button type="submit" disabled={create.isPending}>
          {create.isPending ? "Creating…" : "Create ad"}
        </Button>
      </form>
    </Card>
  );
}
