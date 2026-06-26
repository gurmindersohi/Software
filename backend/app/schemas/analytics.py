"""Analytics (time-series snapshot) schemas."""
from datetime import datetime

from pydantic import BaseModel


class SnapshotRead(BaseModel):
    metric: str
    value: int
    captured_at: datetime

    model_config = {"from_attributes": True}
