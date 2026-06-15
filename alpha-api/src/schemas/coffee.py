from pydantic import BaseModel, ConfigDict, Field

from .users import UserOut


class CoffeeCreate(BaseModel):
    type: str
    shop: str
    location: str
    intensity: int = Field(ge=1, le=10)
    rating: int = Field(ge=1, le=5)
    temperature: str
    notes: str | None = None
    userId: int


class CoffeeUpdate(CoffeeCreate):
    pass


class CoffeeOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    type: str
    shop: str
    location: str
    intensity: int
    rating: int
    temperature: str
    notes: str | None = None
    userId: int = Field(validation_alias="user_id")


class CoffeeDetailOut(CoffeeOut):
    user: UserOut
