package main

import (
	"log"
	"net/http"
)

func main() {
	db, err := initDB()
	if err != nil {
		log.Fatalf("failed to initialize database: %v", err)
	}
	defer db.Close()

	handler := &WebhookHandler{db: db}

	mux := http.NewServeMux()
	mux.HandleFunc("POST /webhooks/stripe", handler.HandleStripe)

	log.Println("Server listening on :9000")
	if err := http.ListenAndServe(":9000", mux); err != nil {
		log.Fatalf("server error: %v", err)
	}
}
