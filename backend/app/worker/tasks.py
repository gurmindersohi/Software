"""arq task wrappers around the pure scheduled-post logic (task 5.1/5.2)."""
from uuid import UUID

from sqlmodel import Session

from app.db.session import engine
from app.integrations.facebook.graph import GraphClient
from app.models.scheduled_post import ScheduledPost
from app.services.scheduled_posts import publish_post, select_due_posts


def _graph_factory(token):
    return GraphClient(access_token=token)


async def publish_scheduled_post(ctx, post_id: str) -> str:
    """Publish a single scheduled post by id."""
    with Session(engine) as session:
        post = session.get(ScheduledPost, UUID(post_id))
        if post is None:
            return "missing"
        publish_post(session, post, _graph_factory)
        return post.status


async def enqueue_due_posts(ctx) -> int:
    """Cron sweep: claim due posts (status -> queued) and enqueue publish jobs."""
    with Session(engine) as session:
        due = select_due_posts(session)
        for post in due:
            post.status = "queued"
            session.add(post)
        session.commit()
        ids = [str(post.id) for post in due]

    redis = ctx["redis"]
    for post_id in ids:
        await redis.enqueue_job("publish_scheduled_post", post_id)
    return len(ids)
