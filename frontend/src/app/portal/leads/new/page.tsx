"use client";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";

import { LeadForm } from "@/components/lead-form";
import { PageHeader } from "@/components/portal";
import { createLead } from "@/lib/leads";

export default function NewLeadPage() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return (
    <div>
      <PageHeader title="New lead" />
      <LeadForm
        submitLabel="Create lead"
        onSubmit={async (data) => {
          await createLead(data);
          await queryClient.invalidateQueries({ queryKey: ["leads"] });
          router.push("/portal/leads");
        }}
      />
    </div>
  );
}
