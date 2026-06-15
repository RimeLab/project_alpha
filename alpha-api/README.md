# alpha-api

Python FastAPI backend with async PostgreSQL via SQLAlchemy + asyncpg.

---

## Stack

| Layer | Tech |
|---|---|
| Framework | FastAPI |
| Server | Uvicorn |
| ORM | SQLAlchemy 2.0 (async) |
| Database driver | asyncpg |
| Password hashing | bcrypt |

---

## Prerequisites

**Docker (recommended)** — see the root README for install instructions. No local Python needed.

**Local Python** — Python 3.12+ with `pip`.

---

## Running locally (without Docker)

```bash
cd alpha-api
pip install -r requirements.txt
DATABASE_URL=postgresql+asyncpg://postgres:postgres@localhost/project_alpha \
  uvicorn main:app --reload
```

API available at `http://localhost:8000`. Interactive docs at `http://localhost:8000/docs`.

---

## Running with Docker

From the project root:

```bash
make stack-up
# or: docker compose up -d api
```

The container mounts `./alpha-api` so edits are picked up automatically by uvicorn's `--reload`.

---

## Environment variables

| Variable | Description | Default |
|---|---|---|
| `DATABASE_URL` | asyncpg connection string | `postgresql+asyncpg://postgres:postgres@localhost/project_alpha` |
| `CORS_ALLOWED_ORIGINS` | Comma-separated allowed origins | *(empty — no CORS header added)* |
| `APP_VERSION` | Version string returned by `GET /metadata` | `v0.0.1` |

---

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/` | Health check |
| GET | `/metadata` | App version |
| GET | `/docs` | Swagger UI |
| POST | `/users` | Create user |
| GET | `/users` | List users |
| GET | `/users/{id}` | Get user |
| PUT | `/users/{id}` | Update user |
| DELETE | `/users/{id}` | Delete user |
| POST | `/coffee` | Log a coffee |
| GET | `/coffee` | List coffees |
| GET | `/coffee/{id}` | Get coffee (includes user) |
| PUT | `/coffee/{id}` | Update coffee |
| DELETE | `/coffee/{id}` | Delete coffee |

### Validation rules

- `intensity` — integer 1–10
- `rating` — integer 1–5

---

## Project structure

```
alpha-api/
├── src/
│   ├── main.py          # FastAPI app, lifespan, middleware, router includes
│   ├── database.py      # Async engine and session factory
│   ├── models.py        # SQLAlchemy ORM models
│   ├── security.py      # bcrypt password hashing
│   ├── seed.py          # Seed data (runs on startup if DB is empty)
│   ├── routers/
│   │   ├── users.py     # /users routes
│   │   └── coffee.py    # /coffee routes
│   └── schemas/
│       ├── users.py     # User request/response schemas
│       └── coffee.py    # Coffee request/response schemas
├── requirements.txt
├── Dockerfile           # Dev image (uvicorn --reload, port 8000)
└── Dockerfile.prod      # Prod image (port $PORT, default 8080)
```
