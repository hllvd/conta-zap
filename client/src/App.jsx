import { BrowserRouter, Routes, Route, NavLink, Navigate, useNavigate } from 'react-router-dom'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Users from './pages/Users'
import Bots from './pages/Bots'
import ChatTest from './pages/ChatTest'
import './index.css'

const isAuth = () => !!localStorage.getItem('token')

function ProtectedRoute({ children }) {
  return isAuth() ? children : <Navigate to="/login" replace />
}

function Sidebar() {
  const navigate = useNavigate()
  const logout = () => { localStorage.removeItem('token'); navigate('/login') }

  const navItems = [
    { to: '/dashboard', icon: '🏠', label: 'Dashboard' },
    { to: '/users', icon: '👥', label: 'Clientes' },
    { to: '/bots', icon: '🤖', label: 'Bots' },
    { to: '/chat', icon: '💬', label: 'Testar Bot' },
  ]

  return (
    <aside className="sidebar">
      <div className="sidebar-logo"><span>⚡</span> ContaZap</div>
      {navItems.map(item => (
        <NavLink key={item.to} to={item.to} className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
          {item.icon} {item.label}
        </NavLink>
      ))}
      <button className="nav-item" style={{ marginTop: 'auto' }} onClick={logout}>
        🚪 Sair
      </button>
    </aside>
  )
}

function AppLayout({ children }) {
  return (
    <div className="layout">
      <Sidebar />
      <main className="main">{children}</main>
    </div>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<ProtectedRoute><AppLayout><Dashboard /></AppLayout></ProtectedRoute>} />
        <Route path="/users" element={<ProtectedRoute><AppLayout><Users /></AppLayout></ProtectedRoute>} />
        <Route path="/bots" element={<ProtectedRoute><AppLayout><Bots /></AppLayout></ProtectedRoute>} />
        <Route path="/chat" element={<ProtectedRoute><AppLayout><ChatTest /></AppLayout></ProtectedRoute>} />
        <Route path="*" element={<Navigate to={isAuth() ? '/dashboard' : '/login'} replace />} />
      </Routes>
    </BrowserRouter>
  )
}
