<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import * as api from '@/api'

const users = ref<api.User[]>([])
const editing = ref<api.User | null>(null)
const busy = ref(false)
const error = ref('')
const success = ref('')

const form = reactive({ username: '', password: '', description: '' })

async function load() {
  users.value = await api.getUsers()
}

function startEdit(user: api.User) {
  editing.value = user
  form.username = user.username
  form.password = ''
  form.description = user.description ?? ''
  error.value = ''
}

function cancelEdit() {
  editing.value = null
  form.username = ''
  form.password = ''
  form.description = ''
  error.value = ''
}

async function submit() {
  if (!form.username.trim()) { error.value = 'Username is required.'; return }
  if (!editing.value && !form.password.trim()) { error.value = 'Password is required.'; return }
  busy.value = true
  error.value = ''
  try {
    if (editing.value) {
      await api.updateUser(editing.value.id, {
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
    error.value = e instanceof Error ? e.message : 'Something went wrong.'
  } finally {
    busy.value = false
  }
}

async function remove(user: api.User) {
  if (!confirm(`Delete "${user.username}"?`)) return
  await api.deleteUser(user.id)
  if (editing.value?.id === user.id) cancelEdit()
  await load()
}

function flash(msg: string) {
  success.value = msg
  setTimeout(() => (success.value = ''), 3000)
}

onMounted(load)
</script>

<template>
  <div class="panel">
    <!-- Form -->
    <aside class="form-col">
      <h2 class="col-title">{{ editing ? 'Edit User' : 'New User' }}</h2>

      <form class="form" @submit.prevent="submit">
        <div class="field">
          <label>Username</label>
          <input v-model="form.username" type="text" placeholder="username" autocomplete="off" />
        </div>

        <div class="field">
          <label>Password</label>
          <input v-model="form.password" type="password" :placeholder="editing ? 'leave blank to keep existing' : 'password'" autocomplete="new-password" />
        </div>

        <div class="field">
          <label>Description <span class="optional">optional</span></label>
          <input v-model="form.description" type="text" placeholder="short bio" />
        </div>

        <p v-if="error" class="msg error">{{ error }}</p>
        <p v-if="success" class="msg success">{{ success }}</p>

        <div class="form-actions">
          <button type="submit" class="btn-primary" :disabled="busy">
            {{ editing ? 'Update' : 'Create' }}
          </button>
          <button v-if="editing" type="button" class="btn-ghost" @click="cancelEdit">Cancel</button>
        </div>
      </form>
    </aside>

    <!-- List -->
    <section class="list-col">
      <h2 class="col-title">Users <span class="count">{{ users.length }}</span></h2>

      <div class="list">
        <div v-if="users.length === 0" class="empty">No users yet.</div>

        <div
          v-for="user in users"
          :key="user.id"
          :class="['list-item', { selected: editing?.id === user.id }]"
        >
          <div class="item-body">
            <span class="item-name">{{ user.username }}</span>
            <span class="item-sub">{{ user.description ?? '—' }}</span>
          </div>
          <div class="item-meta">
            <span class="badge">ID {{ user.id }}</span>
          </div>
          <div class="item-actions">
            <button class="btn-sm" @click="startEdit(user)">Edit</button>
            <button class="btn-sm danger" @click="remove(user)">Delete</button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.panel {
  display: flex;
  height: 100%;
}

.form-col {
  width: 360px;
  flex-shrink: 0;
  padding: 1.5rem;
  border-right: 1px solid var(--color-border);
  overflow-y: auto;
}

.list-col {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}

.col-title {
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--color-text);
  opacity: 0.6;
  margin-bottom: 1.25rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.count {
  background: var(--color-background-mute);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 0 0.45rem;
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0;
}

/* Form */
.form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.field label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-heading);
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.optional {
  font-weight: 400;
  opacity: 0.5;
  font-size: 0.75rem;
}

.field input,
.field textarea,
.field select {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-background);
  color: var(--color-text);
  font-size: 0.875rem;
  transition: border-color 0.15s;
  font-family: inherit;
}

.field input:focus,
.field textarea:focus,
.field select:focus {
  outline: none;
  border-color: var(--color-border-hover);
}

.field select option {
  background: var(--color-background);
  color: var(--color-text);
}

.msg {
  font-size: 0.8rem;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
}

.error  { background: rgba(239,68,68,.12);  color: #ef4444; }
.success { background: rgba(34,197,94,.12); color: #16a34a; }

.form-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.25rem;
}

/* Buttons */
.btn-primary {
  padding: 0.5rem 1.25rem;
  background: #42b883;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
}
.btn-primary:hover { opacity: 0.88; }
.btn-primary:disabled { opacity: 0.45; cursor: not-allowed; }

.btn-ghost {
  padding: 0.5rem 1rem;
  background: none;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  font-size: 0.875rem;
  color: var(--color-text);
  cursor: pointer;
  transition: background 0.15s;
}
.btn-ghost:hover { background: var(--color-background-mute); }

/* List */
.list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.empty {
  color: var(--color-text);
  opacity: 0.4;
  font-size: 0.875rem;
  padding: 1rem 0;
}

.list-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-background-soft);
  transition: border-color 0.15s;
}

.list-item.selected {
  border-color: #42b883;
}

.item-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}

.item-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--color-heading);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-sub {
  font-size: 0.78rem;
  opacity: 0.6;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-meta {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}

.badge {
  font-size: 0.7rem;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  background: var(--color-background-mute);
  border: 1px solid var(--color-border);
  color: var(--color-text);
  opacity: 0.7;
  white-space: nowrap;
}

.item-actions {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}

.btn-sm {
  padding: 0.3rem 0.7rem;
  font-size: 0.775rem;
  border: 1px solid var(--color-border);
  border-radius: 5px;
  background: none;
  color: var(--color-text);
  cursor: pointer;
  transition: background 0.15s;
}
.btn-sm:hover { background: var(--color-background-mute); }
.btn-sm.danger { color: #ef4444; border-color: rgba(239,68,68,.3); }
.btn-sm.danger:hover { background: rgba(239,68,68,.08); }
</style>
