import { Router, Request, Response } from 'express';
import Database from 'better-sqlite3';
import { StripeEvent, WebhookResponse } from './schemas';

export function createWebhookRouter(db: Database.Database): Router {
  const router = Router();

  router.post('/stripe', (req: Request, res: Response) => {
    /**
     * Implement webhook handling for Stripe payment events.
     *
     * Part A: Basic webhook processing
     * Part B: Handle duplicate events (introduced at ~20 min)
     *
     * See README.md for detailed requirements.
     *
     * Useful imports already available:
     *   - db          — better-sqlite3 Database instance
     *   - markOrderPaid(db, orderId) — from ./orders
     *   - v4 as uuidv4 — from 'uuid'
     */
    const event: StripeEvent = req.body;

    // TODO: Implement webhook handling here

    res.json({ ok: true } satisfies WebhookResponse);
  });

  return router;
}
