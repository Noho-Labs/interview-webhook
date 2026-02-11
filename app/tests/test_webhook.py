import uuid
from datetime import datetime
from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from sqlalchemy import select
from sqlalchemy.exc import IntegrityError

from app.db import get_db
from app.models import WebhookEvent
from app.schemas import StripeEvent, WebhookResponse
from app.orders import mark_order_paid

router = APIRouter()

@router.post("/stripe", response_model=WebhookResponse)
def stripe_webhook(event: StripeEvent, db: Session = Depends(get_db)) -> WebhookResponse:
    """
    TODO (Candidate):
      - Persist the event in webhook_events
      - If payment_intent.succeeded, mark order paid (order_id in metadata)
      - Return quickly with 200
      - Then handle idempotency change request (duplicate events)
    """
    # Starter: store the event (candidate can refactor)
    webhook = WebhookEvent(
        provider="stripe",
        event_id=event.id,
        type=event.type,
        payload=event.model_dump(),
    )

    try:
        db.add(webhook)
        db.commit()
    except IntegrityError:
        db.rollback()
        # Duplicate event_id for same provider
        return WebhookResponse(ok=True, duplicate=True, processed=False)

    processed = False
    error = None

    try:
        if event.type == "payment_intent.succeeded":
            # Example payload expectation:
            # event.data["object"]["metadata"]["order_id"] = "<uuid>"
            order_id_str = (
                event.data.get("object", {})
                .get("metadata", {})
                .get("order_id")
            )
            if not order_id_str:
                raise ValueError("missing_order_id")

            mark_order_paid(db, uuid.UUID(order_id_str))
            webhook.processed_at = datetime.utcnow()
            processed = True

        db.commit()
    except Exception as e:
        db.rollback()
        # record error on the webhook row if possible
        try:
            w = db.scalar(
                select(WebhookEvent).where(
                    WebhookEvent.provider == "stripe",
                    WebhookEvent.event_id == event.id
                )
            )
            if w:
                w.processing_error = str(e)
                db.commit()
        except Exception:
            db.rollback()
        error = str(e)

    return WebhookResponse(ok=True, duplicate=False, processed=processed, error=error)