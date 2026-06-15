from pydantic import BaseModel, ConfigDict


class UserCreate(BaseModel):
    username: str
    password: str
    description: str | None = None


class UserUpdate(BaseModel):
    username: str
    password: str | None = None
    description: str | None = None


class UserOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    username: str
    description: str | None = None
