export interface Order {
  id: string;
  status: string;
  updated_at: string;
}

export interface WebhookEvent {
  id: string;
  provider: string;
  event_id: string;
  type: string;
  payload: string; // JSON-encoded string
  received_at: string;
  processed_at: string | null;
  processing_error: string | null;
}
