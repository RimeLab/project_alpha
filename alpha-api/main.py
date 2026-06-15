import asyncio
import os
from contextlib import asynccontextmanager

from fastapi import FastAPI, Depends, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from sqlalchemy import select, exists
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import selectinload

from database import engine, get_db, Base
from models import User, Coffee
from schemas import UserCreate, UserUpdate, CoffeeCreate, CoffeeUpdate
from security import hash_password
from seed import seed_db

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


def _user(u: User) -> dict:
    return {"id": u.id, "username": u.username, "description": u.description}


def _coffee(c: Coffee) -> dict:
    return {
        "id": c.id, "type": c.type, "shop": c.shop, "location": c.location,
        "intensity": c.intensity, "rating": c.rating, "temperature": c.temperature,
        "notes": c.notes, "userId": c.user_id,
    }


# --- Root ---

@app.get("/")
async def root():
    return {"message": "Welcome"}


@app.get("/metadata")
async def metadata():
    return {"version": APP_VERSION}


# --- Users ---

@app.post("/users/", status_code=201)
async def create_user(req: UserCreate, db: AsyncSession = Depends(get_db)):
    user = User(username=req.username, password=hash_password(req.password), description=req.description)
    db.add(user)
    await db.commit()
    await db.refresh(user)
    return JSONResponse(status_code=201, content=_user(user), headers={"Location": f"/users/{user.id}"})


@app.get("/users/")
async def list_users(db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(User))
    return [_user(u) for u in result.scalars()]


@app.get("/users/{user_id}")
async def get_user(user_id: int, db: AsyncSession = Depends(get_db)):
    user = await db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404)
    return _user(user)


@app.put("/users/{user_id}")
async def update_user(user_id: int, req: UserUpdate, db: AsyncSession = Depends(get_db)):
    user = await db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404)
    user.username = req.username
    if req.password is not None:
        user.password = hash_password(req.password)
    user.description = req.description
    await db.commit()
    await db.refresh(user)
    return _user(user)


@app.delete("/users/{user_id}", status_code=204)
async def delete_user(user_id: int, db: AsyncSession = Depends(get_db)):
    user = await db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404)
    await db.delete(user)
    await db.commit()


# --- Coffee ---

@app.post("/coffee/", status_code=201)
async def create_coffee(req: CoffeeCreate, db: AsyncSession = Depends(get_db)):
    if not (1 <= req.intensity <= 10):
        return JSONResponse(status_code=400, content={"error": "Intensity must be between 1 and 10."})
    if not (1 <= req.rating <= 5):
        return JSONResponse(status_code=400, content={"error": "Rating must be between 1 and 5."})
    user_exists = (await db.execute(select(exists().where(User.id == req.userId)))).scalar()
    if not user_exists:
        return JSONResponse(status_code=400, content={"error": "User not found."})

    coffee = Coffee(
        type=req.type, shop=req.shop, location=req.location,
        intensity=req.intensity, rating=req.rating, temperature=req.temperature,
        notes=req.notes, user_id=req.userId,
    )
    db.add(coffee)
    await db.commit()
    await db.refresh(coffee)
    return JSONResponse(status_code=201, content=_coffee(coffee), headers={"Location": f"/coffee/{coffee.id}"})


@app.get("/coffee/")
async def list_coffees(db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(Coffee))
    return [_coffee(c) for c in result.scalars()]


@app.get("/coffee/{coffee_id}")
async def get_coffee(coffee_id: int, db: AsyncSession = Depends(get_db)):
    result = await db.execute(
        select(Coffee).where(Coffee.id == coffee_id).options(selectinload(Coffee.user))
    )
    coffee = result.scalar_one_or_none()
    if coffee is None:
        raise HTTPException(status_code=404)
    d = _coffee(coffee)
    d["user"] = _user(coffee.user)
    return d


@app.put("/coffee/{coffee_id}")
async def update_coffee(coffee_id: int, req: CoffeeUpdate, db: AsyncSession = Depends(get_db)):
    if not (1 <= req.intensity <= 10):
        return JSONResponse(status_code=400, content={"error": "Intensity must be between 1 and 10."})
    if not (1 <= req.rating <= 5):
        return JSONResponse(status_code=400, content={"error": "Rating must be between 1 and 5."})

    coffee = await db.get(Coffee, coffee_id)
    if coffee is None:
        raise HTTPException(status_code=404)

    user_exists = (await db.execute(select(exists().where(User.id == req.userId)))).scalar()
    if not user_exists:
        return JSONResponse(status_code=400, content={"error": "User not found."})

    coffee.type = req.type
    coffee.shop = req.shop
    coffee.location = req.location
    coffee.intensity = req.intensity
    coffee.rating = req.rating
    coffee.temperature = req.temperature
    coffee.notes = req.notes
    coffee.user_id = req.userId
    await db.commit()
    await db.refresh(coffee)
    return _coffee(coffee)


@app.delete("/coffee/{coffee_id}", status_code=204)
async def delete_coffee(coffee_id: int, db: AsyncSession = Depends(get_db)):
    coffee = await db.get(Coffee, coffee_id)
    if coffee is None:
        raise HTTPException(status_code=404)
    await db.delete(coffee)
    await db.commit()
