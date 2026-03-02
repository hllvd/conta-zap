import { useEffect, useState } from 'react'
import { getUsers, getBots, createUser, deleteUser } from '../services/api'

export default function Users() {
  const [users, setUsers] = useState([])
  const [bots, setBots] = useState([])
  const [showModal, setShowModal] = useState(false)
  const [loading, setLoading] = useState(false)
  const [form, setForm] = useState({ name: '', email: '', password: '', whatsAppNumber: '', botId: '' })
  const [error, setError] = useState('')

  const load = async () => {
    const [u, b] = await Promise.all([getUsers(), getBots()])
    setUsers(u)
    setBots(b.filter(b => b.isActive))
    if (b.length > 0) setForm(f => ({ ...f, botId: b[0].id }))
  }

  useEffect(() => { load() }, [])

  const handleCreate = async e => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      await createUser(form)
      setShowModal(false)
      setForm({ name: '', email: '', password: '', whatsAppNumber: '', botId: bots[0]?.id || '' })
      load()
    } catch (err) {
      setError(err.response?.data?.error || 'Erro ao criar cliente.')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async id => {
    if (!confirm('Deseja remover este cliente?')) return
    await deleteUser(id)
    load()
  }

  const botName = id => bots.find(b => b.id === id)?.botName || id

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Clientes</h1>
          <p className="page-sub">Gerencie os clientes da plataforma</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          ＋ Novo Cliente
        </button>
      </div>

      <div className="card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Nome</th><th>Email</th><th>WhatsApp</th>
                <th>Bot</th><th>Planilha</th><th>Criado em</th><th></th>
              </tr>
            </thead>
            <tbody>
              {users.length === 0 && (
                <tr><td colSpan="7" style={{ textAlign: 'center', color: 'var(--muted)', padding: '32px' }}>Nenhum cliente cadastrado.</td></tr>
              )}
              {users.map(u => (
                <tr key={u.id}>
                  <td>{u.name}</td>
                  <td style={{ color: 'var(--muted)' }}>{u.email}</td>
                  <td>{u.whatsAppNumber}</td>
                  <td><span className="badge badge-purple">{u.botType}</span></td>
                  <td>
                    {u.googleSheetId
                      ? <a href={`https://docs.google.com/spreadsheets/d/${u.googleSheetId}`} target="_blank" rel="noreferrer" style={{ color: 'var(--accent-light)' }}>📊 Abrir</a>
                      : <span className="muted">—</span>}
                  </td>
                  <td style={{ color: 'var(--muted)' }}>{new Date(u.createdAt).toLocaleDateString('pt-BR')}</td>
                  <td>
                    <button className="btn btn-danger btn-sm" onClick={() => handleDelete(u.id)}>Remover</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-title">Novo Cliente</div>
            <form onSubmit={handleCreate}>
              <div className="form-group">
                <label>Nome</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required placeholder="João Silva" />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input type="email" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} required placeholder="joao@email.com" />
              </div>
              <div className="form-group">
                <label>Senha inicial</label>
                <input type="password" value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} required placeholder="Senha temporária" />
              </div>
              <div className="form-group">
                <label>WhatsApp (com DDI)</label>
                <input value={form.whatsAppNumber} onChange={e => setForm(f => ({ ...f, whatsAppNumber: e.target.value }))} required placeholder="+5511988887777" />
              </div>
              <div className="form-group">
                <label>Bot</label>
                <select value={form.botId} onChange={e => setForm(f => ({ ...f, botId: e.target.value }))}>
                  {bots.map(b => <option key={b.id} value={b.id}>{b.botName} ({b.botNumber})</option>)}
                </select>
              </div>
              {error && <p className="error">{error}</p>}
              <div className="modal-actions">
                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Criando...' : 'Criar Cliente'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
