"""Scheduled-post domain logic (tasks 5.2/5.3). Pure functions — no Redis — so
the publish + due-selection paths are unit-tested directly."""
from datetime import datetime, timezone
from typing import Callable, List, Optional
from uuid import UUID

from sqlmodel import Session, select

from app.core import crypto
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia

MAX_ATTEMPTS = 3

# A factory that builds a Graph-like client from a (decrypted) page token.
GraphFactory = Callable[[Optional[str]], object]


def _naive_utc(dt: datetime) -> datetime:
    """Store/compare scheduled times as naive UTC (portable across SQLite/PG)."""
    if dt.tzinfo is not None:
        return dt.astimezone(timezone.utc).replace(tzinfo=None)
    return dt


def create_scheduled_post(
    session: Session,
    *,
    account_id: UUID,
    social_media_id: UUID,
    platform: str,
    scheduled_at: datetime,
    message: Optional[str] = None,
    link: Optional[str] = None,
    image_url: Optional[str] = None,
) -> ScheduledPost:
    post = ScheduledPost(
        account_id=account_id,
        social_media_id=social_media_id,
        platform=platform,
        message=message,
        link=link,
        image_url=image_url,
        scheduled_at=_naive_utc(scheduled_at),
        status="pending",
    )
    session.add(post)
    session.commit()
    session.refresh(post)
    return post


def select_due_posts(session: Session, now: Optional[datetime] = None) -> List[ScheduledPost]:
    moment = _naive_utc(now or datetime.now(timezone.utc))
    return session.exec(
        select(ScheduledPost).where(
            ScheduledPost.status == "pending",
            ScheduledPost.scheduled_at <= moment,
        )
    ).all()


def publish_post(
    session: Session, post: ScheduledPost, graph_factory: GraphFactory
) -> ScheduledPost:
    """Publish one scheduled post. On failure, increments attempts and either
    re-queues (status=pending) or dead-letters (status=failed) past MAX_ATTEMPTS."""
    connection = session.get(SocialMedia, post.social_media_id)
    try:
        if connection is None:
            raise RuntimeError("Social connection no longer exists.")
        token = crypto.decrypt(connection.access_token) if connection.access_token else None
        graph = graph_factory(token)

        if post.platform == "instagram":
            container = graph.create_instagram_media(
                connection.page_id, token, post.image_url or "", post.message or ""
            )
            result = graph.publish_instagram_media(connection.page_id, token, container["id"])
        else:
            result = graph.create_post(connection.page_id, token, post.message or "", post.link)

        post.external_post_id = result.get("id")
        post.status = "published"
        post.last_error = None
    except Exception as exc:  # noqa: BLE001 — record any publish failure
        post.attempts += 1
        post.last_error = str(exc)
        post.status = "failed" if post.attempts >= MAX_ATTEMPTS else "pending"

    post.modified_on = datetime.now(timezone.utc)
    session.add(post)
    session.commit()
    session.refresh(post)
    return post
