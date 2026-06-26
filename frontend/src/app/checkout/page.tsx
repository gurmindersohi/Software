import { Suspense } from "react";

import CheckoutInner from "./checkout-inner";

export default function CheckoutPage() {
  return (
    <main className="mx-auto max-w-md px-6 py-20">
      <Suspense fallback={<p className="text-slate-500">Loading…</p>}>
        <CheckoutInner />
      </Suspense>
    </main>
  );
}
