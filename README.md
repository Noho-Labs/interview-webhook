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

## Seed the database
