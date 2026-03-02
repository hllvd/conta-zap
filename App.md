# ContaZap — What is this app?

ContaZap is a **SaaS platform** that lets businesses deploy branded WhatsApp bots for their customers. Each bot has its own personality, niche, and WhatsApp number. Each customer gets their own Google Spreadsheet automatically created when they sign up.

---

## Business model

| Role | Who | What they do |
|---|---|---|
| **Super Admin** | Company owner / operator | Creates bots and customers via the admin panel |
| **Customer** | End user | Talks to the bot via WhatsApp — never sees the admin panel |

---

## How it works (end-to-end)

### 1. Admin sets up a bot
The admin registers a bot number (+55 11 9999-0001), its personality prompt, welcome message, and a Google Sheets template ID. The bot becomes available to assign to customers.

### 2. Admin creates a customer
When a customer is created:
- Their WhatsApp number is registered
- A copy of the Google Sheets template is automatically created in Google Drive
- The sheet is renamed (`finance-joaosilva-20260301`) and shared with the customer's email
- The sheet ID is stored in the database

### 3. Customer talks to the bot
The customer sends a WhatsApp message to the bot number. The WhatsApp provider (Evolution API, Z-API, Twilio, etc.) delivers it to:

```
POST /api/webhook/{botNumber}
```

The system:
1. Identifies the bot and the customer by their WhatsApp number
2. Stores the incoming message
3. Routes to the correct bot strategy (Finance, MEI, Agro…)
4. The strategy reads/writes the customer's Google Sheet
5. Returns a reply that the WhatsApp provider delivers back

### 4. Admin monitors & tests
The admin panel lets the operator view all customers, see their linked spreadsheets, activate/deactivate bots, and **simulate conversations** without WhatsApp (`/api/test/chat`).

---

## Bot types implemented

### 💰 Personal Finance Bot (`PersonalFinance`)
Helps individuals track income and expenses via natural Portuguese language.

**Commands understood:**
| Example message | What happens |
|---|---|
| "gastei R$50 em alimentação" | Registers a R$50 expense in the Alimentação category |
| "recebi R$5000 de salário" | Registers R$5000 income |
| "quanto gastei em março?" | Reads the sheet and sums March expenses |
| "resumo do mês" | Returns total income, expenses, and balance |
| "ajuda" | Shows available commands |

The bot extracts amounts via regex and categories via keyword matching (Portuguese-aware).

### 🏢 MEI Bot (`Mei`)
Answers common questions for Microempreendedores Individuais: DAS due dates, invoice types, revenue limits, employee rules.

### 🌾 Agro Bot (`Agro`)
Answers agronegócio questions: weather/climate resources, commodity price links, rural credit (PRONAF), planting season calendars.

---

## Google Sheets structure (Finance bot)

Each customer gets a spreadsheet with 3 tabs:

| Tab | Purpose |
|---|---|
| **Transações** | One row per transaction: date, type (receita/despesa), amount, category, description |
| **Resumo** | Formula-driven monthly totals by category |
| **Config** | Customer metadata (name, WhatsApp, bot ID) |

---

## What's not yet implemented (future work)

- LLM integration (GPT / Gemini) for richer, context-aware responses
- Refresh token persistence (currently in-memory only)
- Playwright E2E tests (`tests/E2E/` folder exists but is empty)
- Multi-language support
- Customer self-service portal
- Webhooks for outbound notifications (e.g. monthly summary push)
