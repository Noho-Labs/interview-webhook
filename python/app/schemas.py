from pydantic import BaseModel
from typing import Any, Dict, Optional

class StripeEvent(BaseModel):
    id: str
    type: str
    data: Dict[str, Any]  # keep simple for exercise

class WebhookResponse(BaseModel):
    ok: bool
    duplicate: bool = False
    processed: bool = False
    error: Optional[str] = None