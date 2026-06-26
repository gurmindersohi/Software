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
    _init()
    sub = stripe.Subscription.create(customer=customer_id, items=[{"price": price_id}])
    return {"id": sub.id, "status": sub.status}


def construct_event(payload: bytes, sig_header: str):
    """Verify + parse a webhook event. Patched in tests to bypass signatures."""
    return stripe.Webhook.construct_event(payload, sig_header, settings.stripe_webhook_secret)
