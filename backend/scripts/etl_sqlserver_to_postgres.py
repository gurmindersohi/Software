"""One-time ETL: SQL Server (old .NET app) → PostgreSQL (new schema) — task 1.4/9.1.

Reads the legacy EF / ASP.NET Identity tables and writes the new SQLModel tables.
Highlights:
  * Password hashes are copied **verbatim** — the new login path verifies the
    ASP.NET Identity format and re-hashes on first login (no password resets).
  * Social / ad-account tokens are **encrypted at rest** during load.
  * GUIDs are preserved as primary keys; Plan money keeps Numeric(18,2).

Usage:
    export SOURCE_DATABASE_URL="mssql+pyodbc://user:pass@host/Sohi?driver=ODBC+Driver+18+for+SQL+Server"
    export DATABASE_URL="postgresql+psycopg://sohi:sohi@localhost:5432/sohi"
    python scripts/etl_sqlserver_to_postgres.py --dry-run    # validate, no writes
    python scripts/etl_sqlserver_to_postgres.py              # commit

Requires a SQL Server driver (pyodbc or pymssql) installed in the environment.
Run `alembic upgrade head` on the target first.
"""
import argparse
import logging
import os
from datetime import datetime
from decimal import Decimal
from typing import Optional
from uuid import UUID

from sqlalchemy import create_engine, text
from sqlmodel import Session, select

from app.core import crypto
from app.db.session import engine as target_engine
from app.models.account import Account
from app.models.ad_account import AdAccount
from app.models.lead import Lead
from app.models.plan import Plan
from app.models.social import SocialMedia
from app.models.user import Role, User, UserRoleLink

logging.basicConfig(level=logging.INFO, format="%(levelname)s %(message)s")
log = logging.getLogger("etl")


# --- coercion helpers -----------------------------------------------------
def as_uuid(value) -> Optional[UUID]:
    return UUID(str(value)) if value else None


def as_bool(value) -> bool:
    if isinstance(value, bool):
        return value
    return str(value).strip().lower() in {"1", "true", "yes"}


def as_decimal(value) -> Decimal:
    return Decimal(str(value)) if value is not None else Decimal("0")


def rows(src, table: str):
    return list(src.execute(text(f"SELECT * FROM {table}")).mappings())


# --- per-table migrations -------------------------------------------------
def migrate_accounts(src, dst) -> int:
    for r in rows(src, "Accounts"):
        dst.add(
            Account(
                id=as_uuid(r.get("AccountId")),
                account_name=r.get("AccountName"),
                account_type=r.get("AccountType"),
                email=r.get("Email"),
                phone=r.get("Phone"),
                address=r.get("Address"),
                city=r.get("City"),
                province=r.get("Province"),
                country=r.get("Country"),
                postal_code=r.get("PostalCode"),
                users_limit=r.get("UsersLimit"),
                logo=r.get("Logo"),
                trial_expiry=r.get("TrialExpiry"),
                is_account_paid=as_bool(r.get("IsAccountPaid")),
                is_deleted=as_bool(r.get("IsDeleted")),
                on_hold=as_bool(r.get("OnHold")),
                hold_date=r.get("HoldDate"),
                customer_id=r.get("CustomerId"),
                subscription_id=r.get("SubscriptionId"),
            )
        )
    return _flush(dst, "Accounts")


def migrate_plans(src, dst) -> int:
    for r in rows(src, "Plans"):
        dst.add(
            Plan(
                id=as_uuid(r.get("Id")),
                name=r.get("Name"),
                type=r.get("Type"),
                price=as_decimal(r.get("Price")),
                billing_period=r.get("BillingPeriod"),
                product_id=r.get("ProductId"),
                tax=as_decimal(r.get("Tax")),
                total=as_decimal(r.get("Total")),
            )
        )
    return _flush(dst, "Plans")


def migrate_roles(src, dst) -> int:
    for r in rows(src, "AspNetRoles"):
        dst.add(Role(id=as_uuid(r.get("Id")), name=r.get("Name")))
    return _flush(dst, "AspNetRoles")


def migrate_users(src, dst) -> int:
    for r in rows(src, "AspNetUsers"):
        dst.add(
            User(
                id=as_uuid(r.get("Id")),
                email=r.get("Email"),
                normalized_email=r.get("NormalizedEmail") or (r.get("Email") or "").upper(),
                password_hash=r.get("PasswordHash") or "",  # preserved verbatim
                email_confirmed=as_bool(r.get("EmailConfirmed")),
                first_name=r.get("FirstName"),
                last_name=r.get("LastName"),
                date_of_birth=r.get("DateOfBirth"),
                gender=r.get("Gender"),
                photo_path=r.get("PhotoPath"),
                phone_number=r.get("PhoneNumber"),
                two_factor_enabled=as_bool(r.get("TwoFactorEnabled")),
                access_failed_count=int(r.get("AccessFailedCount") or 0),
                account_id=as_uuid(r.get("AccountId")),
                is_deleted=as_bool(r.get("IsDeleted")),
            )
        )
    return _flush(dst, "AspNetUsers")


def migrate_user_roles(src, dst) -> int:
    for r in rows(src, "AspNetUserRoles"):
        dst.add(UserRoleLink(user_id=as_uuid(r.get("UserId")), role_id=as_uuid(r.get("RoleId"))))
    return _flush(dst, "AspNetUserRoles")


def migrate_leads(src, dst) -> int:
    for r in rows(src, "Leads"):
        dst.add(
            Lead(
                id=as_uuid(r.get("LeadId")),
                first_name=r.get("FirstName"),
                last_name=r.get("LastName"),
                full_name=r.get("FullName"),
                email=r.get("Email"),
                primary_phone=r.get("PrimaryPhone"),
                secondary_phone=r.get("SecondaryPhone"),
                date_of_birth=r.get("DateOfBirth"),
                gender=r.get("Gender"),
                address=r.get("Address"),
                city=r.get("City"),
                province=r.get("Province"),
                country=r.get("Country"),
                postal_code=r.get("PostalCode"),
                account_id=as_uuid(r.get("AccountId")),
                lead_source=r.get("LeadSource"),
                is_phone_call_allowed=as_bool(r.get("IsPhoneCallAllowed")),
                is_email_allowed=as_bool(r.get("IsEmailAllowed")),
                is_text_allowed=as_bool(r.get("IsTextAllowed")),
                is_member=as_bool(r.get("IsMember")),
            )
        )
    return _flush(dst, "Leads")


def migrate_social(src, dst) -> int:
    for r in rows(src, "SocialMediaAccounts"):
        dst.add(
            SocialMedia(
                id=as_uuid(r.get("Id")),
                page_id=r.get("PageId"),
                name=r.get("Name"),
                image=r.get("Image"),
                type=r.get("Type"),
                access_token=crypto.encrypt_optional(r.get("AccessToken")),
                secret=crypto.encrypt_optional(r.get("Secret")),
                token_expiry_date=r.get("TokenExpiryDate"),
                email=r.get("Email"),
                user_id=r.get("UserId"),
                account_id=as_uuid(r.get("AccountId")),
            )
        )
    return _flush(dst, "SocialMediaAccounts")


def migrate_ad_accounts(src, dst) -> int:
    for r in rows(src, "AdAccounts"):
        dst.add(
            AdAccount(
                id=as_uuid(r.get("Id")),
                user_account_id=r.get("UserAccountId"),
                name=r.get("Name"),
                image=r.get("Image"),
                type=r.get("Type"),
                access_token=crypto.encrypt_optional(r.get("AccessToken")),
                secret=crypto.encrypt_optional(r.get("Secret")),
                token_expiry_date=r.get("TokenExpiryDate"),
                email=r.get("Email"),
                user_id=r.get("UserId"),
                account_id=as_uuid(r.get("AccountId")),
            )
        )
    return _flush(dst, "AdAccounts")


def _flush(dst: Session, label: str) -> int:
    dst.flush()
    log.info("  staged %s", label)
    return 0  # count reported by verify()


# Order respects FKs: accounts/plans/roles → users → user_roles → child tables.
MIGRATIONS = [
    migrate_accounts,
    migrate_plans,
    migrate_roles,
    migrate_users,
    migrate_user_roles,
    migrate_leads,
    migrate_social,
    migrate_ad_accounts,
]

VERIFY = {
    "Accounts": Account,
    "Plans": Plan,
    "AspNetRoles": Role,
    "AspNetUsers": User,
    "Leads": Lead,
    "SocialMediaAccounts": SocialMedia,
    "AdAccounts": AdAccount,
}


def verify(src, dst: Session) -> bool:
    ok = True
    log.info("Row-count verification (source → target):")
    for table, model in VERIFY.items():
        src_n = src.execute(text(f"SELECT COUNT(*) FROM {table}")).scalar_one()
        dst_n = len(dst.exec(select(model)).all())
        flag = "OK" if src_n == dst_n else "MISMATCH"
        if src_n != dst_n:
            ok = False
        log.info("  %-22s %6s → %-6s [%s]", table, src_n, dst_n, flag)
    return ok


def main() -> None:
    parser = argparse.ArgumentParser(description="SQL Server → Postgres ETL")
    parser.add_argument("--dry-run", action="store_true", help="validate without committing")
    args = parser.parse_args()

    source_url = os.environ.get("SOURCE_DATABASE_URL")
    if not source_url:
        raise SystemExit("Set SOURCE_DATABASE_URL to the SQL Server connection string.")

    started = datetime.now()
    src_engine = create_engine(source_url)
    with src_engine.connect() as src, Session(target_engine) as dst:
        for migration in MIGRATIONS:
            migration(src, dst)
        ok = verify(src, dst)
        if args.dry_run:
            dst.rollback()
            log.info("DRY RUN — rolled back, no data written.")
        elif ok:
            dst.commit()
            log.info("Committed.")
        else:
            dst.rollback()
            raise SystemExit("Verification failed — rolled back. Inspect mismatches above.")

    log.info("Done in %s", datetime.now() - started)


if __name__ == "__main__":
    main()
