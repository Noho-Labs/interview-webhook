import uuid
from datetime import datetime
from sqlalchemy.orm import Session
from sqlalchemy import select
from app.models import Order

def mark_order_paid(db: Session, order_id: uuid.UUID) -> None:
    order = db.scalar(select(Order).where(Order.id == order_id))
    if order is None:
        raise ValueError("order_not_found")

    order.status = "paid"
    order.updated_at = datetime.utcnow()