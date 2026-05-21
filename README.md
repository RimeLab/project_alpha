# project_alpha

A .NET 10 Web API backend with a Vue 3 + TypeScript frontend and a PostgreSQL database.

- Backend: [`AlphaApi/`](AlphaApi/README.md)
- Frontend: [`alpha-fe/`](alpha-fe/README.md)

## Overview

```mermaid
flowchart TD
    Browser(["Browser"])

    subgraph fe["fe · localhost:5173"]
        FE["Vue 3 + TypeScript\nVite"]
    end

    subgraph api["api · localhost:5221"]
        API["ASP.NET Core · .NET 10"]
    end

    subgraph db["db · localhost:5432"]
        DB[("PostgreSQL 18")]
    end

    Browser -->|HTTP| fe
    fe -->|REST / JSON| api
    api -->|EF Core · Npgsql| db
```

---

## Prerequisites

You need two tools installed before you can run this project: **Docker** and **Git**.

### 1. Docker Desktop

Docker runs the PostgreSQL database inside a container so you don't have to install Postgres manually.

**Mac**

1. Download Docker Desktop for Mac from [docker.com](https://www.docker.com/products/docker-desktop/).
2. Open the `.dmg` file and drag Docker to your Applications folder.
3. Launch Docker from Applications and wait until the status bar in the bottom-left says **"Engine running"**.
4. Confirm it worked:
   ```bash
   docker --version
   ```

**Windows**

Docker on Windows requires WSL 2 (Windows Subsystem for Linux). Follow these steps in order:

1. Open PowerShell **as Administrator** — right-click the PowerShell icon and choose *Run as administrator*.
2. Install WSL 2:
   ```powershell
   wsl --install
   ```
3. Restart your computer when prompted.
4. Download Docker Desktop for Windows from [docker.com](https://www.docker.com/products/docker-desktop/) and run the installer with default settings.
5. Launch Docker Desktop and wait until the status bar in the bottom-left says **"Engine running"**.
6. Open a new PowerShell window and confirm:
   ```powershell
   docker --version
   ```

---

### 2. Git

**Mac** — Git comes pre-installed. Confirm with:
```bash
git --version
```
If it's not installed, run `brew install git`.

**Windows**

1. Download the installer from [git-scm.com](https://git-scm.com/download/win) and run it.
2. On the *Adjusting your PATH environment* screen, select **"Git from the command line and also from 3rd-party software"** (the default).
3. Leave all other options at their defaults and complete the install.
4. Open a new PowerShell window and confirm:
   ```powershell
   git --version
   ```

---

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd project_alpha
```

### 2. Configure environment variables

The database container reads credentials from a `.env` file in the project root.

**Mac:**
```bash
cp .env.example .env
```

**Windows:**
```powershell
copy .env.example .env
```

Open `.env` and fill in values for `POSTGRES_USER`, `POSTGRES_PASSWORD`, and `POSTGRES_DB`.

### 3. Start the stack

Make sure Docker Desktop is open and running, then bring up all services (database, API, and frontend):

```bash
make stack-up
# or: docker compose up -d
```

Source files are mounted into each container, so edits you make locally are picked up automatically — no rebuild required.

| Service | URL |
|---|---|
| Frontend | `http://localhost:5173` |
| API | `http://localhost:5221` |
| Database | `localhost:5432` |

---

## Commands

### Full stack

| Task | Command |
|---|---|
| Start all services | `make stack-up` / `docker compose up -d` |
| Stop all services | `make stack-down` / `docker compose down` |
| Tail all logs | `make stack-logs` / `docker compose logs -f` |

### Database only

| Task | Command |
|---|---|
| Start the database | `make db-up` / `docker compose up -d db` |
| Stop the database | `make db-down` / `docker compose down` |
| Stop and delete all data | `make db-clean` / `docker compose down -v` |
| View database logs | `make db-logs` / `docker compose logs -f db` |

> **Warning:** `db-clean` permanently deletes the database volume and all stored data.

---

## Deployment

The project deploys to **Render** using the `render.yaml` Blueprint. The API connects to **Neon Serverless Postgres**.

### 1. Set up Neon

1. Create a project at [neon.tech](https://neon.tech).
2. From the project dashboard, copy the connection string for the `neondb_owner` role. It looks like:
   ```
   postgresql://neondb_owner:<password>@<endpoint>.neon.tech/neondb?sslmode=require
   ```
3. Convert it to the Npgsql format you'll paste into Render:
   ```
   Host=<endpoint>.neon.tech;Database=neondb;Username=neondb_owner;Password=<password>;SSL Mode=Require
   ```

### 2. Deploy to Render

1. Push the repo to GitHub.
2. Go to [render.com](https://render.com) → **New → Blueprint** and connect the repo. Render reads `render.yaml` and creates both services.
3. Once created, open the **alpha-api** service and set its environment variables:

   | Key | Value |
   |---|---|
   | `ConnectionStrings__DefaultConnection` | Npgsql connection string from step 1 |
   | `Cors__AllowedOrigins__0` | Render URL of alpha-fe (e.g. `https://alpha-fe.onrender.com`) |

4. Open the **alpha-fe** service and set:

   | Key | Value |
   |---|---|
   | `VITE_API_BASE_URL` | Render URL of alpha-api (e.g. `https://alpha-api.onrender.com`) |

5. Trigger a deploy on **alpha-fe** after setting `VITE_API_BASE_URL` so the URL is embedded into the build.

> Render terminates TLS at the edge — both services are reachable only over HTTPS. HTTP requests to either service are redirected to HTTPS automatically.

---

## Testing

The `AlphaApi.Tests` project contains unit and integration tests. Tests run against an in-memory database — no running database or Docker is needed.

```bash
dotnet test AlphaApi/AlphaApi.Tests
```

To see per-test output:

```bash
dotnet test AlphaApi/AlphaApi.Tests -v normal
```

---

## API examples

The API runs at `http://localhost:5221`. An interactive reference (Scalar) is available at `http://localhost:5221/scalar`.

### Users

**Create a user**
```bash
curl -X POST http://localhost:5221/users \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "secret", "description": "coffee enthusiast"}'
# {"id":1,"username":"alice","description":"coffee enthusiast"}
```

**List all users**
```bash
curl http://localhost:5221/users
# [{"id":1,"username":"alice","description":"coffee enthusiast"}]
```

**Get a single user**
```bash
curl http://localhost:5221/users/1
```

**Update a user**
```bash
curl -X PUT http://localhost:5221/users/1 \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "newpass", "description": "updated bio"}'
```

**Delete a user**
```bash
curl -X DELETE http://localhost:5221/users/1
```

---

### Coffee

`intensity` must be 1–10. `rating` must be 1–5. `temperature` is a free-text string (e.g. `"hot"`, `"iced"`).

**Log a coffee**
```bash
curl -X POST http://localhost:5221/coffee \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Flat White",
    "shop": "Onyx Coffee Lab",
    "location": "Bentonville, AR",
    "intensity": 7,
    "rating": 5,
    "temperature": "hot",
    "notes": "nutty, sweet finish",
    "userId": 1
  }'
# {"id":1,"type":"Flat White","shop":"Onyx Coffee Lab","location":"Bentonville, AR","intensity":7,"rating":5,"temperature":"hot","notes":"nutty, sweet finish","userId":1}
```

**List all coffees**
```bash
curl http://localhost:5221/coffee
```

**Get a single coffee** (includes user info)
```bash
curl http://localhost:5221/coffee/1
```

**Update a coffee**
```bash
curl -X PUT http://localhost:5221/coffee/1 \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Espresso",
    "shop": "Onyx Coffee Lab",
    "location": "Bentonville, AR",
    "intensity": 9,
    "rating": 4,
    "temperature": "hot",
    "notes": "bold, slightly bitter",
    "userId": 1
  }'
```

**Delete a coffee**
```bash
curl -X DELETE http://localhost:5221/coffee/1
```
