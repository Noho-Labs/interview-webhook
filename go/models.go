package main

// StripeEvent is the incoming webhook payload from Stripe.
type StripeEvent struct {
	ID   string                 `json:"id"`
	Type string                 `json:"type"`
	Data map[string]interface{} `json:"data"`
}

// WebhookResponse is the JSON body returned by the webhook endpoint.
type WebhookResponse struct {
	OK        bool   `json:"ok"`
	Duplicate bool   `json:"duplicate,omitempty"`
	Processed bool   `json:"processed,omitempty"`
	Error     string `json:"error,omitempty"`
}
