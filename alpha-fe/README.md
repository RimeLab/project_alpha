# alpha-fe

Next.js 15 + React 19 + TypeScript frontend.

---

## Prerequisites

### Node.js 20+

**Mac**

```bash
brew install node
node --version
```

**Windows**

Download the LTS installer from [nodejs.org](https://nodejs.org/) and run it with default settings.

---

## Setup

```bash
npm install
```

---

## Running

```bash
npm run dev
```

Frontend available at `http://localhost:3000`.

API requests to `/api/*` are proxied to the backend. By default the target is `http://localhost:8000`. Override with the `API_TARGET` environment variable.

---

## Environment variables

| Variable | Where | Description |
|---|---|---|
| `API_TARGET` | dev server only | Backend URL for the dev proxy (default: `http://localhost:8000`) |
| `NEXT_PUBLIC_API_BASE_URL` | production build | Full URL of the deployed API (e.g. `https://alpha-api.onrender.com`) |
