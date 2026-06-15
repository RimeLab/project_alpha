from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy import select, exists
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import selectinload

from ..database import get_db
from ..models import Coffee, User
from ..schemas.coffee import CoffeeCreate, CoffeeDetailOut, CoffeeOut, CoffeeUpdate

router = APIRouter(prefix="/coffee", tags=["coffee"])


async def _assert_user_exists(user_id: int, db: AsyncSession) -> None:
    found = (await db.execute(select(exists().where(User.id == user_id)))).scalar()
    if not found:
        raise HTTPException(status_code=400, detail="User not found.")


@router.post("", status_code=201, response_model=CoffeeOut)
async def create_coffee(req: CoffeeCreate, response: Response, db: AsyncSession = Depends(get_db)):
    await _assert_user_exists(req.userId, db)
    coffee = Coffee(
        type=req.type, shop=req.shop, location=req.location,
        intensity=req.intensity, rating=req.rating, temperature=req.temperature,
        notes=req.notes, user_id=req.userId,
    )
    db.add(coffee)
    await db.commit()
    await db.refresh(coffee)
    response.headers["Location"] = f"/coffee/{coffee.id}"
    return coffee


@router.get("", response_model=list[CoffeeOut])
async def list_coffees(db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(Coffee))
    return result.scalars().all()


@router.get("/{coffee_id}", response_model=CoffeeDetailOut)
async def get_coffee(coffee_id: int, db: AsyncSession = Depends(get_db)):
    result = await db.execute(
        select(Coffee).where(Coffee.id == coffee_id).options(selectinload(Coffee.user))
    )
    coffee = result.scalar_one_or_none()
    if coffee is None:
        raise HTTPException(status_code=404)
    return coffee


@router.put("/{coffee_id}", response_model=CoffeeOut)
async def update_coffee(coffee_id: int, req: CoffeeUpdate, db: AsyncSession = Depends(get_db)):
    coffee = await db.get(Coffee, coffee_id)
    if coffee is None:
        raise HTTPException(status_code=404)
    await _assert_user_exists(req.userId, db)
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
    return coffee


@router.delete("/{coffee_id}", status_code=204)
async def delete_coffee(coffee_id: int, db: AsyncSession = Depends(get_db)):
    coffee = await db.get(Coffee, coffee_id)
    if coffee is None:
        raise HTTPException(status_code=404)
    await db.delete(coffee)
    await db.commit()
