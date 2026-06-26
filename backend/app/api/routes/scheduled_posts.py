"""Scheduled-post (Queue) CRUD, scoped to the caller's tenant (task 5.2/5.3).

Creating a row is all the API does; the arq worker sweep publishes due rows.
`GET` exposes status/attempts/last_error for the Queue UI (5.3 visibility)."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, status
from sqlmodel import Session

from app.api.deps import get_current_account, require_active_account
from app.core.exceptions import ConflictError
from app.core.quotas import enforce_quota
from app.core.tenancy import list_owned, owned_or_404
from app.db.session import get_session
from app.models.account import Account
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia
from app.schemas.scheduled_post import ScheduledPostCreate, ScheduledPostRead
from app.services import scheduled_posts as svc

router = APIRouter(prefix="/api/v1/scheduled-posts", tags=["scheduled-posts"])


@router.get("", response_model=List[ScheduledPostRead])
def list_posts(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[ScheduledPost]:
    return list_owned(session, ScheduledPost, account.id)


@router.post("", response_model=ScheduledPostRead, status_code=status.HTTP_201_CREATED)
def create_post(
    body: ScheduledPostCreate,
    account: Account = Depends(require_active_account),
    session: Session = Depends(get_session),
) -> ScheduledPost:
    # The target social connection must belong to this tenant.
    owned_or_404(
        session, SocialMedia, body.social_media_id, account.id, name="Social connection"
    )
    # Quota counts posts still in the pipeline (not yet published/failed).
    enforce_quota(
        session,
        plan_name=account.plan_name,
        account_id=account.id,
        resource="scheduled_posts",
        model=ScheduledPost,
        extra=ScheduledPost.status.in_(["pending", "queued"]),
    )
    return svc.create_scheduled_post(
        session,
        account_id=account.id,
        social_media_id=body.social_media_id,
        platform=body.platform,
        scheduled_at=body.scheduled_at,
        message=body.message,
        link=body.link,
        image_url=body.image_url,
        video_url=body.video_url,
    )


@router.get("/{post_id}", response_model=ScheduledPostRead)
def get_post(
    post_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> ScheduledPost:
    return owned_or_404(session, ScheduledPost, post_id, account.id, name="Scheduled post")


@router.delete("/{post_id}", status_code=status.HTTP_204_NO_CONTENT)
def cancel_post(
    post_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    post = owned_or_404(session, ScheduledPost, post_id, account.id, name="Scheduled post")
    if post.status == "published":
        raise ConflictError("Cannot cancel an already-published post.")
    session.delete(post)
    session.commit()
