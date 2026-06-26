"""Media upload + library (tasks 4.7 / gap #7). Files go to object storage and
are tracked per-tenant so the post/ad builders can browse them."""
from fastapi import APIRouter, Depends, File, Query, UploadFile, status
from sqlmodel import Session

from app.api.deps import get_current_account
from app.core import storage
from app.core.tenancy import page_owned
from app.db.session import get_session
from app.models.account import Account
from app.models.media_asset import MediaAsset
from app.schemas.integrations import MediaAssetRead, MediaUploadResponse
from app.schemas.pagination import Page

router = APIRouter(prefix="/api/v1/media", tags=["media"])


def _kind(content_type: str) -> str:
    if content_type.startswith("image/"):
        return "image"
    if content_type.startswith("video/"):
        return "video"
    return "file"


@router.get("", response_model=Page[MediaAssetRead])
def list_media(
    limit: int = Query(50, ge=1, le=200),
    offset: int = Query(0, ge=0),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Page[MediaAssetRead]:
    items, total = page_owned(session, MediaAsset, account.id, limit=limit, offset=offset)
    return Page(items=items, total=total, limit=limit, offset=offset)


@router.post("/upload", response_model=MediaUploadResponse, status_code=status.HTTP_201_CREATED)
async def upload_media(
    file: UploadFile = File(...),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> MediaUploadResponse:
    data = await file.read()
    content_type = file.content_type or "application/octet-stream"
    key = f"{account.id}/{storage.new_key(file.filename or 'upload.bin')}"
    url = storage.get_storage().save(key, data, content_type)
    session.add(
        MediaAsset(
            account_id=account.id,
            url=url,
            key=key,
            content_type=content_type,
            kind=_kind(content_type),
        )
    )
    session.commit()
    return MediaUploadResponse(url=url, key=key)
