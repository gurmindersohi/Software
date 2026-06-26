"""Scheduled-post (Queue) CRUD, scoped to the caller's tenant (task 5.2/5.3).

Creating a row is all the API does; the arq worker sweep publishes due rows.
`GET` exposes status/attempts/last_error for the Queue UI (5.3 visibility)."""
from typing import List
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session, select

from app.api.deps import get_current_account
from app.db.session import get_session
from app.models.account import Account
from app.models.scheduled_post import ScheduledPost
from app.models.social import SocialMedia
from app.schemas.scheduled_post import ScheduledPostCreate, ScheduledPostRead
from app.services import scheduled_posts as svc

router = APIRouter(prefix="/api/v1/scheduled-posts", tags=["scheduled-posts"])


def _get_owned(session: Session, post_id: UUID, account: Account) -> ScheduledPost:
    post = session.get(ScheduledPost, post_id)
    if post is None or post.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Scheduled post not found.")
    return post


@router.get("", response_model=List[ScheduledPostRead])
def list_posts(
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> List[ScheduledPost]:
    return session.exec(
        select(ScheduledPost).where(ScheduledPost.account_id == account.id)
    ).all()


@router.post("", response_model=ScheduledPostRead, status_code=status.HTTP_201_CREATED)
def create_post(
    body: ScheduledPostCreate,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> ScheduledPost:
    # The target social connection must belong to this tenant.
    connection = session.get(SocialMedia, body.social_media_id)
    if connection is None or connection.account_id != account.id:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Social connection not found.")
    return svc.create_scheduled_post(
        session,
        account_id=account.id,
        social_media_id=body.social_media_id,
        platform=body.platform,
        scheduled_at=body.scheduled_at,
        message=body.message,
        link=body.link,
        image_url=body.image_url,
    )


@router.get("/{post_id}", response_model=ScheduledPostRead)
def get_post(
    post_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> ScheduledPost:
    return _get_owned(session, post_id, account)


@router.delete("/{post_id}", status_code=status.HTTP_204_NO_CONTENT)
def cancel_post(
    post_id: UUID,
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> None:
    post = _get_owned(session, post_id, account)
    if post.status == "published":
        raise HTTPException(status.HTTP_409_CONFLICT, "Cannot cancel an already-published post.")
    session.delete(post)
    session.commit()
