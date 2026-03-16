#!/usr/bin/env tsx
/**
 * Seed script to populate the database with test orders.
 *
 * Run this before testing to ensure you have orders to reference in webhooks:
 *   npm run seed
 */

import { getDb } from './src/db';
import { Order } from './src/models';
import * as readline from 'readline';

const TEST_ORDERS: Array<{ id: string; status: string }> = [
  { id: '11111111-1111-1111-1111-111111111111', status: 'pending' },
  { id: '22222222-2222-2222-2222-222222222222', status: 'paid' },
  { id: '33333333-3333-3333-3333-333333333333', status: 'pending' },
];

async function confirm(question: string): Promise<boolean> {
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      rl.close();
      resolve(answer.toLowerCase() === 'y');
    });
  });
}

async function seedOrders(): Promise<void> {
  const db = getDb();

  const existingCount = (
    db.prepare<[], { count: number }>('SELECT COUNT(*) as count FROM orders').get()!
  ).count;

  if (existingCount > 0) {
    console.log(`Warning: Database already has ${existingCount} order(s).`);
    const yes = await confirm('Do you want to clear and reseed? (y/N): ');
    if (!yes) {
      console.log('Seed cancelled.');
      return;
    }
    db.prepare('DELETE FROM orders').run();
    console.log('Cleared existing orders.');
  }

  const insert = db.prepare('INSERT INTO orders (id, status) VALUES (?, ?)');
  db.exec('BEGIN');
  try {
    for (const order of TEST_ORDERS) {
      insert.run(order.id, order.status);
    }
    db.exec('COMMIT');
  } catch (e) {
    db.exec('ROLLBACK');
    throw e;
  }

  console.log('\nSuccessfully seeded orders:');
  console.log('\n' + '─'.repeat(55));
  console.log('│ Order ID                             │ Status   │');
  console.log('─'.repeat(55));
  for (const order of TEST_ORDERS) {
    console.log(`│ ${order.id} │ ${order.status.padEnd(8)} │`);
  }
  console.log('─'.repeat(55));

  console.log(`
Example webhook payload:

curl -X POST http://localhost:9000/webhooks/stripe \\
  -H "Content-Type: application/json" \\
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
  }'
`);
}

console.log('Seeding database with test orders...\n');
seedOrders().catch((err) => {
  console.error('Error seeding database:', err);
  process.exit(1);
});
