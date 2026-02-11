from fastapi import FastAPI
from app.webhooks import router as webhook_router
from app.db import init_db

app = FastAPI(title="Webhook Service")

@app.on_event("startup")
def _startup():
    init_db()

app.include_router(webhook_router, prefix="/webhooks")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=9000, reload=True)