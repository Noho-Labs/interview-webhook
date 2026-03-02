package main

import (
	"database/sql"
	"errors"
	"time"
)

// ErrOrderNotFound is returned by MarkOrderPaid when no order with the given ID exists.
var ErrOrderNotFound = errors.New("order_not_found")

// executor is satisfied by both *sql.DB and *sql.Tx, letting callers use
// MarkOrderPaid inside or outside a transaction.
type executor interface {
	Exec(query string, args ...any) (sql.Result, error)
}

// MarkOrderPaid sets an order's status to "paid".
// Returns ErrOrderNotFound if no row with that ID exists.
func MarkOrderPaid(db executor, orderID string) error {
	result, err := db.Exec(
		"UPDATE orders SET status = 'paid', updated_at = ? WHERE id = ?",
		time.Now().UTC().Format(time.RFC3339),
		orderID,
	)
	if err != nil {
		return err
	}

	rows, err := result.RowsAffected()
	if err != nil {
		return err
	}
	if rows == 0 {
		return ErrOrderNotFound
	}
	return nil
}
