"""Publish + due-selection logic (tasks 5.2/5.3) — no Redis, via a fake Graph."""
from datetime import datetime, timedelta

from app.core import crypto
from app.models.account import Account
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia
from app.services import scheduled_posts as svc


class FakeGraph:
    def __init__(self, token=None, fail=False):
        self.fail = fail

    def create_post(self, page_id, token, message, link=None):
        if self.fail:
            raise RuntimeError("boom")
        return {"id": "ext_fb"}

    def create_photo_post(self, page_id, token, image_url, message=""):
        return {"id": "ext_photo"}

    def create_video_post(self, page_id, token, video_url, message=""):
        return {"id": "ext_video"}

    def create_instagram_media(self, ig, token, image_url, caption):
        return {"id": "container_1"}

    def publish_instagram_media(self, ig, token, creation_id):
        return {"id": "ext_ig"}


def _seed(session, *, platform="facebook", delta=timedelta(minutes=-1), attempts=0, image_url=None):
    account = Account(account_name="A")
    session.add(account)
    session.flush()
    conn = SocialMedia(
        type=platform, page_id="p1", access_token=crypto.encrypt("TKN"), account_id=account.id
    )
    session.add(conn)
    session.flush()
    post = ScheduledPost(
        account_id=account.id,
        social_media_id=conn.id,
        platform=platform,
        message="Hi",
        image_url=image_url,
        attempts=attempts,
        scheduled_at=datetime.utcnow() + delta,
        status="pending",
    )
    session.add(post)
    session.commit()
    session.refresh(post)
    return post


def test_publish_facebook_success(session):
    post = _seed(session)
    svc.publish_post(session, post, lambda token: FakeGraph(token))
    assert post.status == "published"
    assert post.external_post_id == "ext_fb"


def test_publish_instagram_success(session):
    post = _seed(session, platform="instagram")
    svc.publish_post(session, post, lambda token: FakeGraph(token))
    assert post.status == "published"
    assert post.external_post_id == "ext_ig"


def test_publish_facebook_photo(session):
    post = _seed(session, image_url="http://img/x.jpg")
    svc.publish_post(session, post, lambda token: FakeGraph(token))
    assert post.status == "published"
    assert post.external_post_id == "ext_photo"  # routed to photo post, not feed


def test_publish_failure_requeues_then_deadletters(session):
    post = _seed(session, attempts=0)
    svc.publish_post(session, post, lambda token: FakeGraph(token, fail=True))
    assert post.attempts == 1
    assert post.status == "pending"  # retried

    post.attempts = svc.MAX_ATTEMPTS - 1
    session.add(post)
    session.commit()
    svc.publish_post(session, post, lambda token: FakeGraph(token, fail=True))
    assert post.attempts == svc.MAX_ATTEMPTS
    assert post.status == "failed"  # dead-lettered
    assert "boom" in post.last_error


def test_select_due_posts_excludes_future(session):
    due = _seed(session, delta=timedelta(minutes=-5))
    future = _seed(session, delta=timedelta(hours=2))
    ids = {p.id for p in svc.select_due_posts(session)}
    assert due.id in ids
    assert future.id not in ids


def test_worker_settings_register_functions():
    from app.worker.settings import WorkerSettings
    from app.worker.tasks import publish_scheduled_post

    assert publish_scheduled_post in WorkerSettings.functions
    assert WorkerSettings.cron_jobs  # due-post sweep registered
