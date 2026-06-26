"""Input schemas for Graph-proxy write endpoints."""
from typing import Optional

from pydantic import BaseModel


class PagePostInput(BaseModel):
    message: str
    link: Optional[str] = None


class CampaignCreateInput(BaseModel):
    name: str
    objective: str = "LINK_CLICKS"
    status: str = "PAUSED"
