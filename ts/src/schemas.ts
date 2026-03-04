export interface StripeEvent {
  id: string;
  type: string;
  data: Record<string, unknown>;
}

export interface WebhookResponse {
  ok: boolean;
  duplicate?: boolean;
  processed?: boolean;
  error?: string;
}
