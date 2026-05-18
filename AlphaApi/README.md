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

## Endpoints

### Utility

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | Health check |
| `GET` | `/metadata` | App version info |

### User

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/user` | Create a user |
| `GET` | `/user` | List all users |
| `GET` | `/user/{id}` | Get user by ID |
| `PUT` | `/user/{id}` | Update user |
| `DELETE` | `/user/{id}` | Delete user |

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

### POST /user

```bash
curl -s -X POST http://localhost:5221/user \
  -H "Content-Type: application/json" \
  -d '{
    "username": "frost",
    "password": "hunter2",
    "description": "Coffee enthusiast"
  }'
```

```json
{
  "id": 1,
  "username": "frost",
  "description": "Coffee enthusiast"
}
```

---

### GET /user

```bash
curl -s http://localhost:5221/user
```

```json
[
  { "id": 1, "username": "frost", "description": "Coffee enthusiast" }
]
```

---

### GET /user/{id}

```bash
curl -s http://localhost:5221/user/1
```

```json
{ "id": 1, "username": "frost", "description": "Coffee enthusiast" }
```

---

### PUT /user/{id}

`password` is optional — omit it to keep the existing password.

```bash
curl -s -X PUT http://localhost:5221/user/1 \
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

### DELETE /user/{id}

```bash
curl -s -X DELETE http://localhost:5221/user/1
```

Returns `204 No Content`.

---

### POST /coffee

`intensity` must be 1–10. `rating` must be 1–5. `notes` is optional.

```bash
curl -s -X POST http://localhost:5221/coffee \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Espresso",
    "shop": "Blue Bottle",
    "location": "San Francisco, CA",
    "intensity": 8,
    "rating": 5,
    "temperature": "Hot",
    "notes": "Nutty finish, great crema",
    "userId": 1
  }'
```

```json
{
  "id": 1,
  "type": "Espresso",
  "shop": "Blue Bottle",
  "location": "San Francisco, CA",
  "intensity": 8,
  "rating": 5,
  "temperature": "Hot",
  "notes": "Nutty finish, great crema",
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
  {
    "id": 1,
    "type": "Espresso",
    "shop": "Blue Bottle",
    "location": "San Francisco, CA",
    "intensity": 8,
    "rating": 5,
    "temperature": "Hot",
    "notes": "Nutty finish, great crema",
    "userId": 1
  }
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
  "intensity": 8,
  "rating": 5,
  "temperature": "Hot",
  "notes": "Nutty finish, great crema",
  "userId": 1,
  "user": {
    "id": 1,
    "username": "frost",
    "description": "Coffee enthusiast"
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
    "intensity": 9,
    "rating": 4,
    "temperature": "Hot",
    "notes": "Bolder on second visit",
    "userId": 1
  }'
```

```json
{
  "id": 1,
  "type": "Espresso",
  "shop": "Blue Bottle",
  "location": "San Francisco, CA",
  "intensity": 9,
  "rating": 4,
  "temperature": "Hot",
  "notes": "Bolder on second visit",
  "userId": 1
}
```

---

### DELETE /coffee/{id}

```bash
curl -s -X DELETE http://localhost:5221/coffee/1
```

Returns `204 No Content`.

---

## Validation

| Field | Rule |
|-------|------|
| `intensity` | Integer, 1–10 |
| `rating` | Integer, 1–5 |
| `userId` (coffee) | Must reference an existing user |
| `password` (PUT /user) | Optional — omit to keep existing |

Validation failures return `400 Bad Request` with an `error` message.

User passwords are never returned in any response.
