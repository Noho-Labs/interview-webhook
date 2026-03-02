package main

import (
	"database/sql"
	"os"

	_ "modernc.org/sqlite"
)

func initDB() (*sql.DB, error) {
	path := os.Getenv("DATABASE_PATH")
	if path == "" {
		path = "./app.db"
	}
	return openDB(path)
}

func openDB(path string) (*sql.DB, error) {
	db, err := sql.Open("sqlite", path)
	if err != nil {
		return nil, err
	}

	// SQLite does not support concurrent writers; serialize all access.
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
