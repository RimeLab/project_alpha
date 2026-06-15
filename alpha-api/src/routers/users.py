from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from ..database import get_db
from ..models import User
from ..schemas.users import UserCreate, UserOut, UserUpdate
from ..security import hash_password

router = APIRouter(prefix="/users", tags=["users"])


@router.post("", status_code=201, response_model=UserOut)
async def create_user(req: UserCreate, response: Response, db: AsyncSession = Depends(get_db)):
    user = User(username=req.username, password=hash_password(req.password), description=req.description)
    db.add(user)
    await db.commit()
    await db.refresh(user)
    response.headers["Location"] = f"/users/{user.id}"
    return user


@router.get("", response_model=list[UserOut])
async def list_users(db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(User))
    return result.scalars().all()


@router.get("/{user_id}", response_model=UserOut)
async def get_user(user_id: int, db: AsyncSession = Depends(get_db)):
    user = await db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404)
    return user


@router.put("/{user_id}", response_model=UserOut)
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
    return user


@router.delete("/{user_id}", status_code=204)
async def delete_user(user_id: int, db: AsyncSession = Depends(get_db)):
    user = await db.get(User, user_id)
    if user is None:
        raise HTTPException(status_code=404)
    await db.delete(user)
    await db.commit()
