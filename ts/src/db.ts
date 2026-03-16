import { DatabaseSync } from 'node:sqlite';
import path from 'path';

const DB_PATH = process.env.DB_PATH || path.join(process.cwd(), 'app.db');

let _db: DatabaseSync | null = null;

export function getDb(): DatabaseSync {
  if (!_db) {
    _db = new DatabaseSync(DB_PATH);
    _db.exec('PRAGMA journal_mode = WAL');
    _db.exec('PRAGMA foreign_keys = ON');
    initDbSchema(_db);
  }
  return _db;
}

export function initDbSchema(db: DatabaseSync): void {
  db.exec(`
    CREATE TABLE IF NOT EXISTS orders (
      id TEXT PRIMARY KEY,
      status TEXT NOT NULL DEFAULT 'pending',
      updated_at TEXT NOT NULL DEFAULT (datetime('now'))
    )
  `);

  db.exec(`
    CREATE TABLE IF NOT EXISTS webhook_events (
      id TEXT PRIMARY KEY,
      provider TEXT NOT NULL,
      event_id TEXT NOT NULL,
      type TEXT NOT NULL,
      payload TEXT NOT NULL,
      received_at TEXT NOT NULL DEFAULT (datetime('now')),
      processed_at TEXT NULL,
      processing_error TEXT NULL,
      CONSTRAINT uq_provider_event_id UNIQUE (provider, event_id)
    )
  `);
}
