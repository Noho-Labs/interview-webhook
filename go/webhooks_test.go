/*
Tests for the Stripe webhook endpoint.

TODO (Candidate):
Part A - Implement at least ONE test for the happy path
Part B - Add a test for duplicate webhook handling (most important!)

You don't need to implement all the test stubs below - focus on what matters.
*/

package main

import (
	"bytes"
	"database/sql"
	"encoding/json"
	"fmt"
	"net/http"
	"net/http/httptest"
	"testing"
)

// newTestDB creates an isolated in-memory SQLite database for a single test.
func newTestDB(t *testing.T) *sql.DB {
	t.Helper()
	// Use the test name as the in-memory DB identifier so parallel tests
	// each get their own database.
	dsn := fmt.Sprintf("file:%s?mode=memory&cache=shared", t.Name())
	db, err := openDB(dsn)
	if err != nil {
		t.Fatalf("newTestDB: %v", err)
	}
	t.Cleanup(func() {
		db.Exec("DROP TABLE IF EXISTS webhook_events") //nolint:errcheck
		db.Exec("DROP TABLE IF EXISTS orders")         //nolint:errcheck
		db.Close()
	})
	return db
}

// seedOrder inserts a test order directly into the database.
func seedOrder(t *testing.T, db *sql.DB, id, status string) {
	t.Helper()
	_, err := db.Exec(
		"INSERT INTO orders (id, status) VALUES (?, ?)",
		id, status,
	)
	if err != nil {
		t.Fatalf("seedOrder: %v", err)
	}
}

// postWebhook fires POST /webhooks/stripe with the given payload and returns the response.
func postWebhook(t *testing.T, handler *WebhookHandler, payload any) *httptest.ResponseRecorder {
	t.Helper()
	body, err := json.Marshal(payload)
	if err != nil {
		t.Fatalf("postWebhook marshal: %v", err)
	}
	req := httptest.NewRequest(http.MethodPost, "/webhooks/stripe", bytes.NewReader(body))
	req.Header.Set("Content-Type", "application/json")
	rr := httptest.NewRecorder()
	handler.HandleStripe(rr, req)
	return rr
}

// =============================================================================
// Part A Tests — Base webhook functionality
// =============================================================================

func TestWebhookHappyPath(t *testing.T) {
	/*
		PRIORITY: Implement this test first!

		Verify that:
		- POST to /webhooks/stripe returns 200 OK
		- Order status changes to "paid"
		- Webhook event is stored in the database
	*/

	t.Skip("TODO: Implement this test")

	const orderID = "11111111-1111-1111-1111-111111111111"

	db := newTestDB(t)
	seedOrder(t, db, orderID, "pending")

	handler := &WebhookHandler{db: db}

	payload := map[string]any{
		"id":   "evt_test_001",
		"type": "payment_intent.succeeded",
		"data": map[string]any{
			"object": map[string]any{
				"metadata": map[string]any{
					"order_id": orderID,
				},
			},
		},
	}

	rr := postWebhook(t, handler, payload)

	// TODO: assert rr.Code == 200
	// TODO: assert order status is "paid"
	// TODO: assert webhook event was stored
	_ = rr
}

// Optional: add more edge-case tests if you have time
//
// func TestWebhookMissingOrderID(t *testing.T) { ... }
// func TestWebhookOrderNotFound(t *testing.T)  { ... }

// =============================================================================
// Part B Tests — Idempotency
// =============================================================================

func TestDuplicateWebhookIdempotency(t *testing.T) {
	/*
		PART B — CRITICAL TEST: Implement this after Part A works!

		Verify idempotency by sending the same webhook twice:
		- Both requests return 200 OK
		- Only ONE webhook_events row exists for this event_id
		- Order is in the correct state (not corrupted)
	*/

	t.Skip("TODO: Implement in Part B")

	const orderID = "11111111-1111-1111-1111-111111111111"

	db := newTestDB(t)
	seedOrder(t, db, orderID, "pending")

	handler := &WebhookHandler{db: db}

	payload := map[string]any{
		"id":   "evt_duplicate_001",
		"type": "payment_intent.succeeded",
		"data": map[string]any{
			"object": map[string]any{
				"metadata": map[string]any{
					"order_id": orderID,
				},
			},
		},
	}

	rr1 := postWebhook(t, handler, payload)
	rr2 := postWebhook(t, handler, payload)

	// TODO: assert both responses are 200
	// TODO: assert only ONE row in webhook_events for "evt_duplicate_001"
	// TODO: assert order status is "paid" (updated exactly once)
	_, _ = rr1, rr2
}

// Optional: only if you finish Part B with time to spare
//
// func TestConcurrentDuplicateWebhooks(t *testing.T) { ... }

// =============================================================================
// Helper assertions
// =============================================================================

// assertOrderPaid checks that an order's status in the DB is "paid".
func assertOrderPaid(t *testing.T, db *sql.DB, orderID string) {
	t.Helper()
	var status string
	err := db.QueryRow("SELECT status FROM orders WHERE id = ?", orderID).Scan(&status)
	if err == sql.ErrNoRows {
		t.Fatalf("assertOrderPaid: order %s not found", orderID)
	}
	if err != nil {
		t.Fatalf("assertOrderPaid: %v", err)
	}
	if status != "paid" {
		t.Errorf("assertOrderPaid: expected status 'paid', got %q", status)
	}
}

// assertWebhookStored checks that a webhook_events row exists for the event_id.
func assertWebhookStored(t *testing.T, db *sql.DB, eventID string) {
	t.Helper()
	var count int
	err := db.QueryRow(
		"SELECT COUNT(*) FROM webhook_events WHERE event_id = ?", eventID,
	).Scan(&count)
	if err != nil {
		t.Fatalf("assertWebhookStored: %v", err)
	}
	if count == 0 {
		t.Errorf("assertWebhookStored: no webhook_events row found for event_id %q", eventID)
	}
}

// countWebhooksForEvent returns how many webhook_events rows exist for event_id.
func countWebhooksForEvent(t *testing.T, db *sql.DB, eventID string) int {
	t.Helper()
	var count int
	err := db.QueryRow(
		"SELECT COUNT(*) FROM webhook_events WHERE event_id = ?", eventID,
	).Scan(&count)
	if err != nil {
		t.Fatalf("countWebhooksForEvent: %v", err)
	}
	return count
}
