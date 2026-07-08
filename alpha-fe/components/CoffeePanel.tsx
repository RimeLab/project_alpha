'use client'
import { useState, useEffect } from 'react'
import * as api from '@/lib/api'
import styles from './CoffeePanel.module.css'

const tempClass: Record<string, string> = {
  hot: styles.hot,
  warm: styles.warm,
  cold: styles.cold,
  iced: styles.iced,
}

function stars(n: number) { return '★'.repeat(n) + '☆'.repeat(5 - n) }
function dots(n: number)  { return '●'.repeat(n) + '○'.repeat(10 - n) }

export default function CoffeePanel() {
  const [coffees, setCoffees] = useState<api.Coffee[]>([])
  const [users, setUsers] = useState<api.User[]>([])
  const [editing, setEditing] = useState<api.Coffee | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [form, setForm] = useState({
    type: '',
    shop: '',
    location: '',
    intensity: 5,
    rating: 3,
    temperature: 'Hot',
    notes: '',
    userId: 0,
  })

  async function load() {
    const [c, u] = await Promise.all([api.getCoffees(), api.getUsers()])
    setCoffees(c)
    setUsers(u)
    setForm(f => ({ ...f, userId: f.userId || u[0]?.id || 0 }))
  }

  function startEdit(coffee: api.Coffee) {
    setEditing(coffee)
    setForm({
      type:        coffee.type,
      shop:        coffee.shop,
      location:    coffee.location,
      intensity:   coffee.intensity,
      rating:      coffee.rating,
      temperature: coffee.temperature,
      notes:       coffee.notes ?? '',
      userId:      coffee.userId,
    })
    setError('')
  }

  function cancelEdit(currentUsers = users) {
    setEditing(null)
    setForm({
      type: '', shop: '', location: '',
      intensity: 5, rating: 3, temperature: 'Hot',
      notes: '', userId: currentUsers[0]?.id ?? 0,
    })
    setError('')
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    if (!form.type.trim())     { setError('Type is required.');     return }
    if (!form.shop.trim())     { setError('Shop is required.');     return }
    if (!form.location.trim()) { setError('Location is required.'); return }
    if (!form.userId)          { setError('User is required.');     return }
    setBusy(true)
    setError('')
    const data: Omit<api.Coffee, 'id'> = {
      type:        form.type,
      shop:        form.shop,
      location:    form.location,
      intensity:   form.intensity,
      rating:      form.rating,
      temperature: form.temperature,
      notes:       form.notes || null,
      userId:      form.userId,
    }
    try {
      if (editing) {
        await api.updateCoffee(editing.id, data)
        flash('Coffee updated.')
      } else {
        await api.createCoffee(data)
        flash('Coffee logged.')
      }
      cancelEdit()
      await load()
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  async function remove(coffee: api.Coffee) {
    if (!confirm(`Delete "${coffee.type}" at ${coffee.shop}?`)) return
    await api.deleteCoffee(coffee.id)
    if (editing?.id === coffee.id) cancelEdit()
    await load()
  }

  function flash(msg: string) {
    setSuccess(msg)
    setTimeout(() => setSuccess(''), 3000)
  }

  function userName(id: number) {
    return users.find(u => u.id === id)?.username ?? `#${id}`
  }

  useEffect(() => { load() }, [])

  return (
    <div className={styles.panel}>
      <aside className={styles.formCol}>
        <h2 className={styles.colTitle}>{editing ? 'Edit Coffee' : 'Log Coffee'}</h2>

        <form className={styles.form} onSubmit={submit}>
          <div className={styles.field}>
            <label>Type</label>
            <input
              value={form.type}
              onChange={e => setForm(f => ({ ...f, type: e.target.value }))}
              type="text"
              placeholder="Espresso, Latte, Cold Brew…"
            />
          </div>

          <div className={styles.field}>
            <label>Shop</label>
            <input
              value={form.shop}
              onChange={e => setForm(f => ({ ...f, shop: e.target.value }))}
              type="text"
              placeholder="Blue Bottle"
            />
          </div>

          <div className={styles.field}>
            <label>Location</label>
            <input
              value={form.location}
              onChange={e => setForm(f => ({ ...f, location: e.target.value }))}
              type="text"
              placeholder="San Francisco, CA"
            />
          </div>

          <div className={styles.rowFields}>
            <div className={styles.field}>
              <label>Intensity <span className={styles.optional}>1–10</span></label>
              <input
                value={form.intensity}
                onChange={e => setForm(f => ({ ...f, intensity: Number(e.target.value) }))}
                type="number"
                min="1"
                max="10"
              />
            </div>
            <div className={styles.field}>
              <label>Rating <span className={styles.optional}>1–5</span></label>
              <input
                value={form.rating}
                onChange={e => setForm(f => ({ ...f, rating: Number(e.target.value) }))}
                type="number"
                min="1"
                max="5"
              />
            </div>
          </div>

          <div className={styles.field}>
            <label>Temperature</label>
            <select
              value={form.temperature}
              onChange={e => setForm(f => ({ ...f, temperature: e.target.value }))}
            >
              <option>Hot</option>
              <option>Warm</option>
              <option>Cold</option>
              <option>Iced</option>
            </select>
          </div>

          <div className={styles.field}>
            <label>User</label>
            <select
              value={form.userId}
              onChange={e => setForm(f => ({ ...f, userId: Number(e.target.value) }))}
            >
              {users.map(u => (
                <option key={u.id} value={u.id}>{u.username}</option>
              ))}
            </select>
          </div>

          <div className={styles.field}>
            <label>Notes <span className={styles.optional}>optional</span></label>
            <textarea
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
              rows={3}
              placeholder="Tasting notes, impressions…"
            />
          </div>

          {error && <p className={`${styles.msg} ${styles.error}`}>{error}</p>}
          {success && <p className={`${styles.msg} ${styles.success}`}>{success}</p>}

          <div className={styles.formActions}>
            <button type="submit" className={styles.btnPrimary} disabled={busy}>
              {editing ? 'Update' : 'Log'}
            </button>
            {editing && (
              <button type="button" className={styles.btnGhost} onClick={() => cancelEdit()}>
                Cancel
              </button>
            )}
          </div>
        </form>
      </aside>

      <section className={styles.listCol}>
        <h2 className={styles.colTitle}>
          Coffees <span className={styles.count}>{coffees.length}</span>
        </h2>

        <div className={styles.list}>
          {coffees.length === 0 && <div className={styles.empty}>No coffees logged yet.</div>}

          {coffees.map(coffee => (
            <div
              key={coffee.id}
              className={`${styles.listItem}${editing?.id === coffee.id ? ' ' + styles.selected : ''}`}
            >
              <div className={styles.itemBody}>
                <div className={styles.itemTop}>
                  <span className={styles.itemName}>{coffee.type}</span>
                  <span className={styles.itemShop}>{coffee.shop}</span>
                </div>
                <span className={styles.itemSub}>{coffee.location} · {userName(coffee.userId)}</span>
                {coffee.notes && <span className={styles.itemNotes}>{coffee.notes}</span>}
              </div>

              <div className={styles.itemMeta}>
                <span className={`${styles.badge} ${tempClass[coffee.temperature.toLowerCase()] ?? ''}`}>
                  {coffee.temperature}
                </span>
                <span className={styles.badge} title="Intensity">{dots(coffee.intensity)}</span>
                <span className={`${styles.badge} ${styles.stars}`} title="Rating">{stars(coffee.rating)}</span>
              </div>

              <div className={styles.itemActions}>
                <button className={styles.btnSm} onClick={() => startEdit(coffee)}>Edit</button>
                <button
                  className={`${styles.btnSm} ${styles.danger}`}
                  onClick={() => remove(coffee)}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
