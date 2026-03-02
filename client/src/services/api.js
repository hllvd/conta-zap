import axios from 'axios'

const api = axios.create({ baseURL: '/api' })

// Attach JWT to every request
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Auto-redirect on 401
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

// ── Auth ────────────────────────────────────────────────────────────
export const login = (email, password) =>
  api.post('/auth/login', { email, password }).then(r => r.data)

export const getMe = () =>
  api.get('/auth/me').then(r => r.data)

// ── Users ───────────────────────────────────────────────────────────
export const getUsers = () =>
  api.get('/users').then(r => r.data)

export const getUser = id =>
  api.get(`/users/${id}`).then(r => r.data)

export const createUser = data =>
  api.post('/users', data).then(r => r.data)

export const updateUser = (id, data) =>
  api.put(`/users/${id}`, data).then(r => r.data)

export const deleteUser = id =>
  api.delete(`/users/${id}`)

// ── Bots ────────────────────────────────────────────────────────────
export const getBots = () =>
  api.get('/bots').then(r => r.data)

export const createBot = data =>
  api.post('/bots', data).then(r => r.data)

export const toggleBot = id =>
  api.patch(`/bots/${id}/toggle`).then(r => r.data)

// ── Chat Test ───────────────────────────────────────────────────────
export const testChat = (userWhatsApp, botNumber, message) =>
  api.post('/test/chat', { userWhatsApp, botNumber, message }).then(r => r.data)

export const getChatHistory = whatsApp =>
  api.get(`/test/history/${encodeURIComponent(whatsApp)}`).then(r => r.data)

export default api
