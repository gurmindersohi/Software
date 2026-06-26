"""White-labeled PDF client reports (Tier 2)."""
import io
from datetime import datetime, timezone

from reportlab.lib.pagesizes import letter
from reportlab.lib.units import inch
from reportlab.pdfgen import canvas
from sqlmodel import Session

from app.core.tenancy import count_owned
from app.models.account import Account
from app.models.client import Client
from app.models.lead import Lead
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia


def client_report_pdf(session: Session, account: Account, client: Client) -> bytes:
    """A simple, agency-branded performance summary for one client."""
    metrics = [
        (
            "Connected pages",
            count_owned(session, SocialMedia, account.id, SocialMedia.client_id == client.id),
        ),
        ("Leads", count_owned(session, Lead, account.id, Lead.client_id == client.id)),
        (
            "Scheduled posts",
            count_owned(
                session,
                ScheduledPost,
                account.id,
                (ScheduledPost.client_id == client.id)
                & (ScheduledPost.status.in_(["pending", "queued"])),
            ),
        ),
        (
            "Published posts",
            count_owned(
                session,
                ScheduledPost,
                account.id,
                (ScheduledPost.client_id == client.id) & (ScheduledPost.status == "published"),
            ),
        ),
    ]

    buf = io.BytesIO()
    pdf = canvas.Canvas(buf, pagesize=letter)
    _, height = letter
    y = height - inch

    pdf.setFont("Helvetica-Bold", 18)
    pdf.drawString(inch, y, account.account_name or "Agency")  # white-label header
    y -= 0.32 * inch
    pdf.setFont("Helvetica", 12)
    pdf.drawString(inch, y, f"Performance report — {client.name}")
    y -= 0.22 * inch
    pdf.setFont("Helvetica", 9)
    pdf.drawString(inch, y, f"Generated {datetime.now(timezone.utc):%Y-%m-%d}")

    y -= 0.55 * inch
    pdf.setFont("Helvetica-Bold", 13)
    pdf.drawString(inch, y, "Summary")
    y -= 0.32 * inch
    pdf.setFont("Helvetica", 11)
    for label, value in metrics:
        pdf.drawString(1.2 * inch, y, f"{label}: {value}")
        y -= 0.26 * inch

    pdf.showPage()
    pdf.save()
    return buf.getvalue()
