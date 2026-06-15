from sqlalchemy import String, Integer, Text, ForeignKey
from sqlalchemy.orm import Mapped, mapped_column, relationship
from database import Base


class User(Base):
    __tablename__ = "Users"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    username: Mapped[str] = mapped_column("Username", String, nullable=False)
    password: Mapped[str] = mapped_column("Password", String, nullable=False)
    description: Mapped[str | None] = mapped_column("Description", String, nullable=True)

    coffees: Mapped[list["Coffee"]] = relationship(
        "Coffee", back_populates="user", cascade="all, delete-orphan"
    )


class Coffee(Base):
    __tablename__ = "Coffees"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    type: Mapped[str] = mapped_column("Type", String, nullable=False)
    shop: Mapped[str] = mapped_column("Shop", String, nullable=False)
    location: Mapped[str] = mapped_column("Location", String, nullable=False)
    intensity: Mapped[int] = mapped_column("Intensity", Integer, nullable=False)
    rating: Mapped[int] = mapped_column("Rating", Integer, nullable=False)
    temperature: Mapped[str] = mapped_column("Temperature", String, nullable=False)
    notes: Mapped[str | None] = mapped_column("Notes", Text, nullable=True)
    user_id: Mapped[int] = mapped_column("UserId", ForeignKey("Users.Id"), nullable=False)

    user: Mapped["User"] = relationship("User", back_populates="coffees")
