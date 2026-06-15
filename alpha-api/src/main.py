import asyncio
import os
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from .database import engine, Base
from .seed import seed_db
from .routers import users, coffee

APP_VERSION = os.getenv("APP_VERSION", "v0.0.1")


@asynccontextmanager
async def lifespan(app: FastAPI):
    for attempt in range(1, 11):
        try:
            async with engine.begin() as conn:
                await conn.run_sync(Base.metadata.create_all)
            break
        except Exception:
            if attempt < 10:
                print(f"Database not ready, retrying in 3s... (attempt {attempt}/10)")
                await asyncio.sleep(3)
            else:
                raise
    await seed_db()
    yield


app = FastAPI(title="alpha-api", version=APP_VERSION, lifespan=lifespan)

_origins = [o for o in os.getenv("CORS_ALLOWED_ORIGINS", "").split(",") if o]
if _origins:
    app.add_middleware(
        CORSMiddleware,
        allow_origins=_origins,
        allow_methods=["*"],
        allow_headers=["*"],
    )

app.include_router(users.router)
app.include_router(coffee.router)


@app.get("/")
async def root():
    return {"message": "Welcome"}


@app.get("/metadata")
async def metadata():
    return {"version": APP_VERSION}
