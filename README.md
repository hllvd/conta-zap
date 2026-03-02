# ⚡ ContaZap

Multi-tenant WhatsApp bot platform — .NET 9 API · React · SQLite · Docker.

---

## Run with Docker (recommended)

```bash
# 1. Add your Google service account (even a placeholder to start)
mkdir -p secrets
echo '{}' > secrets/service-account.json

# 2. Build and start everything
docker compose up --build
```

| Service | URL |
|---|---|
| Admin panel | http://localhost:3000 |
| API | http://localhost:8080 |
| Health check | http://localhost:8080/health |

**Default login:** `superadmin@test.com` / `string`

---

## Run locally (development)

**Requirements:** .NET 9 SDK · Node.js 20+

```bash
# Terminal 1 — API
cd api/Api
dotnet run

# Terminal 2 — Client
cd client
npm run dev
```

---

## Project structure

```
conta-zap/
├── api/            .NET 9 Minimal API
│   ├── Api/        Endpoints + Program.cs
│   ├── Application/ Services, DTOs, Interfaces
│   ├── Domain/     Entities (User, Bot, Message), Enums
│   └── Infrastructure/ EF Core, Repositories, Google Sheets, Bot Engine
├── client/         React + Vite admin panel
├── tests/
│   └── Integration/ xUnit + WebApplicationFactory
├── config.json     All runtime config (JWT, DB, Sheets, bots)
└── docker-compose.yml
```

---

## Architecture overview

```
WhatsApp Provider → POST /api/webhook/{botNumber}
                         │
                    BotStrategyFactory
                         │
          ┌──────────────┼──────────────┐
   FinanceBot         MeiBot         AgroBot
          │
    Google Sheets ← reads/writes user's personal spreadsheet

Admin Browser → React SPA → REST API (JWT) → EF Core → SQLite
```

- **Strategy Pattern** — each bot type is a separate `IBotStrategy` implementation; add new bots without changing existing code.  
- **Auto-provisioning** — creating a customer automatically copies a Google Sheets template and links it to the user.  
- **SQLite → PostgreSQL** — swap `Database:Provider` in `config.json` and change the EF provider; no other code changes needed.

---

## Adding a new bot type

1. Add value to `Domain/Enums/BotType.cs`
2. Create `api/Infrastructure/BotEngine/MyBotStrategy.cs` implementing `IBotStrategy`
3. Register in `api/Api/Program.cs`: `builder.Services.AddScoped<IBotStrategy, MyBotStrategy>()`

---

## .NET 10 upgrade

When .NET 10 GA ships (Nov 2026): change `net9.0` → `net10.0` in all `.csproj` files and `sdk:9.0` / `aspnet:9.0` → `10.0` in `api/Dockerfile`.
