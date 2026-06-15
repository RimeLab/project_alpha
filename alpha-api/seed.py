from sqlalchemy import select
from database import SessionLocal
from models import User, Coffee
from security import hash_password


async def seed_db() -> None:
    async with SessionLocal() as db:
        existing = await db.execute(select(User).limit(1))
        if existing.scalar_one_or_none() is not None:
            return

        frost = User(username="frost", password=hash_password("password123"), description="Coffee obsessive")
        maya = User(username="maya", password=hash_password("password123"), description="Espresso purist")
        charlie = User(username="charlie", password=hash_password("password123"), description="Cold brew convert")
        db.add_all([frost, maya, charlie])
        await db.flush()

        db.add_all([
            Coffee(type="Espresso",   shop="Blue Bottle",   location="San Francisco, CA", intensity=9, rating=5, temperature="Hot",  notes="Nutty finish, great crema",           user_id=frost.id),
            Coffee(type="Latte",      shop="Sightglass",    location="San Francisco, CA", intensity=5, rating=4, temperature="Hot",  notes="Smooth and well-balanced",            user_id=frost.id),
            Coffee(type="Cortado",    shop="Ritual Coffee", location="San Francisco, CA", intensity=7, rating=5, temperature="Hot",  notes="Perfect ratio, clean aftertaste",     user_id=maya.id),
            Coffee(type="Flat White", shop="Four Barrel",   location="San Francisco, CA", intensity=6, rating=3, temperature="Hot",  notes="A bit over-extracted today",          user_id=maya.id),
            Coffee(type="Cold Brew",  shop="Philz Coffee",  location="Palo Alto, CA",     intensity=8, rating=4, temperature="Cold", notes="Strong and smooth, great for summer", user_id=charlie.id),
            Coffee(type="Iced Latte", shop="Verve Coffee",  location="Santa Cruz, CA",    intensity=4, rating=4, temperature="Cold", notes="Light and refreshing",                user_id=charlie.id),
        ])
        await db.commit()
