from contextlib import asynccontextmanager
from fastapi import FastAPI
from app.webhooks import router as webhook_router
from app.db import init_db


@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: Initialize database
    init_db()
    yield
    # Shutdown: cleanup (none needed for this app)


app = FastAPI(title="Webhook Service", lifespan=lifespan)

app.include_router(webhook_router, prefix="/webhooks")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=9000, reload=True)