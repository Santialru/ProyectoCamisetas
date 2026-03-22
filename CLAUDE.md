# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run in development
dotnet run --launch-profile http       # http://localhost:5295
dotnet run --launch-profile https      # https://localhost:7224

# Build
dotnet build

# Restore dependencies
dotnet restore

# Entity Framework migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

No test projects exist in this solution.

Swagger UI is available at `/swagger` when running in development mode.

## Architecture

**ProyectoCamisetas** is an ASP.NET Core 9.0 MVC application for managing and selling sports jerseys. It uses PostgreSQL (hosted on Supabase) via Entity Framework Core with snake_case naming conventions.

### Key Layers

- **Controllers/** — MVC controllers (`HomeController`, `CamisetasController`, `AdminCamisetasController`, `UserController`) and a REST API under `Controllers/Api/CamisetasApiController.cs`
- **Repository/** — Repository pattern: interfaces (`IUserRepository`, `ICamisetasRepository`) with EF implementations (`EfUserRepository`, `EfCamisetasRepository`)
- **Data/** — `AppDbContext.cs` + `Migrations/` folder (11 migrations, code-first)
- **Models/** — Domain entities (`Camiseta`, `User`, `Venta`, etc.) and view-specific enums (`Talla`, `TipoKit`, `Manga`, `VersionCamiseta`)
- **Views/** — Razor templates; `Views/AdminCamisetas/` for owner-only management, `Views/Camisetas/` for public catalog

### Authentication & Authorization

- Cookie-based auth with cookie name `cdg_auth`, 8-hour sliding expiration
- Single role: `Owner` — enforced via `[Authorize(Roles = "Owner")]` on `AdminCamisetasController`
- PBKDF2 password hashing (100,000 iterations, SHA-256) with 128-bit salt
- Login lockout: 5 failed attempts → 15-minute lockout (tracked in `User` model)
- CSRF protection enabled globally

### Database

- PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`, connection string in `appsettings.json`
- Snake_case column naming via `EFCore.NamingConventions`
- On startup: `db.Database.Migrate()` runs automatically, and an Owner user is seeded from env vars `OWNER_USER`, `OWNER_EMAIL`, `OWNER_PASSWORD`
- `Ventas` (sales) uses denormalized fields (product name, team, season) so sales records remain independent of product changes

### Uploads

- Product images saved to `wwwroot/uploads/`
- In development, `Uploads:EnableDevMirror: true` + `Uploads:RemoteBaseUrl` mirror images from the production site when local files are missing

### Middleware Pipeline Order

Static Files → Routing → Anti-Cache headers (HTML) → Authentication → Authorization → MVC Routes
