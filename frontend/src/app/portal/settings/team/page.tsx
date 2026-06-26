"use client";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import { ErrorNote } from "@/components/portal";
import { Button, Card, Field, Input } from "@/components/ui";
import { ApiError } from "@/lib/api";
import {
  createRole,
  inviteMember,
  listRoles,
  listTeam,
  removeMember,
} from "@/lib/team";

export default function TeamPage() {
  const queryClient = useQueryClient();
  const team = useQuery({ queryKey: ["team"], queryFn: listTeam });
  const roles = useQuery({ queryKey: ["roles"], queryFn: listRoles });

  const [email, setEmail] = useState("");
  const [role, setRole] = useState("User");
  const [newRole, setNewRole] = useState("");

  const invite = useMutation({
    mutationFn: () => inviteMember({ email, role }),
    onSuccess: async () => {
      setEmail("");
      await queryClient.invalidateQueries({ queryKey: ["team"] });
    },
  });

  const addRole = useMutation({
    mutationFn: () => createRole(newRole),
    onSuccess: async () => {
      setNewRole("");
      await queryClient.invalidateQueries({ queryKey: ["roles"] });
    },
  });

  const remove = useMutation({
    mutationFn: (id: string) => removeMember(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["team"] });
    },
  });

  return (
    <div className="space-y-8">
      <section>
        <h2 className="mb-3 text-lg font-semibold text-slate-800">Members</h2>
        <Card className="overflow-x-auto p-0">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-200 text-slate-500">
              <tr>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Roles</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {team.data?.map((member) => (
                <tr key={member.id} className="border-b border-slate-100">
                  <td className="px-4 py-3">{member.email}</td>
                  <td className="px-4 py-3 text-slate-600">{member.roles.join(", ") || "—"}</td>
                  <td className="px-4 py-3 text-slate-600">
                    {member.is_deleted
                      ? "Removed"
                      : member.email_confirmed
                        ? "Active"
                        : "Invited"}
                  </td>
                  <td className="px-4 py-3 text-right">
                    {!member.is_deleted && (
                      <button
                        className="text-sm text-red-600 hover:underline"
                        onClick={() => remove.mutate(member.id)}
                      >
                        Remove
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      </section>

      <section className="grid gap-6 md:grid-cols-2">
        <Card>
          <h3 className="mb-3 font-medium text-slate-800">Invite a member</h3>
          <form
            className="space-y-3"
            onSubmit={(e) => {
              e.preventDefault();
              invite.mutate();
            }}
          >
            <Field label="Email">
              <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </Field>
            <label className="block text-sm">
              <span className="text-slate-600">Role</span>
              <select
                className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                value={role}
                onChange={(e) => setRole(e.target.value)}
              >
                {(roles.data ?? [{ id: "u", name: "User" }]).map((r) => (
                  <option key={r.id} value={r.name}>
                    {r.name}
                  </option>
                ))}
              </select>
            </label>
            {invite.isError && (
              <ErrorNote>
                {invite.error instanceof ApiError ? invite.error.message : "Could not invite."}
              </ErrorNote>
            )}
            <Button type="submit" disabled={invite.isPending}>
              {invite.isPending ? "Inviting…" : "Send invite"}
            </Button>
          </form>
        </Card>

        <Card>
          <h3 className="mb-3 font-medium text-slate-800">Roles</h3>
          <ul className="mb-3 space-y-1 text-sm text-slate-700">
            {roles.data?.map((r) => (
              <li key={r.id}>• {r.name}</li>
            ))}
          </ul>
          <form
            className="flex items-end gap-2"
            onSubmit={(e) => {
              e.preventDefault();
              addRole.mutate();
            }}
          >
            <Field label="New role">
              <Input value={newRole} onChange={(e) => setNewRole(e.target.value)} required />
            </Field>
            <Button type="submit" disabled={addRole.isPending}>
              Add
            </Button>
          </form>
          {addRole.isError && (
            <ErrorNote>
              {addRole.error instanceof ApiError ? addRole.error.message : "Could not add role."}
            </ErrorNote>
          )}
        </Card>
      </section>
    </div>
  );
}
