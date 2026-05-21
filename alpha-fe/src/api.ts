const BASE = import.meta.env.VITE_API_BASE_URL ?? '/api'

export interface User {
  id: number
  username: string
  description: string | null
}

export interface Coffee {
  id: number
  type: string
  shop: string
  location: string
  intensity: number
  rating: number
  temperature: string
  notes: string | null
  userId: number
}

export interface CoffeeDetail extends Coffee {
  user: User
}

async function handle<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T
  const body = await res.json().catch(() => ({}))
  if (!res.ok) throw new Error(body.error ?? `HTTP ${res.status}`)
  return body as T
}

const json = (body: unknown) => ({
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
})

const request = (url: string, options?: RequestInit) =>
  fetch(url, { signal: AbortSignal.timeout(120_000), ...options })

export const getUsers    = () => request(`${BASE}/users`).then(r => handle<User[]>(r))
export const createUser  = (data: { username: string; password: string; description?: string | null }) =>
  request(`${BASE}/users`, { method: 'POST', ...json(data) }).then(r => handle<User>(r))
export const updateUser  = (id: number, data: { username: string; password?: string; description?: string | null }) =>
  request(`${BASE}/users/${id}`, { method: 'PUT', ...json(data) }).then(r => handle<User>(r))
export const deleteUser  = (id: number) =>
  request(`${BASE}/users/${id}`, { method: 'DELETE' }).then(r => handle<void>(r))

export const getCoffees   = () => request(`${BASE}/coffee`).then(r => handle<Coffee[]>(r))
export const createCoffee = (data: Omit<Coffee, 'id'>) =>
  request(`${BASE}/coffee`, { method: 'POST', ...json(data) }).then(r => handle<Coffee>(r))
export const updateCoffee = (id: number, data: Omit<Coffee, 'id'>) =>
  request(`${BASE}/coffee/${id}`, { method: 'PUT', ...json(data) }).then(r => handle<Coffee>(r))
export const deleteCoffee = (id: number) =>
  request(`${BASE}/coffee/${id}`, { method: 'DELETE' }).then(r => handle<void>(r))
