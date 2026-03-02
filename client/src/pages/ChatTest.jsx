import { useEffect, useRef, useState } from 'react'
import { getBots, getUsers, testChat, getChatHistory } from '../services/api'

export default function ChatTest() {
  const [bots, setBots] = useState([])
  const [users, setUsers] = useState([])
  const [selectedBot, setSelectedBot] = useState('')
  const [selectedUser, setSelectedUser] = useState('')
  const [messages, setMessages] = useState([])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const bottomRef = useRef(null)

  useEffect(() => {
    Promise.all([getBots(), getUsers()]).then(([b, u]) => {
      setBots(b.filter(x => x.isActive))
      setUsers(u)
      if (b.length > 0) setSelectedBot(b[0].botNumber)
      if (u.length > 0) setSelectedUser(u[0].whatsAppNumber)
    })
  }, [])

  useEffect(() => {
    if (selectedUser) {
      getChatHistory(selectedUser)
        .then(hist => setMessages(hist.map(m => ({ sender: m.sender, text: m.content }))))
        .catch(() => setMessages([]))
    }
  }, [selectedUser])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const send = async e => {
    e.preventDefault()
    if (!input.trim() || !selectedBot || !selectedUser) return
    const userMsg = input.trim()
    setMessages(m => [...m, { sender: 'user', text: userMsg }])
    setInput('')
    setLoading(true)
    try {
      const res = await testChat(selectedUser, selectedBot, userMsg)
      setMessages(m => [...m, { sender: 'bot', text: res.reply }])
    } catch {
      setMessages(m => [...m, { sender: 'bot', text: '❌ Erro ao processar mensagem.' }])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Testar Bot</h1>
          <p className="page-sub">Simule conversas sem precisar do WhatsApp</p>
        </div>
      </div>

      <div className="card" style={{ maxWidth: '680px' }}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '20px' }}>
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label>Cliente (usuário WhatsApp)</label>
            <select value={selectedUser} onChange={e => setSelectedUser(e.target.value)}>
              {users.map(u => <option key={u.id} value={u.whatsAppNumber}>{u.name} ({u.whatsAppNumber})</option>)}
            </select>
          </div>
          <div className="form-group" style={{ marginBottom: 0 }}>
            <label>Bot</label>
            <select value={selectedBot} onChange={e => setSelectedBot(e.target.value)}>
              {bots.map(b => <option key={b.id} value={b.botNumber}>{b.botName}</option>)}
            </select>
          </div>
        </div>

        <div className="chat-window">
          {messages.length === 0 && (
            <div style={{ margin: 'auto', color: 'var(--muted)', textAlign: 'center', fontSize: '0.88rem' }}>
              Nenhuma mensagem ainda. Comece a conversa abaixo 👇
            </div>
          )}
          {messages.map((m, i) => (
            <div key={i} className={`bubble bubble-${m.sender}`}>
              {m.text}
            </div>
          ))}
          {loading && <div className="bubble bubble-bot" style={{ opacity: 0.5 }}>Digitando…</div>}
          <div ref={bottomRef} />
        </div>

        <form className="chat-input-row" onSubmit={send}>
          <input
            value={input}
            onChange={e => setInput(e.target.value)}
            placeholder='Ex: "gastei R$50 em alimentação"'
            disabled={loading}
          />
          <button type="submit" className="btn btn-primary" disabled={loading || !input.trim()}>
            Enviar
          </button>
        </form>

        <p className="muted mt-2" style={{ fontSize: '0.78rem' }}>
          💡 Dica: Use frases naturais. Para FinBot: "gastei R$80 em transporte", "quanto gastei esse mês?", "resumo do mês"
        </p>
      </div>
    </div>
  )
}
