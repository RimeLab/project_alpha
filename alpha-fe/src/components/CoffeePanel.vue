<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import * as api from '@/api'

const coffees = ref<api.Coffee[]>([])
const users   = ref<api.User[]>([])
const editing = ref<api.Coffee | null>(null)
const busy    = ref(false)
const error   = ref('')
const success = ref('')

const form = reactive({
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
  ;[coffees.value, users.value] = await Promise.all([api.getCoffees(), api.getUsers()])
  if (!form.userId && users.value.length) form.userId = users.value[0]!.id
}

function startEdit(coffee: api.Coffee) {
  editing.value = coffee
  form.type        = coffee.type
  form.shop        = coffee.shop
  form.location    = coffee.location
  form.intensity   = coffee.intensity
  form.rating      = coffee.rating
  form.temperature = coffee.temperature
  form.notes       = coffee.notes ?? ''
  form.userId      = coffee.userId
  error.value      = ''
}

function cancelEdit() {
  editing.value = null
  form.type        = ''
  form.shop        = ''
  form.location    = ''
  form.intensity   = 5
  form.rating      = 3
  form.temperature = 'Hot'
  form.notes       = ''
  form.userId      = users.value[0]?.id ?? 0
  error.value      = ''
}

async function submit() {
  if (!form.type.trim())     { error.value = 'Type is required.'; return }
  if (!form.shop.trim())     { error.value = 'Shop is required.'; return }
  if (!form.location.trim()) { error.value = 'Location is required.'; return }
  if (!form.userId)          { error.value = 'User is required.'; return }
  busy.value  = true
  error.value = ''
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
    if (editing.value) {
      await api.updateCoffee(editing.value.id, data)
      flash('Coffee updated.')
    } else {
      await api.createCoffee(data)
      flash('Coffee logged.')
    }
    cancelEdit()
    await load()
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Something went wrong.'
  } finally {
    busy.value = false
  }
}

async function remove(coffee: api.Coffee) {
  if (!confirm(`Delete "${coffee.type}" at ${coffee.shop}?`)) return
  await api.deleteCoffee(coffee.id)
  if (editing.value?.id === coffee.id) cancelEdit()
  await load()
}

function flash(msg: string) {
  success.value = msg
  setTimeout(() => (success.value = ''), 3000)
}

function stars(n: number) { return '★'.repeat(n) + '☆'.repeat(5 - n) }
function dots(n: number)  { return '●'.repeat(n) + '○'.repeat(10 - n) }

function userName(id: number) {
  return users.value.find(u => u.id === id)?.username ?? `#${id}`
}

onMounted(load)
</script>

<template>
  <div class="panel">
    <!-- Form -->
    <aside class="form-col">
      <h2 class="col-title">{{ editing ? 'Edit Coffee' : 'Log Coffee' }}</h2>

      <form class="form" @submit.prevent="submit">
        <div class="field">
          <label>Type</label>
          <input v-model="form.type" type="text" placeholder="Espresso, Latte, Cold Brew…" />
        </div>

        <div class="field">
          <label>Shop</label>
          <input v-model="form.shop" type="text" placeholder="Blue Bottle" />
        </div>

        <div class="field">
          <label>Location</label>
          <input v-model="form.location" type="text" placeholder="San Francisco, CA" />
        </div>

        <div class="row-fields">
          <div class="field">
            <label>Intensity <span class="optional">1–10</span></label>
            <input v-model.number="form.intensity" type="number" min="1" max="10" />
          </div>
          <div class="field">
            <label>Rating <span class="optional">1–5</span></label>
            <input v-model.number="form.rating" type="number" min="1" max="5" />
          </div>
        </div>

        <div class="field">
          <label>Temperature</label>
          <select v-model="form.temperature">
            <option>Hot</option>
            <option>Warm</option>
            <option>Cold</option>
            <option>Iced</option>
          </select>
        </div>

        <div class="field">
          <label>User</label>
          <select v-model.number="form.userId">
            <option v-for="u in users" :key="u.id" :value="u.id">{{ u.username }}</option>
          </select>
        </div>

        <div class="field">
          <label>Notes <span class="optional">optional</span></label>
          <textarea v-model="form.notes" rows="3" placeholder="Tasting notes, impressions…" />
        </div>

        <p v-if="error" class="msg error">{{ error }}</p>
        <p v-if="success" class="msg success">{{ success }}</p>

        <div class="form-actions">
          <button type="submit" class="btn-primary" :disabled="busy">
            {{ editing ? 'Update' : 'Log' }}
          </button>
          <button v-if="editing" type="button" class="btn-ghost" @click="cancelEdit">Cancel</button>
        </div>
      </form>
    </aside>

    <!-- List -->
    <section class="list-col">
      <h2 class="col-title">Coffees <span class="count">{{ coffees.length }}</span></h2>

      <div class="list">
        <div v-if="coffees.length === 0" class="empty">No coffees logged yet.</div>

        <div
          v-for="coffee in coffees"
          :key="coffee.id"
          :class="['list-item', { selected: editing?.id === coffee.id }]"
        >
          <div class="item-body">
            <div class="item-top">
              <span class="item-name">{{ coffee.type }}</span>
              <span class="item-shop">{{ coffee.shop }}</span>
            </div>
            <span class="item-sub">{{ coffee.location }} · {{ userName(coffee.userId) }}</span>
            <span v-if="coffee.notes" class="item-notes">{{ coffee.notes }}</span>
          </div>

          <div class="item-meta">
            <span class="badge temp" :class="coffee.temperature.toLowerCase()">{{ coffee.temperature }}</span>
            <span class="badge" title="Intensity">{{ dots(coffee.intensity) }}</span>
            <span class="badge stars" title="Rating">{{ stars(coffee.rating) }}</span>
          </div>

          <div class="item-actions">
            <button class="btn-sm" @click="startEdit(coffee)">Edit</button>
            <button class="btn-sm danger" @click="remove(coffee)">Delete</button>
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
  gap: 0.9rem;
}

.row-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
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
  resize: vertical;
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

.error   { background: rgba(239,68,68,.12);  color: #ef4444; }
.success { background: rgba(34,197,94,.12);  color: #16a34a; }

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
.btn-primary:hover    { opacity: 0.88; }
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
  align-items: flex-start;
  gap: 1rem;
  padding: 0.85rem 1rem;
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

.item-top {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
}

.item-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--color-heading);
}

.item-shop {
  font-size: 0.8rem;
  opacity: 0.7;
}

.item-sub {
  font-size: 0.775rem;
  opacity: 0.55;
}

.item-notes {
  font-size: 0.775rem;
  opacity: 0.65;
  font-style: italic;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-meta {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  align-items: flex-end;
  flex-shrink: 0;
}

.badge {
  font-size: 0.68rem;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  background: var(--color-background-mute);
  border: 1px solid var(--color-border);
  color: var(--color-text);
  opacity: 0.75;
  white-space: nowrap;
  font-family: monospace;
}

.badge.stars {
  color: #f59e0b;
  opacity: 1;
  border-color: rgba(245,158,11,.3);
  background: rgba(245,158,11,.08);
}

.badge.temp.hot  { color: #ef4444; border-color: rgba(239,68,68,.3);  background: rgba(239,68,68,.07);  }
.badge.temp.warm { color: #f97316; border-color: rgba(249,115,22,.3); background: rgba(249,115,22,.07); }
.badge.temp.cold { color: #3b82f6; border-color: rgba(59,130,246,.3); background: rgba(59,130,246,.07); }
.badge.temp.iced { color: #06b6d4; border-color: rgba(6,182,212,.3);  background: rgba(6,182,212,.07);  }

.item-actions {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
  align-self: center;
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
.btn-sm:hover        { background: var(--color-background-mute); }
.btn-sm.danger       { color: #ef4444; border-color: rgba(239,68,68,.3); }
.btn-sm.danger:hover { background: rgba(239,68,68,.08); }
</style>
