"""SQLModel tables. Importing here registers them on SQLModel.metadata
so Alembic autogenerate and create_all see every table."""
from app.models.account import Account
from app.models.ad_account import AdAccount
from app.models.lead import Lead
from app.models.plan import Plan
from app.models.social import SocialMedia

__all__ = ["Account", "AdAccount", "Lead", "Plan", "SocialMedia"]
