"""Object storage abstraction (task 4.7).

Local filesystem backend for dev/tests; S3-compatible (MinIO/AWS) for prod.
boto3 is imported lazily so the local path needs no AWS deps.
"""
import os
import uuid
from pathlib import Path
from typing import Protocol

from app.core.config import settings


class StorageBackend(Protocol):
    def save(self, key: str, data: bytes, content_type: str = ...) -> str: ...


class LocalStorage:
    def __init__(self, base_dir: str) -> None:
        self.base = Path(base_dir)
        self.base.mkdir(parents=True, exist_ok=True)

    def save(self, key: str, data: bytes, content_type: str = "application/octet-stream") -> str:
        path = self.base / key
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        return path.resolve().as_uri()


class S3Storage:
    def __init__(self) -> None:
        import boto3  # lazy — only needed for the S3 backend

        self._s3 = boto3.client(
            "s3",
            endpoint_url=settings.s3_endpoint_url,
            aws_access_key_id=settings.s3_access_key,
            aws_secret_access_key=settings.s3_secret_key,
        )
        self._bucket = settings.s3_bucket

    def save(self, key: str, data: bytes, content_type: str = "application/octet-stream") -> str:
        self._s3.put_object(Bucket=self._bucket, Key=key, Body=data, ContentType=content_type)
        base = settings.s3_endpoint_url or "https://s3.amazonaws.com"
        return f"{base}/{self._bucket}/{key}"


def get_storage() -> StorageBackend:
    if settings.storage_backend == "s3":
        return S3Storage()
    return LocalStorage(settings.local_storage_dir)


def new_key(filename: str) -> str:
    ext = os.path.splitext(filename)[1]
    return f"{uuid.uuid4().hex}{ext}"
