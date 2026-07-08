'use client'
import { useState, useEffect } from 'react'
import * as api from '@/lib/api'
import styles from './UserPanel.module.css'

export default function UserPanel() {
  const [users, setUsers] = useState<api.User[]>([])
  const [editing, setEditing] = useState<api.User | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [form, setForm] = useState({ username: '', password: '', description: '' })

  async function load() {
    setUsers(await api.getUsers())
  }

  function startEdit(user: api.User) {
    setEditing(user)
    setForm({ username: user.username, password: '', description: user.description ?? '' })
    setError('')
  }

  function cancelEdit() {
    setEditing(null)
    setForm({ username: '', password: '', description: '' })
    setError('')
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    if (!form.username.trim()) { setError('Username is required.'); return }
    if (!editing && !form.password.trim()) { setError('Password is required.'); return }
    setBusy(true)
    setError('')
    try {
      if (editing) {
        await api.updateUser(editing.id, {
          username: form.username,
          password: form.password || undefined,
          description: form.description || null,
        })
        flash('User updated.')
      } else {
        await api.createUser({
          username: form.username,
          password: form.password,
          description: form.description || null,
        })
        flash('User created.')
      }
      cancelEdit()
      await load()
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  async function remove(user: api.User) {
    if (!confirm(`Delete "${user.username}"?`)) return
    await api.deleteUser(user.id)
    if (editing?.id === user.id) cancelEdit()
    await load()
  }

  function flash(msg: string) {
    setSuccess(msg)
    setTimeout(() => setSuccess(''), 3000)
  }

  useEffect(() => { load() }, [])

  return (
    <div className={styles.panel}>
      <aside className={styles.formCol}>
        <h2 className={styles.colTitle}>{editing ? 'Edit User' : 'New User'}</h2>

        <form className={styles.form} onSubmit={submit}>
          <div className={styles.field}>
            <label>Username</label>
            <input
              value={form.username}
              onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
              type="text"
              placeholder="username"
              autoComplete="off"
            />
          </div>

          <div className={styles.field}>
            <label>Password</label>
            <input
              value={form.password}
              onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
              type="password"
              placeholder={editing ? 'leave blank to keep existing' : 'password'}
              autoComplete="new-password"
            />
          </div>

          <div className={styles.field}>
            <label>
              Description <span className={styles.optional}>optional</span>
            </label>
            <input
              value={form.description}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              type="text"
              placeholder="short bio"
            />
          </div>

          {error && <p className={`${styles.msg} ${styles.error}`}>{error}</p>}
          {success && <p className={`${styles.msg} ${styles.success}`}>{success}</p>}

          <div className={styles.formActions}>
            <button type="submit" className={styles.btnPrimary} disabled={busy}>
              {editing ? 'Update' : 'Create'}
            </button>
            {editing && (
              <button type="button" className={styles.btnGhost} onClick={cancelEdit}>
                Cancel
              </button>
            )}
          </div>
        </form>
      </aside>

      <section className={styles.listCol}>
        <h2 className={styles.colTitle}>
          Users <span className={styles.count}>{users.length}</span>
        </h2>

        <div className={styles.list}>
          {users.length === 0 && <div className={styles.empty}>No users yet.</div>}

          {users.map(user => (
            <div
              key={user.id}
              className={`${styles.listItem}${editing?.id === user.id ? ' ' + styles.selected : ''}`}
            >
              <div className={styles.itemBody}>
                <span className={styles.itemName}>{user.username}</span>
                <span className={styles.itemSub}>{user.description ?? '—'}</span>
              </div>
              <div className={styles.itemMeta}>
                <span className={styles.badge}>ID {user.id}</span>
              </div>
              <div className={styles.itemActions}>
                <button className={styles.btnSm} onClick={() => startEdit(user)}>Edit</button>
                <button
                  className={`${styles.btnSm} ${styles.danger}`}
                  onClick={() => remove(user)}
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
