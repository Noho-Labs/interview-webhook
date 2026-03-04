import Database from 'better-sqlite3';
import path from 'path';

const DB_PATH = process.env.DB_PATH || path.join(process.cwd(), 'app.db');

let _db: Database.Database | null = null;

export function getDb(): Database.Database {
  if (!_db) {
    _db = new Database(DB_PATH);
    _db.pragma('journal_mode = WAL');
    _db.pragma('foreign_keys = ON');
    initDbSchema(_db);
  }
  return _db;
}

export function initDbSchema(db: Database.Database): void {
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
