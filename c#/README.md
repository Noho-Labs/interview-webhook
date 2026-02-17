# Webhook Service Interview Exercise — C#

This is an ASP.NET Core + SQLite service that ingests Stripe-like webhook events and updates orders. It is a direct port of the Python (FastAPI) version in `../python/`.

**Time: ~45 minutes coding**

You may use any tools (including LLMs), but you must be able to explain your decisions and verify correctness with tests.

**Timeline:**
- 0-5 min: Setup, read instructions
- 5-20 min: Part A (base feature)
- 20-40 min: Part B (idempotency - this is the main challenge)
- 40-45 min: Verify with tests
- 45-60 min: Discussion

---

## Setup

### Prerequisites
- .NET 8 SDK (`dotnet --version`)
- SQLite (comes with the package — no separate install needed)

### Run the application

```bash
cd WebhookService
dotnet run
```

The service will be available at `http://localhost:9000`

### Seed test data

```bash
cd WebhookService
dotnet run -- seed
```

This creates sample orders with known IDs you can reference in webhook payloads.

### Run tests

```bash
cd WebhookService.Tests
dotnet test -v
```

---

## File Structure

```
c#/
├── WebhookService/
│   ├── Program.cs                  # App entry point (mirrors python/app/main.py)
│   ├── Controllers/
│   │   └── WebhooksController.cs   # TODO: Your implementation here (mirrors webhooks.py)
│   ├── Data/
│   │   ├── AppDbContext.cs         # EF Core context (mirrors db.py)
│   │   └── SeedData.cs             # Seed helper (mirrors seed.py)
│   ├── Models/
│   │   ├── Order.cs                # (mirrors models.py Order)
│   │   └── WebhookEvent.cs         # (mirrors models.py WebhookEvent)
│   ├── Schemas/
│   │   ├── StripeEvent.cs          # (mirrors schemas.py StripeEvent)
│   │   └── WebhookResponse.cs      # (mirrors schemas.py WebhookResponse)
│   └── Services/
│       └── OrdersService.cs        # (mirrors orders.py mark_order_paid)
└── WebhookService.Tests/
    └── WebhookTests.cs             # TODO: Add/update tests (mirrors test_webhook.py)
```

---

## Part A — Base Feature (first 15-20 minutes)

**Goal: Get the happy path working quickly. Don't overthink error handling yet.**

### Objective

Implement `POST /webhooks/stripe` in `WebhooksController.cs` that:

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

### Accessing `data.object.metadata.order_id`

The `StripeEvent.Data` property is a `JsonElement`. Navigate nested fields like this:

```csharp
var orderIdStr = stripeEvent.Data
    .GetProperty("object")
    .GetProperty("metadata")
    .GetProperty("order_id")
    .GetString();
```

### Expected Behavior (Minimum for Part A)
- ✅ Event persisted to `webhook_events` table
- ✅ Order status updated to `paid`
- ✅ Returns `200 OK`
- ✅ Basic error handling (don't crash if something goes wrong)

---

## Part B — Change Request (introduced at ~20 min)

**This is the main challenge. Budget 20 minutes for this.**

### New Requirement: Idempotency

**Problem:** Stripe sometimes delivers the same event multiple times due to retries or network issues. We must ensure we **never apply side effects twice**.

### Objective

Update your implementation to:

1. **Detect duplicate events** using the event's unique `id` field
2. **Return `200 OK` for duplicates** without reprocessing
3. **Handle concurrent requests** safely (two webhook requests for the same event arriving simultaneously)

### Hints

- The database schema includes a unique index on `(provider, event_id)` (defined in `AppDbContext.cs`)
- Inserting a duplicate will throw a `DbUpdateException` with a unique constraint violation
- Catch that exception and return `WebhookResponse { Ok = true, Duplicate = true }` instead

### Expected Behavior
- ✅ First request: processes normally, marks order paid
- ✅ Second request with same `event.id`: returns `200 OK` but doesn't reprocess
- ✅ Concurrent requests: only one processes, others detect duplicate
- ✅ Side effects (order status update) happen exactly once

---

## Part C — Async Processing (Bonus)

**Skip this unless you have extra time.**

Consider using `IHostedService` or `BackgroundService` / `Task.Run` with a channel to return 200 immediately while processing in the background, while still maintaining idempotency.

---

## Database Schema (EF Core → SQLite)

Tables are created automatically via `db.Database.EnsureCreated()` on startup.

```sql
CREATE TABLE orders (
  id TEXT PRIMARY KEY,
  status TEXT NOT NULL,
  updated_at TEXT NOT NULL
);

CREATE TABLE webhook_events (
  id TEXT PRIMARY KEY,
  provider TEXT NOT NULL,
  event_id TEXT NOT NULL,
  type TEXT NOT NULL,
  payload TEXT NOT NULL,
  received_at TEXT NOT NULL,
  processed_at TEXT NULL,
  processing_error TEXT NULL,
  CONSTRAINT uq_provider_event_id UNIQUE (provider, event_id)
);
```

---

## Evaluation Criteria (for interviewers)

### Strong Senior Behaviors
- ✅ Clarifies requirements and edge cases upfront
- ✅ Works test-first or adds tests early
- ✅ Leverages DB constraints for correctness (unique constraint)
- ✅ Handles duplicate inserts gracefully (catches `DbUpdateException`)
- ✅ Thinks about transaction boundaries
- ✅ Clean separation: controller thin, business logic testable
- ✅ Pragmatic error handling (records errors, returns appropriate status codes)
- ✅ Can explain failure modes and tradeoffs

### Red Flags
- ❌ No tests for duplicates or edge cases
- ❌ In-memory deduplication only (breaks across instances)
- ❌ Updates order before ensuring event uniqueness (race condition)
- ❌ Over-engineered solution for this exercise
- ❌ Can't explain what happens when order doesn't exist or fields are missing
