"""Media upload + library (task 4.7 / gap #7). Files go to object storage and
are tracked per-tenant. `public_url` yields a fetchable URL (S3 presigned in
prod; an app-served path for local dev) so Graph can publish the media."""
from fastapi import APIRouter, Depends, File, HTTPException, Query, UploadFile, status
from fastapi.responses import FileResponse
from sqlmodel import Session

from app.api.deps import get_current_account
from app.core import storage
from app.core.config import settings
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


def _to_read(asset: MediaAsset, backend: storage.StorageBackend) -> MediaAssetRead:
    return MediaAssetRead(
        id=asset.id,
        url=backend.public_url(asset.key),
        kind=asset.kind,
        content_type=asset.content_type,
    )


@router.get("", response_model=Page[MediaAssetRead])
def list_media(
    limit: int = Query(50, ge=1, le=200),
    offset: int = Query(0, ge=0),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> Page[MediaAssetRead]:
    items, total = page_owned(session, MediaAsset, account.id, limit=limit, offset=offset)
    backend = storage.get_storage()
    return Page(
        items=[_to_read(a, backend) for a in items], total=total, limit=limit, offset=offset
    )


@router.post("/upload", response_model=MediaUploadResponse, status_code=status.HTTP_201_CREATED)
async def upload_media(
    file: UploadFile = File(...),
    account: Account = Depends(get_current_account),
    session: Session = Depends(get_session),
) -> MediaUploadResponse:
    data = await file.read()
    content_type = file.content_type or "application/octet-stream"
    key = f"{account.id}/{storage.new_key(file.filename or 'upload.bin')}"
    backend = storage.get_storage()
    stored = backend.save(key, data, content_type)
    session.add(
        MediaAsset(
            account_id=account.id,
            url=stored,
            key=key,
            content_type=content_type,
            kind=_kind(content_type),
        )
    )
    session.commit()
    return MediaUploadResponse(url=backend.public_url(key), key=key)


@router.get("/file/{key:path}")
def serve_file(
    key: str,
    account: Account = Depends(get_current_account),
) -> FileResponse:
    """Serve a local-storage file (dev). Tenant-scoped by the key prefix."""
    if settings.storage_backend != "local":
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Not found.")
    if not key.startswith(f"{account.id}/"):
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Not found.")
    backend = storage.get_storage()
    assert isinstance(backend, storage.LocalStorage)
    try:
        path = backend.resolve(key)
    except ValueError as exc:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Not found.") from exc
    if not path.exists():
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Not found.")
    return FileResponse(path)
