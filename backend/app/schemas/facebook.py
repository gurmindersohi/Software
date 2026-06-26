"""Transport schemas mirroring Meta Graph API payloads.

These port the C# classes that were *not* EF entities — they describe data
exchanged with Facebook/Instagram, not rows in our DB. Used by the Graph
integration layer (Phase 4) and the ads/social routers.
"""
from datetime import datetime
from decimal import Decimal
from typing import List, Optional

from pydantic import BaseModel, Field


class Campaign(BaseModel):
    id: Optional[str] = None
    name: str
    objective: str = "LINK_CLICKS"
    status: bool = False
    status_string: Optional[str] = None
    start_time: Optional[datetime] = None


class Adset(BaseModel):
    id: Optional[str] = None
    name: Optional[str] = None
    optimization_goal: str = "LINK_CLICKS"
    destination_type: str = "WEBSITE"
    budget: str = "daily_budget"
    daily_budget: Decimal = Decimal("10")
    campaign_id: Optional[str] = None
    targeting: Optional[str] = None
    status: Optional[str] = None
    start_date: Optional[datetime] = None
    start_time: Optional[datetime] = None
    time_zone: str = "PDT"
    schedule_end_date: bool = False
    end_date: Optional[datetime] = None
    end_time: Optional[datetime] = None
    min_age: int = 18
    max_age: int = 65
    gender: str = "All"
    placements: str = "Auto"
    location_type: str = "RecentHome"
    location: Optional[str] = None
    billing_events: str = "IMPRESSIONS"
    cost_control: Decimal = Decimal("0")


class Ad(BaseModel):
    id: Optional[str] = None
    name: Optional[str] = None
    ad_setup: str = "CreateAd"
    ad_type: str = "Single"
    primary_text: Optional[str] = None
    destination: str = "Website"
    headline: Optional[str] = None
    website_url: Optional[str] = None  # fixes original "WebsitrUrl" typo
    description: Optional[str] = None
    display_link: Optional[str] = None
    call_to_action: str = "LEARN_MORE"


class AdImage(BaseModel):
    id: Optional[str] = None
    name: Optional[str] = None
    hash: Optional[str] = None
    url: Optional[str] = None
    height: Optional[str] = None
    width: Optional[str] = None


class Targeting(BaseModel):
    id: Optional[str] = None
    name: Optional[str] = None
    type: Optional[str] = None
    audience_size: Optional[str] = None
    path: List[str] = Field(default_factory=list)


class FacebookLocation(BaseModel):
    key: Optional[str] = None
    name: Optional[str] = None
    type: Optional[str] = None
    country_code: Optional[str] = None
    country_name: Optional[str] = None
    region: Optional[str] = None
    region_id: Optional[str] = None
    primary_city: Optional[str] = None
    primary_city_id: Optional[str] = None
    supports_region: Optional[str] = None
    supports_city: Optional[str] = None
    latitude: Optional[str] = None
    longitude: Optional[str] = None
    radius: str = "5"
    distance_unit: str = "mile"


class Profile(BaseModel):
    id: Optional[str] = None
    name: Optional[str] = None
    image: Optional[str] = None
    token: Optional[str] = None
    type: Optional[str] = None
    is_active: bool = False


class PostInsights(BaseModel):
    id: Optional[str] = None
    post_reactions_like_total: Optional[str] = None
    post_engaged_users: Optional[str] = None
    post_impressions: Optional[str] = None
    post_clicks: Optional[str] = None


class Post(BaseModel):
    id: Optional[str] = None
    description: Optional[str] = None
    picture: Optional[str] = None
    created_time: Optional[str] = None
    media_type: Optional[str] = None
    insights: Optional[PostInsights] = None
    profile: Optional[Profile] = None


class InsightValue(BaseModel):
    value: Optional[str] = None
    end_time: Optional[datetime] = None


class PageInsights(BaseModel):
    id: Optional[str] = None
    title: Optional[str] = None
    description: Optional[str] = None
    name: Optional[str] = None
    period: Optional[str] = None
    total_value: int = 0
    values: List[InsightValue] = Field(default_factory=list)
