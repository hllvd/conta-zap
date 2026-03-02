import { useEffect, useState } from 'react'
import { getBots, createBot, toggleBot } from '../services/api'

const BOT_TYPES = ['PersonalFinance', 'Mei', 'Agro']

export default function Bots() {
  const [bots, setBots] = useState([])
  const [showModal, setShowModal] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    botNumber: '', botName: '', botType: 'PersonalFinance',
    personalityPrompt: '', setupMessage: '', sheetTemplateId: ''
  })

  const load = () => getBots().then(setBots)

  useEffect(() => { load() }, [])

  const handleCreate = async e => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      await createBot(form)
      setShowModal(false)
      setForm({ botNumber: '', botName: '', botType: 'PersonalFinance', personalityPrompt: '', setupMessage: '', sheetTemplateId: '' })
      load()
    } catch (err) {
      setError(err.response?.data?.error || 'Erro ao criar bot.')
    } finally {
      setLoading(false)
    }
  }

  const handleToggle = async id => {
    await toggleBot(id)
    load()
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Bots</h1>
          <p className="page-sub">Gerencie os bots WhatsApp da plataforma</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>＋ Novo Bot</button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '16px' }}>
        {bots.length === 0 && (
          <div className="card" style={{ textAlign: 'center', color: 'var(--muted)', padding: '40px' }}>
            Nenhum bot cadastrado.
          </div>
        )}
        {bots.map(b => (
          <div className="card" key={b.id}>
            <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: '12px' }}>
              <div>
                <div style={{ fontWeight: 600, fontSize: '1rem' }}>{b.botName}</div>
                <div style={{ color: 'var(--muted)', fontSize: '0.82rem' }}>{b.botNumber}</div>
              </div>
              <span className={`badge ${b.isActive ? 'badge-green' : 'badge-red'}`}>
                {b.isActive ? 'Ativo' : 'Inativo'}
              </span>
            </div>
            <div style={{ marginBottom: '12px' }}>
              <span className="badge badge-purple">{b.botType}</span>
            </div>
            <div style={{ color: 'var(--muted)', fontSize: '0.82rem', marginBottom: '16px', lineHeight: '1.5' }}>
              {b.personalityPrompt?.slice(0, 90)}…
            </div>
            <button
              className={`btn btn-sm ${b.isActive ? 'btn-danger' : 'btn-primary'}`}
              onClick={() => handleToggle(b.id)}
            >
              {b.isActive ? 'Desativar' : 'Ativar'}
            </button>
          </div>
        ))}
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()} style={{ width: '520px' }}>
            <div className="modal-title">Novo Bot</div>
            <form onSubmit={handleCreate}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
                <div className="form-group">
                  <label>Nome do Bot</label>
                  <input value={form.botName} onChange={e => setForm(f => ({ ...f, botName: e.target.value }))} required placeholder="FinBot" />
                </div>
                <div className="form-group">
                  <label>Número WhatsApp</label>
                  <input value={form.botNumber} onChange={e => setForm(f => ({ ...f, botNumber: e.target.value }))} required placeholder="+5511999990001" />
                </div>
              </div>
              <div className="form-group">
                <label>Tipo de Bot</label>
                <select value={form.botType} onChange={e => setForm(f => ({ ...f, botType: e.target.value }))}>
                  {BOT_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>ID do Template Google Sheets</label>
                <input value={form.sheetTemplateId} onChange={e => setForm(f => ({ ...f, sheetTemplateId: e.target.value }))} required placeholder="1A2B3C4D..." />
              </div>
              <div className="form-group">
                <label>Mensagem de Boas-Vindas</label>
                <textarea rows={2} value={form.setupMessage} onChange={e => setForm(f => ({ ...f, setupMessage: e.target.value }))} required placeholder="Olá! Sou o FinBot..." />
              </div>
              <div className="form-group">
                <label>Prompt de Personalidade</label>
                <textarea rows={3} value={form.personalityPrompt} onChange={e => setForm(f => ({ ...f, personalityPrompt: e.target.value }))} required placeholder="Você é um assistente financeiro..." />
              </div>
              {error && <p className="error">{error}</p>}
              <div className="modal-actions">
                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Criando...' : 'Criar Bot'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
