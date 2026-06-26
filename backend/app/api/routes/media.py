"""Media upload → object storage (task 4.7). Used by social posts and ad
creatives. Files are namespaced under the tenant's account id."""
from fastapi import APIRouter, Depends, File, UploadFile, status

from app.api.deps import get_current_account
from app.core import storage
from app.models.account import Account
from app.schemas.integrations import MediaUploadResponse

router = APIRouter(prefix="/api/v1/media", tags=["media"])


@router.post("/upload", response_model=MediaUploadResponse, status_code=status.HTTP_201_CREATED)
async def upload_media(
    file: UploadFile = File(...),
    account: Account = Depends(get_current_account),
) -> MediaUploadResponse:
    data = await file.read()
    key = f"{account.id}/{storage.new_key(file.filename or 'upload.bin')}"
    backend = storage.get_storage()
    url = backend.save(key, data, file.content_type or "application/octet-stream")
    return MediaUploadResponse(url=url, key=key)
