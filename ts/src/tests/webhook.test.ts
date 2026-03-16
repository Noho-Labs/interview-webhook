/**
 * Tests for the Stripe webhook endpoint.
 *
 * TODO (Candidate):
 * Part A - Implement at least ONE test for the happy path
 * Part B - Add a test for duplicate webhook handling (most important!)
 *
 * You don't need to implement all the test stubs below - focus on what matters.
 */

import request from 'supertest';
import { DatabaseSync } from 'node:sqlite';
import { createApp } from '../main';
import { initDbSchema } from '../db';
import { Order, WebhookEvent } from '../models';

function createTestDb(): DatabaseSync {
  const db = new DatabaseSync(':memory:');
  initDbSchema(db);
  return db;
}

function seedOrder(db: DatabaseSync, id: string, status: string): Order {
  db.prepare('INSERT INTO orders (id, status) VALUES (?, ?)').run(id, status);
  return db.prepare('SELECT * FROM orders WHERE id = ?').get(id) as unknown as Order;
}

// =============================================================================
// Part A Tests — Base webhook functionality
// =============================================================================

test.skip('happy path: marks order paid and stores webhook event', async () => {
  /**
   * PRIORITY: Implement this test first!
   *
   * Verify that:
   * - POST to /webhooks/stripe returns 200 OK
   * - Order status changes to 'paid'
   * - Webhook event is stored in database
   */

  const db = createTestDb();
  const ORDER_ID = '11111111-1111-1111-1111-111111111111';
  seedOrder(db, ORDER_ID, 'pending');

  const app = createApp(db);

  const payload = {
    id: 'evt_test_001',
    type: 'payment_intent.succeeded',
    data: {
      object: {
        metadata: {
          order_id: ORDER_ID,
        },
      },
    },
  };

  // TODO: Implement this test
  // const res = await request(app).post('/webhooks/stripe').send(payload);
  // expect(res.status).toBe(200);
  // assertOrderPaid(db, ORDER_ID);
  // assertWebhookStored(db, payload.id);
});

// Optional: Add more edge case tests if you have time
// test('missing order_id returns 200 with error', async () => { ... });
// test('order not found returns 200 with error', async () => { ... });

// =============================================================================
// Part B Tests — Idempotency
// =============================================================================

test.skip('duplicate webhook: second request is a no-op', async () => {
  /**
   * PART B — CRITICAL TEST: Implement this after Part A works!
   *
   * Verify idempotency by sending the same webhook twice:
   * - Both requests return 200 OK
   * - Only ONE webhook_events row exists for this event_id
   * - Order is in the correct state (not corrupted)
   */

  const db = createTestDb();
  const ORDER_ID = '11111111-1111-1111-1111-111111111111';
  seedOrder(db, ORDER_ID, 'pending');

  const app = createApp(db);

  const payload = {
    id: 'evt_duplicate_001',
    type: 'payment_intent.succeeded',
    data: {
      object: {
        metadata: {
          order_id: ORDER_ID,
        },
      },
    },
  };

  // TODO: Implement this test for Part B
  // const res1 = await request(app).post('/webhooks/stripe').send(payload);
  // const res2 = await request(app).post('/webhooks/stripe').send(payload);
  // expect(res1.status).toBe(200);
  // expect(res2.status).toBe(200);
  // expect(countWebhooksForEvent(db, payload.id)).toBe(1);
  // assertOrderPaid(db, ORDER_ID);
});

// Optional: only if you finish Part B with time to spare
// test('concurrent duplicate webhooks: only one processes', async () => { ... });

// =============================================================================
// Helper assertions
// =============================================================================

function assertOrderPaid(db: DatabaseSync, orderId: string): void {
  const order = db.prepare('SELECT * FROM orders WHERE id = ?').get(orderId) as Order | undefined;
  expect(order).not.toBeNull();
  expect(order!.status).toBe('paid');
}

function assertWebhookStored(db: DatabaseSync, eventId: string): WebhookEvent {
  const event = db
    .prepare('SELECT * FROM webhook_events WHERE event_id = ?')
    .get(eventId) as WebhookEvent | undefined;
  expect(event).not.toBeNull();
  return event!;
}

function countWebhooksForEvent(db: DatabaseSync, eventId: string): number {
  const row = db
    .prepare('SELECT COUNT(*) as count FROM webhook_events WHERE event_id = ?')
    .get(eventId) as { count: number } | undefined;
  return row!.count;
}
