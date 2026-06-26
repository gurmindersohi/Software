"use client";
import { useQuery, useQueryClient } from "@tanstack/react-query";

import { Empty } from "@/components/portal";
import { Button, Card } from "@/components/ui";
import { ApiError } from "@/lib/api";
import {
  deleteSocialConnection,
  getFacebookConnectUrl,
  listAdAccounts,
  listSocialConnections,
} from "@/lib/connections";

export default function ConnectionsPage() {
  const queryClient = useQueryClient();
  const social = useQuery({ queryKey: ["social"], queryFn: listSocialConnections });
  const ads = useQuery({ queryKey: ["ad-accounts"], queryFn: listAdAccounts });

  async function connectFacebook() {
    try {
      const { authorize_url } = await getFacebookConnectUrl();
      window.location.href = authorize_url;
    } catch (err) {
      alert(
        err instanceof ApiError && err.status === 503
          ? "Facebook integration isn't configured yet."
          : "Could not start Facebook connection.",
      );
    }
  }

  return (
    <div className="space-y-8">
      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-slate-800">Social pages</h2>
          <Button onClick={connectFacebook}>Connect Facebook</Button>
        </div>
        {social.data && social.data.length === 0 && (
          <Empty message="No social pages connected yet." />
        )}
        <div className="grid gap-3 sm:grid-cols-2">
          {social.data?.map((conn) => (
            <Card key={conn.id} className="flex items-center justify-between">
              <div>
                <p className="font-medium text-slate-800">{conn.name ?? conn.page_id}</p>
                <p className="text-xs uppercase text-slate-400">{conn.type}</p>
              </div>
              <button
                className="text-sm text-red-600 hover:underline"
                onClick={async () => {
                  await deleteSocialConnection(conn.id);
                  await queryClient.invalidateQueries({ queryKey: ["social"] });
                }}
              >
                Disconnect
              </button>
            </Card>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold text-slate-800">Ad accounts</h2>
        {ads.data && ads.data.length === 0 && <Empty message="No ad accounts connected yet." />}
        <div className="grid gap-3 sm:grid-cols-2">
          {ads.data?.map((account) => (
            <Card key={account.id}>
              <p className="font-medium text-slate-800">{account.name ?? "Ad account"}</p>
              <p className="text-xs text-slate-400">{account.user_account_id}</p>
            </Card>
          ))}
        </div>
      </section>
    </div>
  );
}
