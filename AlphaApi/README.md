# AlphaApi

.NET 10 minimal Web API backed by PostgreSQL.

Base URL: `http://localhost:5221`

---

## Prerequisites

### .NET 10 SDK

**Mac**

1. Download the .NET 10 SDK installer from [dot.net](https://dotnet.microsoft.com/download).
2. Run the installer and follow the prompts.
3. Confirm it worked:
   ```bash
   dotnet --version
   ```

**Windows**

1. Download the .NET 10 SDK installer from [dot.net](https://dotnet.microsoft.com/download).
2. Run the installer with default settings.
3. Open a new PowerShell window and confirm:
   ```powershell
   dotnet --version
   ```

### EF Core CLI

```bash
dotnet tool install --global dotnet-ef
```

---

## Setup

### 1. Configure the database connection

The default connection string in `appsettings.json` points to a local Postgres instance:

```
Host=localhost;Database=project_alpha;Username=postgres;Password=postgres
```

Edit `appsettings.json` to match your credentials, or override with environment variables using the `ConnectionStrings__DefaultConnection` key.

If you're running the full Docker stack from the project root, the connection string is injected automatically via `docker-compose.yml` — no changes needed.

### 2. Apply migrations

Run this once (and again after any new migration is added):

```bash
dotnet ef database update
```

---

## Running

### Local (dotnet CLI)

```bash
dotnet run
```

The API will be available at `http://localhost:5221`. Press `Ctrl + C` to stop.

### Docker (full stack)

From the project root:

```bash
docker compose up -d
```

See the [root README](../README.md) for the full stack command reference.

---

## Seed Data

On first startup the API automatically seeds the database with test users and coffee entries. Seeding is skipped if the `Users` table already has rows.

### Users

| ID | Username | Password | Description |
|----|----------|----------|-------------|
| 1 | `frost` | `password123` | Coffee obsessive |
| 2 | `maya` | `password123` | Espresso purist |
| 3 | `charlie` | `password123` | Cold brew convert |

### Coffees

| ID | Type | Shop | Location | Intensity | Rating | Temperature | User |
|----|------|------|----------|-----------|--------|-------------|------|
| 1 | Espresso | Blue Bottle | San Francisco, CA | 9 | 5 | Hot | frost |
| 2 | Latte | Sightglass | San Francisco, CA | 5 | 4 | Hot | frost |
| 3 | Cortado | Ritual Coffee | San Francisco, CA | 7 | 5 | Hot | maya |
| 4 | Flat White | Four Barrel | San Francisco, CA | 6 | 3 | Hot | maya |
| 5 | Cold Brew | Philz Coffee | Palo Alto, CA | 8 | 4 | Cold | charlie |
| 6 | Iced Latte | Verve Coffee | Santa Cruz, CA | 4 | 4 | Cold | charlie |

To reset seed data, clear the database and restart the container:

```bash
docker compose down -v && docker compose up -d
```

---

## Endpoints

### Utility

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | Health check |
| `GET` | `/metadata` | App version info |

### User

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/users` | Create a user |
| `GET` | `/users` | List all users |
| `GET` | `/users/{id}` | Get user by ID |
| `PUT` | `/users/{id}` | Update user |
| `DELETE` | `/users/{id}` | Delete user |

### Coffee

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/coffee` | Log a coffee |
| `GET` | `/coffee` | List all coffees |
| `GET` | `/coffee/{id}` | Get coffee by ID (includes user) |
| `PUT` | `/coffee/{id}` | Update coffee |
| `DELETE` | `/coffee/{id}` | Delete coffee |

---

## Example Requests

### POST /users

```bash
curl -s -X POST http://localhost:5221/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alex",
    "password": "password123",
    "description": "New user"
  }'
```

```json
{
  "id": 4,
  "username": "alex",
  "description": "New user"
}
```

---

### GET /users

```bash
curl -s http://localhost:5221/users
```

```json
[
  { "id": 1, "username": "frost",   "description": "Coffee obsessive" },
  { "id": 2, "username": "maya",    "description": "Espresso purist" },
  { "id": 3, "username": "charlie", "description": "Cold brew convert" }
]
```

---

### GET /users/{id}

```bash
curl -s http://localhost:5221/users/1
```

```json
{ "id": 1, "username": "frost", "description": "Coffee obsessive" }
```

---

### PUT /users/{id}

`password` is optional — omit it to keep the existing password.

```bash
curl -s -X PUT http://localhost:5221/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "username": "frost",
    "description": "Updated bio"
  }'
```

```json
{ "id": 1, "username": "frost", "description": "Updated bio" }
```

---

### DELETE /users/{id}

```bash
curl -s -X DELETE http://localhost:5221/users/3
```

Returns `204 No Content`.

---

### POST /coffee

`intensity` must be 1–10. `rating` must be 1–5. `notes` is optional.

```bash
curl -s -X POST http://localhost:5221/coffee \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Cappuccino",
    "shop": "Sightglass",
    "location": "San Francisco, CA",
    "intensity": 7,
    "rating": 5,
    "temperature": "Hot",
    "notes": "Perfectly textured milk, great balance",
    "userId": 1
  }'
```

```json
{
  "id": 7,
  "type": "Cappuccino",
  "shop": "Sightglass",
  "location": "San Francisco, CA",
  "intensity": 7,
  "rating": 5,
  "temperature": "Hot",
  "notes": "Perfectly textured milk, great balance",
  "userId": 1
}
```

---

### GET /coffee

```bash
curl -s http://localhost:5221/coffee
```

```json
[
  { "id": 1, "type": "Espresso",   "shop": "Blue Bottle",   "location": "San Francisco, CA", "intensity": 9, "rating": 5, "temperature": "Hot",  "notes": "Nutty finish, great crema",           "userId": 1 },
  { "id": 2, "type": "Latte",      "shop": "Sightglass",    "location": "San Francisco, CA", "intensity": 5, "rating": 4, "temperature": "Hot",  "notes": "Smooth and well-balanced",            "userId": 1 },
  { "id": 3, "type": "Cortado",    "shop": "Ritual Coffee", "location": "San Francisco, CA", "intensity": 7, "rating": 5, "temperature": "Hot",  "notes": "Perfect ratio, clean aftertaste",     "userId": 2 },
  { "id": 4, "type": "Flat White", "shop": "Four Barrel",   "location": "San Francisco, CA", "intensity": 6, "rating": 3, "temperature": "Hot",  "notes": "A bit over-extracted today",          "userId": 2 },
  { "id": 5, "type": "Cold Brew",  "shop": "Philz Coffee",  "location": "Palo Alto, CA",     "intensity": 8, "rating": 4, "temperature": "Cold", "notes": "Strong and smooth, great for summer", "userId": 3 },
  { "id": 6, "type": "Iced Latte", "shop": "Verve Coffee",  "location": "Santa Cruz, CA",    "intensity": 4, "rating": 4, "temperature": "Cold", "notes": "Light and refreshing",                "userId": 3 }
]
```

---

### GET /coffee/{id}

Includes the associated user object.

```bash
curl -s http://localhost:5221/coffee/1
```

```json
{
  "id": 1,
  "type": "Espresso",
  "shop": "Blue Bottle",
  "location": "San Francisco, CA",
  "intensity": 9,
  "rating": 5,
  "temperature": "Hot",
  "notes": "Nutty finish, great crema",
  "userId": 1,
  "user": {
    "id": 1,
    "username": "frost",
    "description": "Coffee obsessive"
  }
}
```

---

### PUT /coffee/{id}

```bash
curl -s -X PUT http://localhost:5221/coffee/1 \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Espresso",
    "shop": "Blue Bottle",
    "location": "San Francisco, CA",
    "intensity": 10,
    "rating": 5,
    "temperature": "Hot",
    "notes": "Even better on the second visit",
    "userId": 1
  }'
```

```json
{
  "id": 1,
  "type": "Espresso",
  "shop": "Blue Bottle",
  "location": "San Francisco, CA",
  "intensity": 10,
  "rating": 5,
  "temperature": "Hot",
  "notes": "Even better on the second visit",
  "userId": 1
}
```

---

### DELETE /coffee/{id}

```bash
curl -s -X DELETE http://localhost:5221/coffee/6
```

Returns `204 No Content`.

---

## Validation

| Field | Rule |
|-------|------|
| `intensity` | Integer, 1–10 |
| `rating` | Integer, 1–5 |
| `userId` (coffee) | Must reference an existing user |
| `password` (PUT /users) | Optional — omit to keep existing |

Validation failures return `400 Bad Request` with an `error` message.

User passwords are never returned in any response.
