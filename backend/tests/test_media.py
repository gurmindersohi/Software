"""Media upload to object storage (task 4.7) using the local backend."""
from app.core.config import settings
from tests.conftest import register_confirm_login


def test_upload_writes_file_and_returns_url(client, session, tmp_path, monkeypatch):
    monkeypatch.setattr(settings, "storage_backend", "local")
    monkeypatch.setattr(settings, "local_storage_dir", str(tmp_path))
    register_confirm_login(client, session)

    files = {"file": ("hello.txt", b"hello world", "text/plain")}
    resp = client.post("/api/v1/media/upload", files=files)
    assert resp.status_code == 201

    body = resp.json()
    assert body["key"].endswith(".txt")
    assert body["url"].startswith("/api/v1/media/file/")  # fetchable, not file://

    written = list(tmp_path.rglob("*.txt"))
    assert len(written) == 1
    assert written[0].read_bytes() == b"hello world"


def test_serve_file_round_trip_and_tenant_guard(client, session, tmp_path, monkeypatch):
    from fastapi.testclient import TestClient

    from app.main import app

    monkeypatch.setattr(settings, "storage_backend", "local")
    monkeypatch.setattr(settings, "local_storage_dir", str(tmp_path))
    register_confirm_login(client, session)
    url = client.post(
        "/api/v1/media/upload", files={"file": ("a.txt", b"payload", "text/plain")}
    ).json()["url"]

    served = client.get(url)
    assert served.status_code == 200
    assert served.content == b"payload"

    other = TestClient(app)
    register_confirm_login(other, session, email="other@acme.com")
    assert other.get(url).status_code == 404  # cross-tenant blocked by key prefix


def test_upload_records_asset_and_library_lists_it(client, session, tmp_path, monkeypatch):
    monkeypatch.setattr(settings, "storage_backend", "local")
    monkeypatch.setattr(settings, "local_storage_dir", str(tmp_path))
    register_confirm_login(client, session)

    client.post("/api/v1/media/upload", files={"file": ("pic.png", b"\x89PNG", "image/png")})
    library = client.get("/api/v1/media")
    assert library.status_code == 200
    body = library.json()
    assert body["total"] == 1
    assert body["items"][0]["kind"] == "image"


def test_upload_requires_auth(client):
    files = {"file": ("x.txt", b"x", "text/plain")}
    assert client.post("/api/v1/media/upload", files=files).status_code == 401
