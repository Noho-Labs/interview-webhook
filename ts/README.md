# Webhook Service Interview Exercise

This is an Express + SQLite service that ingests Stripe-like webhook events and updates orders. You will implement the webhook endpoint and ensure it handles edge cases correctly.

**Time: ~45 minutes coding**

**Timeline:**

- 0-5 min: Setup, read instructions
- 5-20 min: Part A (base feature)
- 20-40 min: Part B (idempotency - this is the main challenge)
- 40-45 min: Verify with tests
- 45-60 min: Discussion

---

## Setup

### Prerequisites

- Node.js 18+
- npm

### Install dependencies

```bash
npm install
```

### Run the application

```bash
npm run dev
```

The service will be available at `http://localhost:9000`

### Seed test data

```bash
npm run seed
```

This creates sample orders with known IDs that you can reference in webhook payloads.

### Run tests

```bash
npm test
```

---

## Part A — Base Feature (first 15-20 minutes)

**Goal: Get the happy path working quickly. Don't overthink error handling yet.**

### Objective

Implement `POST /webhooks/stripe` that:

1. **Persists the incoming webhook event** to the `webhook_events` table
2. **For `payment_intent.succeeded` events**, reads `order_id` from `data.object.metadata.order_id` and marks that order as `paid`
3. **Returns `200 OK` quickly**
4. **Handles errors gracefully** (missing order_id, order not found, etc.)

### Example Payload

```json
{
  "id": "evt_1234567890",
  "type": "payment_intent.succeeded",
  "data": {
    "object": {
      "metadata": {
        "order_id": "11111111-1111-1111-1111-111111111111"
      }
    }
  }
}
```

### Testing Your Implementation

```bash
# Test with a valid order (use order IDs from seed data)
curl -X POST http://localhost:9000/webhooks/stripe \
  -H "Content-Type: application/json" \
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

# Verify the order was marked as paid
sqlite3 app.db "SELECT * FROM orders WHERE id = '11111111-1111-1111-1111-111111111111';"

# Check the webhook was recorded
sqlite3 app.db "SELECT * FROM webhook_events WHERE event_id = 'evt_test_001';"
```

### Expected Behavior (Minimum for Part A)

- ✅ Event persisted to `webhook_events` table
- ✅ Order status updated to `paid`
- ✅ Returns `200 OK`
- ✅ Basic error handling (don't crash if something goes wrong)

**Nice to have but not required for Part A:**

- Setting `processed_at` timestamp
- Detailed error recording in `processing_error` field
- Handling all edge cases (focus on Part B instead)

### Requirements

- Implement the happy path first
- Add at least one test that verifies the order gets marked as paid
- Basic error handling is fine (you'll refine in Part B)

---

## Part B — Change Request (introduced at ~20 min)

**This is the main challenge. Budget 20 minutes for this.**

### New Requirement: Idempotency

**Problem:** Stripe sometimes delivers the same event multiple times due to retries or network issues. We must ensure we **never apply side effects twice** (e.g., marking an order as paid twice, or processing a payment twice).

### Objective

Update your implementation to:

1. **Detect duplicate events** using the event's unique `id` field
2. **Return `200 OK` for duplicates** without reprocessing
3. **Handle concurrent requests** safely (two webhook requests for the same event arriving simultaneously)

### Testing Duplicate Handling

```bash
# Send the same event twice
curl -X POST http://localhost:9000/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{
    "id": "evt_duplicate_test",
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "metadata": {
          "order_id": "11111111-1111-1111-1111-111111111111"
        }
      }
    }
  }'

# Send it again - should be detected as duplicate
curl -X POST http://localhost:9000/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{
    "id": "evt_duplicate_test",
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "metadata": {
          "order_id": "11111111-1111-1111-1111-111111111111"
        }
      }
    }
  }'

# Verify only ONE webhook_event row exists for this event_id
sqlite3 app.db "SELECT COUNT(*) FROM webhook_events WHERE event_id = 'evt_duplicate_test';"
# Expected output: 1

# Verify the order was only updated once
sqlite3 app.db "SELECT status FROM orders WHERE id = '11111111-1111-1111-1111-111111111111';"
# Expected output: paid (not double-processed)
```

### Expected Behavior

- ✅ First request: processes normally, marks order paid
- ✅ Second request with same `event.id`: returns `200 OK` but doesn't reprocess
- ✅ Concurrent requests: only one processes, others detect duplicate
- ✅ Side effects (order status update) happen exactly once

### Hints

- The database schema includes a unique constraint: `UNIQUE (provider, event_id)`
- Consider what happens when you try to insert a duplicate row
- Think about transaction boundaries and database-level guarantees
- better-sqlite3 runs synchronously — use `db.transaction(fn)` for atomic operations

---

## Part C — Async Processing (Bonus - Only if you finish early)

**Skip this unless you have extra time. Part B is the priority.**

If you have time remaining, consider how you'd make webhook processing asynchronous:

- Return `200 OK` immediately after storing the event
- Process the order update asynchronously (e.g., `setImmediate`)
- Still maintain idempotency guarantees

---

## Database Schema

```sql
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
  processing_error TEXT NULL,
  CONSTRAINT uq_provider_event_id UNIQUE (provider, event_id)
);
```

**Note:** The unique constraint on `(provider, event_id)` is already in place. You can leverage this for Part B.

### Seed Data

The database includes these test orders (created via `npm run seed`):

```
11111111-1111-1111-1111-111111111111  pending
22222222-2222-2222-2222-222222222222  paid
33333333-3333-3333-3333-333333333333  pending
```

---

## Viewing Database Contents

```bash
# Open SQLite shell
sqlite3 app.db

# Inside SQLite, format output nicely
.mode table
.headers on

# View orders
SELECT * FROM orders;

# View webhook events
SELECT * FROM webhook_events;

# Check specific order status
SELECT status FROM orders WHERE id = '11111111-1111-1111-1111-111111111111';

# Exit SQLite
.quit
```

---

## File Structure

```
src/
├── main.ts              # Express app entry point
├── webhooks.ts          # TODO: Your implementation here
├── orders.ts            # Helper: markOrderPaid(db, orderId)
├── models.ts            # TypeScript interfaces (Order, WebhookEvent)
├── schemas.ts           # Request/response types (StripeEvent, WebhookResponse)
├── db.ts                # Database setup (better-sqlite3)
└── tests/
    └── webhook.test.ts  # TODO: Add/update tests
seed.ts                  # Seed script for test data
```

---

## Evaluation Criteria (for interviewers)

### Strong Senior Behaviors

- ✅ Clarifies requirements and edge cases upfront
- ✅ Works test-first or adds tests early
- ✅ Leverages DB constraints for correctness (unique constraint)
- ✅ Handles duplicate inserts gracefully (e.g., catches SQLite `UNIQUE constraint failed`)
- ✅ Thinks about transaction boundaries
- ✅ Clean separation: controller thin, business logic testable
- ✅ Pragmatic error handling (records errors, returns appropriate status codes)
- ✅ Can explain failure modes and tradeoffs

### Red Flags

- ❌ No tests for duplicates or edge cases
- ❌ In-memory deduplication only (breaks across instances)
- ❌ Updates order before ensuring event uniqueness (race condition)
- ❌ Over-engineered solution (message queues, complex abstractions for this exercise)
- ❌ Can't explain what happens when order doesn't exist or fields are missing

---

## Tips

- Start simple: get the happy path working first
- Write a test before implementing each feature
- Use the database to enforce correctness (constraints, transactions)
- Think about what happens in production: retries, concurrent requests, partial failures
- Check `src/orders.ts` for the `markOrderPaid()` helper function
- Look at `src/models.ts` to see the TypeScript interfaces
- The `WebhookResponse` type has fields you can use: `ok`, `duplicate`, `processed`, `error`
- better-sqlite3 is **synchronous** — no async/await needed for DB calls
- Use `db.transaction(fn)` to wrap multiple statements atomically

---

## Questions?

If anything is unclear, please ask! In a real interview, asking clarifying questions is a positive signal.

Good luck!
