"""arq task wrappers around the pure scheduled-post logic (task 5.1/5.2).

The DB work is synchronous (SQLModel), so it runs in a thread via
`asyncio.to_thread` to avoid blocking the worker's event loop."""
import asyncio
from typing import List
from uuid import UUID

from sqlmodel import Session

from app.db.session import engine
from app.integrations.facebook.graph import GraphClient
from app.models.scheduled_post import ScheduledPost
from app.services.scheduled_posts import publish_post, select_due_posts


def _graph_factory(token):
    return GraphClient(access_token=token)


def _publish_sync(post_id: str) -> str:
    with Session(engine) as session:
        post = session.get(ScheduledPost, UUID(post_id))
        if post is None:
            return "missing"
        publish_post(session, post, _graph_factory)
        return post.status


def _claim_due_sync() -> List[str]:
    with Session(engine) as session:
        due = select_due_posts(session)
        for post in due:
            post.status = "queued"
            session.add(post)
        session.commit()
        return [str(post.id) for post in due]


async def publish_scheduled_post(ctx, post_id: str) -> str:
    """Publish a single scheduled post by id (DB work off the event loop)."""
    return await asyncio.to_thread(_publish_sync, post_id)


async def enqueue_due_posts(ctx) -> int:
    """Cron sweep: claim due posts (status -> queued) and enqueue publish jobs."""
    ids = await asyncio.to_thread(_claim_due_sync)
    redis = ctx["redis"]
    for post_id in ids:
        await redis.enqueue_job("publish_scheduled_post", post_id)
    return len(ids)
