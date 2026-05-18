# project_alpha

A .NET 10 Web API backend with a Vue 3 + TypeScript frontend and a PostgreSQL database.

- Backend: [`AlphaApi/`](AlphaApi/README.md)
- Frontend: [`alpha-fe/`](alpha-fe/README.md)

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

### 3. Start the database

Make sure Docker Desktop is open and running, then:

```bash
make db-up
# or: docker compose up -d
```

---

## Database commands

| Task | Command |
|---|---|
| Start the database | `make db-up` / `docker compose up -d` |
| Stop the database | `make db-down` / `docker compose down` |
| Stop and delete all data | `make db-clean` / `docker compose down -v` |
| View database logs | `make db-logs` / `docker compose logs -f db` |

> **Warning:** `db-clean` permanently deletes the database volume and all stored data.
