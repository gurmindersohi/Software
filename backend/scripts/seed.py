"""Seed reference data into a fresh database (task 1.5).

  python scripts/seed.py           # the 3 subscription plans
  python scripts/seed.py --demo    # plans + a ready-to-use demo owner account

Run `alembic upgrade head` first. Idempotent — safe to re-run.
"""
import argparse
from datetime import datetime, timedelta, timezone
from decimal import Decimal

from sqlmodel import Session, select

from app.core import security
from app.db.session import engine
from app.models.account import Account
from app.models.plan import Plan
from app.models.user import Role, User

PLANS = [
    {"name": "Basic", "type": "basic", "price": "24"},
    {"name": "Premium", "type": "premium", "price": "99"},
    {"name": "Unlimited", "type": "unlimited", "price": "299"},
]

DEMO_EMAIL = "demo@sohi.app"
DEMO_PASSWORD = "demo1234"


def seed_plans(session: Session) -> int:
    created = 0
    for spec in PLANS:
        if session.exec(select(Plan).where(Plan.name == spec["name"])).first() is None:
            price = Decimal(spec["price"])
            session.add(
                Plan(
                    name=spec["name"],
                    type=spec["type"],
                    price=price,
                    billing_period="month",
                    tax=Decimal("0"),
                    total=price,
                )
            )
            created += 1
    session.commit()
    return created


def seed_demo(session: Session) -> bool:
    if session.exec(select(User).where(User.normalized_email == DEMO_EMAIL.upper())).first():
        return False
    account = Account(
        account_name="Demo Co",
        email=DEMO_EMAIL,
        plan_name="premium",
        is_account_paid=True,
        trial_expiry=datetime.now(timezone.utc) + timedelta(days=14),
    )
    session.add(account)
    session.flush()

    role = session.exec(select(Role).where(Role.name == "Owner")).first()
    if role is None:
        role = Role(name="Owner")
        session.add(role)
        session.flush()

    user = User(
        email=DEMO_EMAIL,
        normalized_email=DEMO_EMAIL.upper(),
        password_hash=security.hash_password(DEMO_PASSWORD),
        first_name="Demo",
        account_id=account.id,
        email_confirmed=True,
    )
    user.roles.append(role)
    session.add(user)
    session.commit()
    return True


def main() -> None:
    parser = argparse.ArgumentParser(description="Seed reference data")
    parser.add_argument("--demo", action="store_true", help="also create a demo owner account")
    args = parser.parse_args()

    with Session(engine) as session:
        print(f"plans: {seed_plans(session)} created")
        if args.demo:
            if seed_demo(session):
                print(f"demo account: created ({DEMO_EMAIL} / {DEMO_PASSWORD})")
            else:
                print("demo account: already exists")


if __name__ == "__main__":
    main()
