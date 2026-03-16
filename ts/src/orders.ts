import { DatabaseSync } from 'node:sqlite';
import { Order } from './models';

export function markOrderPaid(db: DatabaseSync, orderId: string): void {
  const order = db.prepare(
    'SELECT * FROM orders WHERE id = ?'
  ).get(orderId) as Order | undefined;

  if (!order) {
    throw new Error('order_not_found');
  }

  db.prepare('UPDATE orders SET status = ?, updated_at = ? WHERE id = ?').run(
    'paid',
    new Date().toISOString(),
    orderId
  );
}
