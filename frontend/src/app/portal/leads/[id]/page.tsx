"use client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";

import { LeadForm } from "@/components/lead-form";
import { PageHeader } from "@/components/portal";
import { Button } from "@/components/ui";
import { deleteLead, getLead, updateLead } from "@/lib/leads";

export default function EditLeadPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: lead, isLoading, isError } = useQuery({
    queryKey: ["leads", id],
    queryFn: () => getLead(id),
  });

  async function refreshAndLeave() {
    await queryClient.invalidateQueries({ queryKey: ["leads"] });
    router.push("/portal/leads");
  }

  if (isLoading) return <p className="text-slate-500">Loading…</p>;
  if (isError || !lead) return <p className="text-red-600">Lead not found.</p>;

  return (
    <div>
      <PageHeader
        title="Edit lead"
        action={
          <Button
            className="bg-red-600 hover:bg-red-700"
            onClick={async () => {
              if (!confirm("Delete this lead?")) return;
              await deleteLead(id);
              await refreshAndLeave();
            }}
          >
            Delete
          </Button>
        }
      />
      <LeadForm
        submitLabel="Save changes"
        initial={lead}
        onSubmit={async (data) => {
          await updateLead(id, data);
          await refreshAndLeave();
        }}
      />
    </div>
  );
}
