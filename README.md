# Webhook Service

This is a small FastAPI + Postgres service that ingests Stripe-like webhook events and updates orders.

## What you are building

You will implement `POST /webhooks/stripe` to:

1. Persist the incoming webhook event to the database.
2. For `payment_intent.succeeded`, read `order_id` from `data.object.metadata.order_id` and mark that order as `paid`.
3. Return `200 OK` quickly.
4. Add/adjust tests to prove correctness.

You may use any tools (including an LLM), but you must be able to explain your decisions and verify correctness with tests.

---

## Part A — Base feature (first ~25–30 minutes)

Payload shape:

```json
{
  "id": "evt_123",
  "type": "payment_intent.succeeded",
  "data": {
    "object": {
      "metadata": {
        "order_id": "00000000-0000-0000-0000-000000000000"
      }
    }
  }
}
```

## Database schema

```
CREATE TABLE orders (
  id TEXT PRIMARY KEY,
  status TEXT NOT NULL DEFAULT 'pending',
  updated_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE webhook_events (
  id TEXT PRIMARY KEY,
  provider TEXT NOT NULL,
  event_id TEXT NOT NULL,
  type TEXT NOT NULL,
  payload TEXT NOT NULL,
  received_at TEXT NOT NULL DEFAULT (datetime('now')),
  processed_at TEXT NULL,
  processing_error TEXT NULL
);
```
