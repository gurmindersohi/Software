"""Stripe subscriptions + webhook (tasks 4.5/4.6). The webhook drives the
trial/subscription lifecycle on the Account (overlaps task 3.9)."""
from fastapi import APIRouter, Depends, HTTPException, Request, status
from sqlmodel import Session, select

from app.api.deps import get_current_account
from app.core.config import settings
from app.db.session import get_session
from app.integrations import stripe_service
from app.models.account import Account
from app.schemas.integrations import SubscriptionRequest, SubscriptionResult

router = APIRouter(prefix="/api/v1/payments", tags=["payments"])

_PAID_STATUSES = {"active", "trialing"}
_HOLD_STATUSES = {"past_due", "unpaid"}


@router.post("/create-subscription", response_model=SubscriptionResult)
def create_subscription(
    body: SubscriptionRequest,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> SubscriptionResult:
    if not settings.stripe_secret_key:
        raise HTTPException(status.HTTP_503_SERVICE_UNAVAILABLE, "Billing is not configured.")
    if not account.customer_id:
        account.customer_id = stripe_service.create_customer(
            account.email or "", account.account_name
        )
    sub = stripe_service.create_subscription(account.customer_id, body.price_id)
    account.subscription_id = sub["id"]
    account.is_account_paid = sub["status"] in _PAID_STATUSES
    session.add(account)
    session.commit()
    return SubscriptionResult(subscription_id=sub["id"], status=sub["status"])


@router.post("/webhook")
async def stripe_webhook(
    request: Request, session: Session = Depends(get_session)
) -> dict:
    payload = await request.body()
    sig = request.headers.get("stripe-signature", "")
    try:
        event = stripe_service.construct_event(payload, sig)
    except Exception as exc:  # signature/verification failure
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Invalid webhook.") from exc
    _apply_event(session, event)
    return {"received": True}


def _apply_event(session: Session, event) -> None:
    event_type = event["type"]
    obj = event["data"]["object"]
    customer_id = obj.get("customer")
    if not customer_id:
        return
    account = session.exec(
        select(Account).where(Account.customer_id == customer_id)
    ).first()
    if account is None:
        return

    if event_type in ("customer.subscription.created", "customer.subscription.updated"):
        sub_status = obj.get("status")
        account.subscription_id = obj.get("id")
        account.is_account_paid = sub_status in _PAID_STATUSES
        account.on_hold = sub_status in _HOLD_STATUSES
    elif event_type == "customer.subscription.deleted":
        account.is_account_paid = False
        account.on_hold = True
    elif event_type == "invoice.payment_failed":
        account.on_hold = True
    elif event_type == "invoice.payment_succeeded":
        account.on_hold = False
        account.is_account_paid = True

    session.add(account)
    session.commit()
