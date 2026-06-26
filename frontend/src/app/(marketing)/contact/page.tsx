"use client";
import { useState } from "react";

import { Button, Card, Field, Input } from "@/components/ui";

export default function ContactPage() {
  const [sent, setSent] = useState(false);

  return (
    <main className="mx-auto max-w-md px-6 py-20">
      <h1 className="text-3xl font-bold text-slate-900">Get in touch</h1>
      <p className="mt-3 text-slate-600">
        Questions about plans or features? Send us a message.
      </p>

      <Card className="mt-8">
        {sent ? (
          <p className="text-slate-700">Thanks — we&apos;ll get back to you shortly.</p>
        ) : (
          <form
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              setSent(true);
            }}
          >
            <Field label="Name">
              <Input required />
            </Field>
            <Field label="Email">
              <Input type="email" required />
            </Field>
            <Field label="Message">
              <textarea
                required
                rows={4}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand focus:ring-1 focus:ring-brand"
              />
            </Field>
            <Button type="submit" className="w-full">
              Send message
            </Button>
          </form>
        )}
      </Card>
    </main>
  );
}
