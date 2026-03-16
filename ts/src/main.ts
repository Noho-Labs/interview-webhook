import express, { Express } from 'express';
import { DatabaseSync } from 'node:sqlite';
import { getDb } from './db';
import { createWebhookRouter } from './webhooks';

export function createApp(db?: DatabaseSync): Express {
  const app = express();
  app.use(express.json());

  const database = db ?? getDb();
  app.use('/webhooks', createWebhookRouter(database));

  return app;
}

if (require.main === module) {
  const app = createApp();
  app.listen(9000, '0.0.0.0', () => {
    console.log('Server running on http://localhost:9000');
  });
}
