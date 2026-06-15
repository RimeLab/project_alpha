from pydantic import BaseModel


class UserCreate(BaseModel):
    username: str
    password: str
    description: str | None = None


class UserUpdate(BaseModel):
    username: str
    password: str | None = None
    description: str | None = None


class CoffeeCreate(BaseModel):
    type: str
    shop: str
    location: str
    intensity: int
    rating: int
    temperature: str
    notes: str | None = None
    userId: int


class CoffeeUpdate(CoffeeCreate):
    pass
