#!/usr/bin/env python3
"""
Seed script to populate the database with test orders.

Run this before testing to ensure you have orders to reference in webhooks.
"""

import uuid
from datetime import datetime

from app.db import SessionLocal, init_db
from app.models import Order


def seed_orders():
    """Create test orders in the database."""
    # Initialize database tables
    init_db()

    # Create database session
    db = SessionLocal()

    try:
        # Define test orders
        test_orders = [
            {
                "id": uuid.UUID("11111111-1111-1111-1111-111111111111"),
                "status": "pending",
            },
            {
                "id": uuid.UUID("22222222-2222-2222-2222-222222222222"),
                "status": "paid",
            },
            {
                "id": uuid.UUID("33333333-3333-3333-3333-333333333333"),
                "status": "pending",
            },
        ]

        # Check if orders already exist
        existing_count = db.query(Order).count()
        if existing_count > 0:
            print(f"⚠️  Database already has {existing_count} order(s).")
            response = input("Do you want to clear and reseed? (y/N): ")
            if response.lower() != 'y':
                print("❌ Seed cancelled.")
                return

            # Clear existing orders
            db.query(Order).delete()
            db.commit()
            print("✅ Cleared existing orders.")

        # Insert test orders
        for order_data in test_orders:
            order = Order(**order_data)
            db.add(order)

        db.commit()

        # Display created orders
        print("\n✅ Successfully seeded orders:")
        print("\n┌─────────────────────────────────────────┬──────────┐")
        print("│ Order ID                                │ Status   │")
        print("├─────────────────────────────────────────┼──────────┤")
        for order_data in test_orders:
            print(f"│ {order_data['id']} │ {order_data['status']:<8} │")
        print("└─────────────────────────────────────────┴──────────┘")

        print("\n📝 Example webhook payload:")
        print("""
curl -X POST http://localhost:9000/webhooks/stripe \\
  -H "Content-Type: application/json" \\
  -d '{
    "id": "evt_test_001",
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "metadata": {
          "order_id": "11111111-1111-1111-1111-111111111111"
        }
      }
    }
  }'
        """)

    except Exception as e:
        print(f"❌ Error seeding database: {e}")
        db.rollback()
        raise
    finally:
        db.close()


if __name__ == "__main__":
    print("🌱 Seeding database with test orders...\n")
    seed_orders()
