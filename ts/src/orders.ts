import Database from 'better-sqlite3';
import { Order } from './models';

export function markOrderPaid(db: Database.Database, orderId: string): void {
  const order = db.prepare<string, Order>(
    'SELECT * FROM orders WHERE id = ?'
  ).get(orderId);

  if (!order) {
    throw new Error('order_not_found');
  }

  db.prepare('UPDATE orders SET status = ?, updated_at = ? WHERE id = ?').run(
    'paid',
    new Date().toISOString(),
    orderId
  );
}
