"""Phase 1 sanity: the ported tables persist, relate, and keep money precision."""
from decimal import Decimal

from app.models import Account, AdAccount, Lead, Plan, SocialMedia


def test_account_and_lead_fk(session):
    acc = Account(account_name="Acme Agency", email="ops@acme.test")
    session.add(acc)
    session.commit()
    session.refresh(acc)

    lead = Lead(email="jane@lead.test", first_name="Jane", account_id=acc.id)
    session.add(lead)
    session.commit()
    session.refresh(lead)

    assert lead.account_id == acc.id
    assert lead.is_member is False
    assert acc.is_account_paid is False
    assert acc.created_on is not None  # AuditMixin default applied


def test_plan_decimal_precision(session):
    plan = Plan(
        name="Premium",
        type="recurring",
        price=Decimal("99.00"),
        tax=Decimal("12.87"),
        total=Decimal("111.87"),
        billing_period="monthly",
    )
    session.add(plan)
    session.commit()
    session.refresh(plan)

    assert plan.price == Decimal("99.00")
    assert plan.total == Decimal("111.87")


def test_connected_accounts_scoped_to_tenant(session):
    acc = Account(account_name="Tenant")
    session.add(acc)
    session.commit()
    session.refresh(acc)

    sm = SocialMedia(name="Page", type="facebook", account_id=acc.id, page_id="123")
    ad = AdAccount(name="Ads", user_account_id="act_123", account_id=acc.id)
    session.add(sm)
    session.add(ad)
    session.commit()
    session.refresh(sm)
    session.refresh(ad)

    assert sm.account_id == acc.id
    assert ad.account_id == acc.id
