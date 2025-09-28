# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VHouse is a B2B distribution platform for vegan businesses built with Clean Architecture and CQRS. The system serves real clients (Mona la Dona, Sano Market, La Papelería) and aims to make vegan distribution more efficient.

**Tech Stack:** .NET 8, Blazor Server, Entity Framework Core, MediatR, SQLite (local) / PostgreSQL (production)

## Essential Commands

### Quick Start with start-fresh.bat (Recommended)
```bash
# Windows - Ejecutar desde la raíz del proyecto

# MODO RÁPIDO (recomendado para desarrollo diario)
start-fresh.bat

# MODO COMPLETO (cuando tengas problemas de BD/migración)
start-fresh.bat --hard

# Ver ayuda completa
start-fresh.bat --help
```

**MODO RÁPIDO** (default):
- Solo detiene procesos previos
- Compilación incremental rápida
- Reutiliza BD existente
- Ideal para desarrollo diario

**MODO COMPLETO** (--hard):
- Limpia completamente BD, bins, y caché NuGet
- Restaura paquetes completos
- Aplica migraciones de Entity Framework
- Usa cuando tengas problemas de BD/migración

**IMPORTANTE**: Ejecutar desde el directorio raíz (donde está VHouse.sln). El script validará automáticamente que está en el directorio correcto.

### Development Workflow Manual
```bash
# Si prefieres control manual:

# Restore dependencies
dotnet restore

# Build solution (warnings as errors in CI)
dotnet build

# Run with hot reload (development)
dotnet watch run --project VHouse.Web

# Run normally
dotnet run --project VHouse.Web
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/VHouse.Infrastructure

# Apply migrations to database
dotnet ef database update --project src/VHouse.Infrastructure

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project src/VHouse.Infrastructure

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/VHouse.Infrastructure
```

### Testing
```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security
```

### Docker
```bash
# Start PostgreSQL for development
docker-compose up -d

# Start with admin tools (pgAdmin)
docker-compose -f docker-compose.yml -f docker-compose.dbadmin.yml up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Architecture Overview

### Clean Architecture Layers

```
VHouse.Web (Blazor Server)
├── Components/
│   ├── Pages/         - Routable pages
│   ├── Layout/        - Layout components
│   └── Shared/        - Reusable components
├── Extensions/        - Configuration extensions
└── Program.cs         - Application entry point

VHouse.Application (CQRS + MediatR)
├── Commands/          - Write operations (CreateProductCommand, etc.)
├── Queries/           - Read operations
├── Handlers/          - Command/Query handlers
├── DTOs/              - Data Transfer Objects
└── Common/            - Shared application logic

VHouse.Infrastructure (EF Core + Services)
├── Data/
│   └── VHouseDbContext.cs  - EF Core DbContext
├── Repositories/      - Data access implementations
└── Services/          - External service integrations (AI, etc.)

VHouse.Domain (Entities + Business Rules)
├── Entities/          - Core business entities
└── Exceptions/        - Domain exceptions
```

### CQRS Pattern

Commands and queries are handled through MediatR:
- **Commands**: Write operations that modify state (in `VHouse.Application/Commands/`)
- **Queries**: Read operations that return data (in `VHouse.Application/Queries/`)
- **Handlers**: Process commands/queries (in `VHouse.Application/Handlers/`)

Example command flow:
1. Component sends command via MediatR
2. Appropriate handler processes it
3. Handler uses repositories to persist changes
4. Result returned to component

### Database Context

`VHouseDbContext` (in `src/VHouse.Infrastructure/Data/VHouseDbContext.cs`) contains:
- **Core entities**: Product, Order, OrderItem, Customer, Supplier
- **Multi-tenancy**: ClientTenant, ClientProduct
- **Delivery tracking**: Delivery, DeliveryItem
- **Consignment**: Consignment, ConsignmentItem, ConsignmentSale
- **Monitoring**: AuditLog, SystemMetric, BusinessAlert
- **Gallery**: Album, Photo

### Multi-Tenancy

The system supports multiple clients through `ClientTenant` entity. Each client operates in isolation:
- Tenant-specific product catalogs
- Isolated order processing
- Separate analytics per client

## Configuration

### Environment Setup

1. Copy `.env.example` to `.env` in project root
2. Configure required values:
   - Database connection strings
   - AI API keys (Claude preferred, OpenAI fallback)
   - Application URLs

The application loads `.env` automatically in `Program.cs`.

### AI Configuration

Dual AI provider setup with automatic fallback:
- **Primary**: Claude API (Anthropic)
- **Fallback**: OpenAI
- Configuration in `appsettings.json` under `Claude`, `OpenAI`, and `AI` sections

### Database Configuration

- **Local Development**: SQLite (`vhouse_clean.db`)
- **Production**: PostgreSQL (via Docker or cloud)
- Connection string in `appsettings.json` or environment variables

## Key Features & Systems

### Gallery System
Multi-file upload with album organization. Uses secure validation and thumbnail generation.
- Albums contain multiple photos
- File validation for security
- Size limits enforced (10MB default)

### Delivery Tracking
Track deliveries to clients with item-level detail.
- Delivery routes and schedules
- Item status per delivery
- Client confirmation workflows

### Consignment System
Manage products on consignment with sales tracking.
- Track items at client locations
- Record sales and settlements
- Inventory reconciliation

### Audit & Monitoring
Enterprise-level tracking for compliance and debugging.
- `AuditLog` for all critical operations
- `SystemMetric` for performance monitoring
- `BusinessAlert` for anomaly detection

## Development Patterns

### Adding a New Feature

1. **Domain Entity** (`VHouse.Domain/Entities/`)
   - Create entity with business rules
   - Add to `VHouseDbContext` DbSet

2. **Application Layer** (`VHouse.Application/`)
   - Create command/query
   - Create DTO for data transfer
   - Implement handler with business logic

3. **Infrastructure** (`VHouse.Infrastructure/`)
   - Add repository if needed
   - Configure EF Core relationships in DbContext

4. **Web Layer** (`VHouse.Web/`)
   - Create Blazor component in appropriate folder
   - Inject MediatR to send commands/queries

5. **Migration**
   ```bash
   dotnet ef migrations add AddFeatureName --project src/VHouse.Infrastructure
   dotnet ef database update --project src/VHouse.Infrastructure
   ```

### Component Organization

- **Pages**: Routable components with `@page` directive
- **Layout**: MainLayout, NavMenu, etc.
- **Shared**: Reusable UI components
- **Feature folders**: Group related components (Orders/, etc.)

### Dependency Injection

Services registered in:
- `Program.cs` for Web layer
- `ApplicationServiceRegistration.cs` for Application layer
- Infrastructure services registered via extension methods

## Testing Strategy

### Test Categories

Use `[Trait("Category", "...")]` to categorize:
- **Unit**: Domain logic, validators, handlers (no external dependencies)
- **Integration**: Database operations, API calls
- **Security**: Tenant isolation, authorization, validation

### Running Targeted Tests

```bash
# Fast unit tests only
dotnet test --filter Category=Unit

# Full integration suite (slower)
dotnet test --filter Category=Integration

# Security-focused tests
dotnet test --filter Category=Security
```

## Important Notes

### Line Endings
**CRITICAL**: All files must use CRLF (`\r\n`) line endings for Windows compatibility.

### Git Commits
- Commits are authored by Bernard Uriza Orozco only (no AI co-author attribution)
- Use conventional commit format: `feat:`, `fix:`, `refactor:`, etc.

### Database Provider
- **Default**: SQLite for simplicity in local development
- **Note**: README mentions PostgreSQL, but current implementation uses SQLite
- PostgreSQL docker-compose is available for testing production scenarios

### Web Server
- Runs on `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS)
- Configured in `.env` via `ASPNETCORE_URLS`

## Troubleshooting

### start-fresh.bat Issues
```bash
# Si start-fresh.bat falla:

# 1. Verificar ubicación - debe ejecutarse desde la raíz
dir VHouse.sln  # Debe encontrar este archivo

# 2. Si fallan las migraciones, ejecutar manualmente:
dotnet ef migrations add InitialCreate --project src/VHouse.Infrastructure
dotnet ef database update --project src/VHouse.Infrastructure

# 3. Si los puertos están ocupados:
netstat -ano | findstr :5000  # Ver qué proceso usa el puerto
taskkill /F /PID [PID_NUMBER] # Terminar ese proceso

# 4. Para limpiar completamente y empezar de nuevo:
taskkill /F /IM dotnet.exe
del /F /Q vhouse_clean.db* VHouse.Web\vhouse_clean.db*
rmdir /S /Q src\VHouse.Infrastructure\Migrations
dotnet clean
```

### Migration Issues
```bash
# If migrations fail, check:
# 1. Infrastructure project builds successfully
dotnet build src/VHouse.Infrastructure

# 2. Connection string is valid
# 3. Database file/server is accessible

# Reset database (destructive - local dev only)
rm vhouse_clean.db  # or drop PostgreSQL database
dotnet ef database update --project src/VHouse.Infrastructure
```

### AI Service Failures
Both Claude and OpenAI APIs configured with fallback:
1. Check API keys in `.env` file
2. Verify `appsettings.json` has correct configuration
3. Check application logs for specific error messages
4. Fallback should activate automatically if primary fails

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## CI/CD Pipeline

GitHub Actions workflows (`.github/workflows/`):
- **ci.yml**: Build, test, coverage, Docker build, SBOM generation
- **codeql.yml**: Security scanning

Build requirements:
- Must build with `-warnaserror` (warnings treated as errors)
- Tests must pass
- Code coverage tracked