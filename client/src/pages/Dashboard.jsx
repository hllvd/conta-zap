import { useEffect, useState } from 'react'
import { getUsers, getBots } from '../services/api'

export default function Dashboard() {
  const [stats, setStats] = useState({ users: 0, bots: 0, activeBots: 0 })

  useEffect(() => {
    Promise.all([getUsers(), getBots()])
      .then(([users, bots]) => {
        setStats({
          users: users.length,
          bots: bots.length,
          activeBots: bots.filter(b => b.isActive).length
        })
      })
      .catch(() => {})
  }, [])

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-sub">Visão geral da plataforma ContaZap</p>
        </div>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-label">Clientes</div>
          <div className="stat-value" style={{ color: 'var(--accent-light)' }}>{stats.users}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🤖</div>
          <div className="stat-label">Bots Cadastrados</div>
          <div className="stat-value" style={{ color: 'var(--accent-light)' }}>{stats.bots}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">✅</div>
          <div className="stat-label">Bots Ativos</div>
          <div className="stat-value" style={{ color: 'var(--green)' }}>{stats.activeBots}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">⚡</div>
          <div className="stat-label">Versão API</div>
          <div className="stat-value" style={{ fontSize: '1.2rem', color: 'var(--muted)' }}>v1.0</div>
        </div>
      </div>

      <div className="card">
        <div className="card-title">🚀 Início Rápido</div>
        <ol style={{ paddingLeft: '20px', lineHeight: '2', color: 'var(--muted)', fontSize: '0.9rem' }}>
          <li>Vá em <strong style={{ color: 'var(--text)' }}>Bots</strong> e cadastre o número WhatsApp do bot</li>
          <li>Vá em <strong style={{ color: 'var(--text)' }}>Clientes</strong> e crie um cliente — a planilha Google é criada automaticamente</li>
          <li>Use <strong style={{ color: 'var(--text)' }}>Testar Bot</strong> para simular conversas sem precisar do WhatsApp</li>
          <li>Configure o webhook do seu provedor WhatsApp: <code style={{ background: 'var(--surface2)', padding: '2px 6px', borderRadius: '4px' }}>POST /api/webhook/:botNumber</code></li>
        </ol>
      </div>
    </div>
  )
}
