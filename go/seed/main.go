// seed populates the database with test orders.
//
// Run from the go/ directory:
//
//	go run ./seed
package main

import (
	"database/sql"
	"fmt"
	"log"
	"os"

	_ "modernc.org/sqlite"
)

func main() {
	path := os.Getenv("DATABASE_PATH")
	if path == "" {
		path = "./app.db"
	}

	db, err := openDB(path)
	if err != nil {
		log.Fatalf("failed to open database: %v", err)
	}
	defer db.Close()

	fmt.Println("Seeding database with test orders...\n")

	orders := []struct {
		id     string
		status string
	}{
		{"11111111-1111-1111-1111-111111111111", "pending"},
		{"22222222-2222-2222-2222-222222222222", "paid"},
		{"33333333-3333-3333-3333-333333333333", "pending"},
	}

	var existing int
	db.QueryRow("SELECT COUNT(*) FROM orders").Scan(&existing) //nolint:errcheck
	if existing > 0 {
		fmt.Printf("WARNING: database already has %d order(s).\n", existing)
		fmt.Print("Clear and reseed? (y/N): ")
		var answer string
		fmt.Scanln(&answer)
		if answer != "y" && answer != "Y" {
			fmt.Println("Seed cancelled.")
			return
		}
		if _, err := db.Exec("DELETE FROM orders"); err != nil {
			log.Fatalf("failed to clear orders: %v", err)
		}
		fmt.Println("Cleared existing orders.")
	}

	for _, o := range orders {
		if _, err := db.Exec(
			"INSERT INTO orders (id, status) VALUES (?, ?)", o.id, o.status,
		); err != nil {
			log.Fatalf("insert order %s: %v", o.id, err)
		}
	}

	fmt.Println("\nSuccessfully seeded orders:")
	fmt.Println()
	fmt.Printf("%-40s  %s\n", "Order ID", "Status")
	fmt.Println("----------------------------------------  --------")
	for _, o := range orders {
		fmt.Printf("%-40s  %s\n", o.id, o.status)
	}

	fmt.Println(`
Example webhook payload:

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
    }'`)
}

func openDB(path string) (*sql.DB, error) {
	db, err := sql.Open("sqlite", path)
	if err != nil {
		return nil, err
	}
	db.SetMaxOpenConns(1)
	if err := createSchema(db); err != nil {
		db.Close()
		return nil, err
	}
	return db, nil
}

func createSchema(db *sql.DB) error {
	_, err := db.Exec(`
		CREATE TABLE IF NOT EXISTS orders (
			id         TEXT PRIMARY KEY,
			status     TEXT NOT NULL DEFAULT 'pending',
			updated_at TEXT NOT NULL DEFAULT (datetime('now'))
		);

		CREATE TABLE IF NOT EXISTS webhook_events (
			id               TEXT PRIMARY KEY,
			provider         TEXT NOT NULL,
			event_id         TEXT NOT NULL,
			type             TEXT NOT NULL,
			payload          TEXT NOT NULL,
			received_at      TEXT NOT NULL DEFAULT (datetime('now')),
			processed_at     TEXT NULL,
			processing_error TEXT NULL,
			CONSTRAINT uq_provider_event_id UNIQUE (provider, event_id)
		);
	`)
	return err
}
