"""
Tests for the Stripe webhook endpoint.

TODO (Candidate):
Part A - Implement at least ONE test for the happy path
Part B - Add a test for duplicate webhook handling (most important!)

You don't need to implement all the test stubs below - focus on what matters.
"""

import uuid
import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker

from app.main import app
from app.db import Base, get_db
from app.models import Order, WebhookEvent


# Test database setup
TEST_DATABASE_URL = "sqlite:///./test.db"
engine = create_engine(TEST_DATABASE_URL, connect_args={"check_same_thread": False})
TestSessionLocal = sessionmaker(bind=engine, autocommit=False, autoflush=False)


def override_get_db():
    """Override the get_db dependency for tests."""
    db = TestSessionLocal()
    try:
        yield db
    finally:
        db.close()


app.dependency_overrides[get_db] = override_get_db


@pytest.fixture(autouse=True)
def setup_database():
    """Create fresh database tables for each test."""
    Base.metadata.create_all(bind=engine)
    yield
    Base.metadata.drop_all(bind=engine)


@pytest.fixture
def client():
    """FastAPI test client."""
    return TestClient(app)


@pytest.fixture
def db():
    """Database session for test setup."""
    session = TestSessionLocal()
    try:
        yield session
    finally:
        session.close()


@pytest.fixture
def test_order(db):
    """Create a test order in pending status."""
    order_id = uuid.UUID("11111111-1111-1111-1111-111111111111")
    order = Order(id=order_id, status="pending")
    db.add(order)
    db.commit()
    db.refresh(order)
    return order


# =============================================================================
# Part A Tests - Base webhook functionality
# =============================================================================

def test_webhook_happy_path(client, test_order, db):
    """
    PRIORITY: Implement this test first!

    Verify that:
    - POST to /webhooks/stripe returns 200 OK
    - Order status changes to 'paid'
    - Webhook event is stored in database
    """
    payload = {
        "id": "evt_test_001",
        "type": "payment_intent.succeeded",
        "data": {
            "object": {
                "metadata": {
                    "order_id": str(test_order.id)
                }
            }
        }
    }

    # TODO: Implement this test
    pytest.skip("TODO: Implement this test")


# Optional: Add more edge case tests if you have time
# def test_webhook_missing_order_id(client, test_order):
#     """Test handling of webhook with missing order_id"""
#     pass
#
# def test_webhook_order_not_found(client):
#     """Test handling of webhook referencing non-existent order"""
#     pass


# =============================================================================
# Part B Tests - Idempotency
# =============================================================================

def test_duplicate_webhook_idempotency(client, test_order, db):
    """
    PART B - CRITICAL TEST: Implement this after Part A works!

    Verify idempotency by sending the same webhook twice:
    - Both requests return 200 OK
    - Only ONE webhook_event record exists
    - Order is in correct state (not corrupted)
    """
    payload = {
        "id": "evt_duplicate_001",
        "type": "payment_intent.succeeded",
        "data": {
            "object": {
                "metadata": {
                    "order_id": str(test_order.id)
                }
            }
        }
    }

    # TODO: Implement this test for Part B
    pytest.skip("TODO: Implement in Part B")


# Optional: Only if you finish Part B with time to spare
# def test_concurrent_duplicate_webhooks(client, test_order):
#     """Advanced: Test concurrent requests for same event"""
#     pass


# =============================================================================
# Helper assertions (examples)
# =============================================================================

def assert_order_paid(db, order_id: uuid.UUID):
    """Helper to assert an order is marked as paid."""
    order = db.query(Order).filter(Order.id == order_id).first()
    assert order is not None, f"Order {order_id} not found"
    assert order.status == "paid", f"Expected status 'paid', got '{order.status}'"


def assert_webhook_stored(db, event_id: str):
    """Helper to assert a webhook event was stored."""
    webhook = db.query(WebhookEvent).filter(WebhookEvent.event_id == event_id).first()
    assert webhook is not None, f"Webhook event {event_id} not found"
    return webhook


def count_webhooks_for_event(db, event_id: str) -> int:
    """Count how many webhook records exist for an event_id."""
    return db.query(WebhookEvent).filter(WebhookEvent.event_id == event_id).count()
