# alpha-fe

Vue 3 + TypeScript frontend.

---

## Prerequisites

### Node.js 20+

**Mac**

1. Install [Homebrew](https://brew.sh) if you don't have it:
   ```bash
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```
2. Install Node.js:
   ```bash
   brew install node
   ```
3. Confirm it worked:
   ```bash
   node --version
   ```

**Windows**

1. Download the LTS installer from [nodejs.org](https://nodejs.org/) and run it with default settings.
2. Open a new PowerShell window and confirm:
   ```powershell
   node --version
   ```

---

## Setup

Install dependencies once after cloning:

```bash
npm install
```

---

## Running

```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`.

The Vite dev server proxies all `/api` requests to the backend. By default it targets `http://localhost:8000`. Override with the `VITE_API_TARGET` environment variable.

Press `Ctrl + C` to stop the server.
