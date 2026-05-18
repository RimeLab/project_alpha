<script setup lang="ts">
import { ref } from 'vue'
import UserPanel from '@/components/UserPanel.vue'
import CoffeePanel from '@/components/CoffeePanel.vue'

const tab = ref<'users' | 'coffees'>('users')
</script>

<template>
  <div class="app">
    <header class="header">
      <span class="logo">☕ AlphaApi</span>
      <nav class="tabs">
        <button :class="['tab', { active: tab === 'users' }]" @click="tab = 'users'">Users</button>
        <button :class="['tab', { active: tab === 'coffees' }]" @click="tab = 'coffees'">Coffees</button>
      </nav>
    </header>

    <main class="main">
      <UserPanel v-if="tab === 'users'" />
      <CoffeePanel v-else />
    </main>
  </div>
</template>

<style scoped>
.app {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.header {
  display: flex;
  align-items: center;
  gap: 2rem;
  padding: 0 1.5rem;
  height: 56px;
  background: var(--color-background-soft);
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
}

.logo {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-heading);
}

.tabs {
  display: flex;
  gap: 0.25rem;
}

.tab {
  padding: 0.35rem 1rem;
  border: 1px solid transparent;
  border-radius: 6px;
  background: none;
  color: var(--color-text);
  cursor: pointer;
  font-size: 0.875rem;
  transition: all 0.15s;
}

.tab:hover {
  background: var(--color-background-mute);
}

.tab.active {
  background: var(--color-background-mute);
  border-color: var(--color-border-hover);
  font-weight: 600;
  color: var(--color-heading);
}

.main {
  flex: 1;
  overflow: hidden;
}
</style>
