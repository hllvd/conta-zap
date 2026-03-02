# 📜 Development History: ContaZap

This document summarizes the archival steps, architecture decisions, and implementation milestones of the ContaZap platform.

---

## 🏗️ Architecture Overview

The system follows **Clean Architecture** principles and the **Strategy Pattern** to ensure scalability and maintainability.

### 1. Layers
- **Domain**: Pure business entities (`User`, `Bot`, `Message`) and enums. No dependencies.
- **Application**: Business logic orchestrators (`UserService`, `BotService`). Defines interfaces for infrastructure services.
- **Infrastructure**: Concrete implementations (EF Core, Google Sheets, Bot Strategies).
- **Api**: Entry point. Minimal API endpoints, JWT configuration, and dependency injection wiring.

### 2. The Bot Engine (Strategy Pattern)
This is the core of the multi-bot capability. 
- Instead of a massive `if/else` block, each niche (Finance, MEI, Agro) implements `IBotStrategy`.
- A `BotStrategyFactory` resolves the correct logic based on the bot type at runtime.
- **Benefit**: You can add 100 new bot types by just adding new classes without touching the core webhook logic.

### 3. Google Sheets Integration
- **Auto-provisioning**: When a user is created, the system clones a template sheet and shares it with the customer.
- **Lazy Initialization**: The system is resilient. If Google credentials are missing or invalid, the app **stays up**. Only the Sheets-related features will fail gracefully with a log warning.

---

## ⏳ Implementation Milestones

### Phase 1: Design & Planning
- Defined the multi-tenant model (one bot per number, many customers per bot).
- Designed the relational schema with SQLite (ready for PostgreSQL).
- Drafted the Strategy Pattern for bot extensibility.

### Phase 2: Backend Scaffolding
- Built the 4-layer .NET solution.
- Implemented **JWT Authentication** and role-based policies (AdminOnly).
- Created the **Webhook Pipeline**: Secret validation → User identification → Message storage → Strategy execution → Response.
- Added **Simulation Endpoints**: Allowed chatting with bots via the API without requiring a real WhatsApp hook.

### Phase 3: Frontend (Admin Panel)
- Built a modern React SPA using **Vite**.
- Implemented a custom **Design System** (Sidebar, Stats Cards, Chat Bubbles) without external UI libraries for maximum performance.
- Built-in **Chat Simulator** in the admin panel to speed up bot development.

### Phase 4: Containerization & DevOps
- Created multi-stage **Dockerfiles** for API and Client.
- Set up **Docker Compose** with persistent volumes for SQLite.
- Fixed the "EF Migration Gap": Generated proper migrations and implemented an auto-migration startup sequence.

---

## 💡 Important Design Decisions

- **Minimal API**: Swapped traditional Controllers for Minimal API for lower overhead and faster startup.
- **Design-Time Factory**: Added `IDesignTimeDbContextFactory` to allow database migrations to be generated without the full runtime configuration.
- **Type Aliasing**: Resolved name conflicts between `Google.Apis.Drive.v3.Data.User` and our `Domain.Entities.User` via selective type aliasing.
- **Resilient Seeding**: Implemented a seeder that ensures a Super Admin exists on every startup without duplicating entries.

---

## 🛠️ How it "Thinks"

When a message arrives at `/api/webhook/{botNumber}`:
1. **Security**: Checks the `X-Webhook-Secret`.
2. **Context**: Looks up which Bot is configured for that number.
3. **Identity**: Finds the customer by their WhatsApp number. If new, it auto-registers them.
4. **Strategy**: Asks the `BotStrategyFactory` for the logic assigned to that bot type.
5. **Execution**: The strategy processes the message (e.g., "Gastei 50"), writes to the user's Google Sheet, and returns a human-friendly reply.
6. **Persistence**: Every interaction is stored for history.
