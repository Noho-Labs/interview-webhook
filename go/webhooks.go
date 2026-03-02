package main

import (
	"database/sql"
	"encoding/json"
	"net/http"
)

// WebhookHandler holds shared dependencies for webhook handlers.
type WebhookHandler struct {
	db *sql.DB
}

// HandleStripe handles POST /webhooks/stripe.
//
// Implement webhook handling for Stripe payment events.
//
// Part A: Basic webhook processing
// Part B: Handle duplicate events (introduced at ~20 min)
//
// See README.md for detailed requirements.
//
// Useful stdlib packages:
//   - encoding/json  — json.NewDecoder(r.Body).Decode(&event)
//   - fmt            — fmt.Sprintf("%x", ...)  or crypto/rand for UUID generation
//   - database/sql   — h.db.Begin() returns a *sql.Tx (accepted by MarkOrderPaid)
func (h *WebhookHandler) HandleStripe(w http.ResponseWriter, r *http.Request) {
	// TODO: Implement webhook handling here

	writeJSON(w, http.StatusOK, WebhookResponse{OK: true})
}

// writeJSON serialises v to JSON and writes it with the given status code.
func writeJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(v)
}
