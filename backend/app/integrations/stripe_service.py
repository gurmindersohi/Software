"""Stripe billing service (task 4.5/4.6). Ports the old Blazor Checkout logic
(CreateCustomerInStripe / CreateSubscriptionInStripe) to the API tier."""
from typing import Optional

import stripe

from app.core.config import settings


def _init() -> None:
    stripe.api_key = settings.stripe_secret_key


def create_customer(email: str, name: Optional[str] = None) -> str:
    _init()
    customer = stripe.Customer.create(email=email, name=name)
    return customer.id


def create_subscription(customer_id: str, price_id: str) -> dict:
    """Create an incomplete subscription and return the PaymentIntent client
    secret so the frontend can collect a card with Stripe Elements."""
    _init()
    sub = stripe.Subscription.create(
        customer=customer_id,
        items=[{"price": price_id}],
        payment_behavior="default_incomplete",
        payment_settings={"save_default_payment_method": "on_subscription"},
        expand=["latest_invoice.payment_intent"],
    )
    invoice = sub.latest_invoice
    payment_intent = getattr(invoice, "payment_intent", None) if invoice else None
    return {
        "id": sub.id,
        "status": sub.status,
        "client_secret": getattr(payment_intent, "client_secret", None),
    }


def create_portal_session(customer_id: str, return_url: str) -> str:
    """Stripe-hosted billing portal (update card, cancel, invoices)."""
    _init()
    session = stripe.billing_portal.Session.create(customer=customer_id, return_url=return_url)
    return session.url


def construct_event(payload: bytes, sig_header: str):
    """Verify + parse a webhook event. Patched in tests to bypass signatures."""
    return stripe.Webhook.construct_event(payload, sig_header, settings.stripe_webhook_secret)
