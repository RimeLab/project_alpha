'use client'
import { useState } from 'react'
import UserPanel from '@/components/UserPanel'
import CoffeePanel from '@/components/CoffeePanel'
import styles from './page.module.css'

export default function Home() {
  const [tab, setTab] = useState<'users' | 'coffees'>('users')

  return (
    <div className={styles.app}>
      <header className={styles.header}>
        <span className={styles.logo}>☕ AlphaApi</span>
        <nav className={styles.tabs}>
          <button
            className={`${styles.tab}${tab === 'users' ? ' ' + styles.active : ''}`}
            onClick={() => setTab('users')}
          >
            Users
          </button>
          <button
            className={`${styles.tab}${tab === 'coffees' ? ' ' + styles.active : ''}`}
            onClick={() => setTab('coffees')}
          >
            Coffees
          </button>
        </nav>
      </header>
      <main className={styles.main}>
        {tab === 'users' ? <UserPanel /> : <CoffeePanel />}
      </main>
    </div>
  )
}
