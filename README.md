# project_alpha

A Django blog application with a PostgreSQL database.

---

## Prerequisites

You need three tools installed before you can run this project: **Python**, **Docker**, and **Git**.

### 1. Python 3.12+

**Mac**

1. Open Terminal — press `Cmd + Space`, type *Terminal*, hit Enter.
2. Install [Homebrew](https://brew.sh) if you don't have it:
   ```bash
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```
3. Install Python:
   ```bash
   brew install python
   ```
4. Confirm it worked — you should see a version number:
   ```bash
   python3 --version
   ```

**Windows**

1. Open PowerShell — press `Win + S`, type *PowerShell*, hit Enter.
2. Download the Python installer from [python.org](https://www.python.org/downloads/) and run it.
3. On the first screen of the installer, **check the box that says "Add Python to PATH"** before clicking Install. This is easy to miss and important.
4. Close and reopen PowerShell, then confirm it worked:
   ```powershell
   python --version
   ```

> **Note:** On Windows the command is `python`, not `python3`. Wherever this guide shows `python3`, use `python` instead.

---

### 2. Docker Desktop

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
6. Open a new PowerShell window and confirm it worked:
   ```powershell
   docker --version
   ```

---

### 3. Git

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

Run these commands once to get everything ready. Open a terminal (Terminal on Mac, PowerShell on Windows) and follow each step.

### 1. Clone the repository

```bash
git clone <repository-url>
cd project_alpha
```

### 2. Create a virtual environment

A virtual environment keeps this project's Python packages separate from everything else on your machine.

**Mac:**
```bash
python3 -m venv venv
source venv/bin/activate
```

**Windows:**
```powershell
python -m venv venv
venv\Scripts\activate
```

You'll know it's active when you see `(venv)` at the start of your prompt. **You need to run the activate command every time you open a new terminal window.**

### 3. Install dependencies

```bash
pip install -r requirements.txt
```

### 4. Configure environment variables

The project reads database credentials from a `.env` file in the project root.

**Mac:**
```bash
cp .env.example .env
```

**Windows:**
```powershell
copy .env.example .env
```

### 5. Start the database

Make sure Docker Desktop is open and running, then:

```bash
docker compose up -d
```

This starts PostgreSQL in the background. To stop it later, run `docker compose down`.

### 6. Run migrations

This creates the database tables the app needs:

```bash
python manage.py migrate
```

### 7. Load sample data

This creates 5 sample blog posts and an admin account (username: `admin`, password: `admin`):

```bash
python manage.py seed_posts
```

---

## Running the development server

```bash
python manage.py runserver
```

Open your browser and go to:

| URL | What you'll see |
|---|---|
| `http://localhost:8000/` | Redirects to the blog |
| `http://localhost:8000/blog/` | Blog post list |
| `http://localhost:8000/admin/` | Admin panel (login: `admin` / `admin`) |

Press `Ctrl + C` in the terminal to stop the server.

---

## Common commands

### Installing `make` on Windows

`make` is built into Mac and Linux but not Windows. Install it once using **Chocolatey**, a package manager for Windows.

**Step 1 — Install Chocolatey**

Open PowerShell **as Administrator** (right-click the PowerShell icon → *Run as administrator*) and run:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

Close and reopen PowerShell when it finishes.

**Step 2 — Install `make`**

```powershell
choco install make -y
```

**Step 3 — Confirm it worked**

```powershell
make --version
```

Once installed, all `make` commands work the same on Windows as on Mac.

---

### Command reference

| Task | Mac / Windows with `make` | Windows without `make` |
|---|---|---|
| Start the server | `make run` | `python manage.py runserver` |
| Start the database | `make db-up` | `docker compose up -d` |
| Stop the database | `make db-down` | `docker compose down` |
| Apply migrations | `make migrate` | `python manage.py makemigrations && python manage.py migrate` |
| Install dependencies | `make install` | `pip install -r requirements.txt` |
| Run tests | `make test` | `python manage.py test` |
