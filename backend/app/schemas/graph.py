"""Input schemas for Graph-proxy write endpoints."""
from typing import List, Optional

from pydantic import BaseModel, Field


class PagePostInput(BaseModel):
    message: str
    link: Optional[str] = None


class CampaignCreateInput(BaseModel):
    name: str
    objective: str = "LINK_CLICKS"
    status: str = "PAUSED"


class AdsetCreateInput(BaseModel):
    name: str
    campaign_id: str
    daily_budget: int = Field(ge=100, description="minor currency units (e.g. cents)")
    optimization_goal: str = "LINK_CLICKS"
    billing_event: str = "IMPRESSIONS"
    age_min: int = 18
    age_max: int = 65
    country_codes: List[str] = ["US"]
    interest_ids: List[str] = []
    status: str = "PAUSED"


class AdCreateInput(BaseModel):
    name: str
    adset_id: str
    page_id: str
    message: str
    link: str
    headline: Optional[str] = None
    status: str = "PAUSED"
