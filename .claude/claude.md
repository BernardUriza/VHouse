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

### Verificación de Disponibilidad

Después de ejecutar `start-fresh.bat`, usa `wait-for-vhouse.sh` para verificar que la app está respondiendo:

```bash
# Ejecutar en Git Bash o WSL
./wait-for-vhouse.sh

# Esto:
# - Espera hasta 120 segundos a que VHouse responda en localhost:5000
# - Verifica con curl que el endpoint HTTP funciona
# - Evita el error "ERR_CONNECTION_REFUSED"
# - Confirma que la app realmente arrancó correctamente
```

Si el script falla, revisa los logs de `start-fresh.bat` para ver errores en migraciones, compilación o seeding de datos.

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
VHouse/                        (Repository root)
├── VHouse.Web/                - Blazor Server application (at root level)
│   ├── Components/
│   │   ├── Pages/            - Routable pages (@page directive)
│   │   ├── Layout/           - MainLayout, NavMenu
│   │   ├── Shared/           - Reusable UI components
│   │   └── Orders/           - Feature-specific components
│   ├── Extensions/           - Service registration extensions
│   ├── Middleware/           - Security headers, etc.
│   ├── Services/             - Web-layer services
│   └── Program.cs            - Application entry point, .env loading
│
├── src/VHouse.Application/   - CQRS + MediatR
│   ├── Commands/             - Write operations (CreateProductCommand, etc.)
│   ├── Queries/              - Read operations
│   ├── Handlers/             - Command/Query handlers
│   ├── DTOs/                 - Data Transfer Objects
│   ├── Services/             - Application services (IAIService, etc.)
│   └── Common/               - Shared application logic
│
├── src/VHouse.Infrastructure/ - EF Core + External Services
│   ├── Data/
│   │   └── VHouseDbContext.cs - EF Core DbContext with all entities
│   ├── Migrations/           - EF Core migrations
│   ├── Repositories/         - Data access implementations
│   └── Services/             - AI services, file storage, etc.
│
├── src/VHouse.Domain/        - Core Domain
│   ├── Entities/             - Product, Order, Customer, ClientTenant, etc.
│   ├── Enums/                - Domain enumerations
│   ├── Interfaces/           - Domain interfaces
│   ├── ValueObjects/         - Domain value objects
│   └── Exceptions/           - Domain exceptions
│
└── tests/VHouse.Tests/       - Test suite
    └── Gallery/              - Feature-specific tests
```

**Note**: VHouse.Web is at repository root level (not in src/), while other layers are in src/.

### CQRS Pattern with MediatR

Commands and queries are handled through MediatR:
- **Commands**: Write operations that modify state (in `src/VHouse.Application/Commands/`)
- **Queries**: Read operations that return data (in `src/VHouse.Application/Queries/`)
- **Handlers**: Process commands/queries (in `src/VHouse.Application/Handlers/`)

Example command flow:
```
Blazor Component → IMediator.Send(command) → CommandHandler → Repository → DbContext → Database
                                                    ↓
                                                 Response
```

Typical implementation pattern:
1. **Component** injects `IMediator` and sends command/query
2. **Handler** implements `IRequestHandler<TRequest, TResponse>`
3. **Handler** uses injected repositories/services for business logic
4. **Result** flows back to component for UI update

Example from codebase:
```csharp
// Component sends command
var result = await Mediator.Send(new CreateProductCommand { Name = "Product", Price = 10.0m });

// Handler processes it (in VHouse.Application/Handlers/)
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Business logic here
        var product = new Product { Name = request.Name, Price = request.Price };
        await _repository.AddAsync(product);
        return new ProductDto { Id = product.Id, Name = product.Name };
    }
}
```

### Database Context

`VHouseDbContext` (in `src/VHouse.Infrastructure/Data/VHouseDbContext.cs`) contains:
- **Core entities**: Product, Order, OrderItem, Customer, Supplier
- **Multi-tenancy**: ClientTenant, ClientProduct, ClientTenantPriceList
- **Pricing**: PriceList, PriceListItem
- **Delivery tracking**: Delivery, DeliveryItem
- **Consignment**: Consignment, ConsignmentItem, ConsignmentSale
- **Monitoring**: AuditLog, SystemMetric, BusinessAlert
- **Gallery**: Album, Photo

**Gallery seeding**: The DbContext seeds 7 default albums on database creation:
- Products, Sales Receipts, Purchase Receipts, Invoices, Suppliers, Customers, Misc

### Multi-Tenancy

The system supports multiple clients (Mona la Dona, Sano Market, La Papelería) through `ClientTenant` entity. Each client operates in isolation:
- **Tenant identification**: Via `TenantCode` (slug) and `LoginUsername`
- **Product catalogs**: `ClientProduct` links tenants to specific products with custom pricing
- **Price lists**: `ClientTenantPriceList` allows per-tenant pricing strategies
- **Isolated operations**: Orders, deliveries, and consignments scoped to tenant
- **Separate analytics**: Business metrics filtered by `ClientTenant`

Client URLs follow pattern: `http://localhost:5000/client/{TENANT_CODE}` (e.g., `/client/MONA_DONA`)

## Configuration

### Environment Setup

1. Copy `.env.example` to `.env` in project root
2. Configure required values:
   - Database connection strings
   - AI API keys (Claude preferred, OpenAI fallback)
   - Application URLs (`ASPNETCORE_URLS`)

The application loads `.env` automatically in `VHouse.Web/Program.cs` (lines 9-27) before configuration and services are registered. The .env file is located one directory up from VHouse.Web (`../.env`).

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

Follow Clean Architecture principles by implementing from the inside out:

1. **Domain Entity** (`src/VHouse.Domain/Entities/`)
   - Create entity class with business rules and validation
   - Add navigation properties for relationships
   - Add to `VHouseDbContext` as `DbSet<YourEntity>`

2. **Infrastructure Configuration** (`src/VHouse.Infrastructure/Data/VHouseDbContext.cs`)
   - Configure entity in `OnModelCreating()`:
     - Primary keys, indexes, unique constraints
     - Decimal precision for money fields: `HasColumnType("decimal(18,2)")`
     - String max lengths with `HasMaxLength()`
     - Relationships with `HasOne()/WithMany()` and foreign keys
     - Delete behavior (`OnDelete(DeleteBehavior.Cascade/SetNull)`)

3. **Application Layer** (`src/VHouse.Application/`)
   - Create **Command** or **Query** in respective folders
   - Create **DTO** in `DTOs/` for data transfer
   - Implement **Handler** in `Handlers/` folder
     - Inject required repositories/services in constructor
     - Implement `IRequestHandler<TRequest, TResponse>`
     - Add business logic in `Handle()` method

4. **Web Layer** (`VHouse.Web/Components/`)
   - Create Blazor component in appropriate folder (Pages/, Shared/, or feature folder)
   - Inject `IMediator` with `@inject IMediator Mediator`
   - Call commands/queries: `await Mediator.Send(new YourCommand())`
   - Handle results and update UI

5. **Migration** (from VHouse.Web directory or specify startup project)
   ```bash
   # From VHouse.Web directory
   dotnet ef migrations add AddFeatureName --project ../src/VHouse.Infrastructure

   # Or specify startup project from root
   dotnet ef migrations add AddFeatureName --project src/VHouse.Infrastructure --startup-project VHouse.Web

   # Apply migration
   dotnet ef database update --project src/VHouse.Infrastructure --startup-project VHouse.Web
   ```

6. **Testing** (`tests/VHouse.Tests/`)
   - Add unit tests for handlers and domain logic
   - Add integration tests for repository operations
   - Use `[Trait("Category", "Unit|Integration|Security")]` attributes

### Component Organization

- **Pages**: Routable components with `@page` directive
- **Layout**: MainLayout, NavMenu, etc.
- **Shared**: Reusable UI components
- **Feature folders**: Group related components (Orders/, etc.)

### Dependency Injection

Services are registered in multiple locations following Clean Architecture:
- **Web layer**: `VHouse.Web/Program.cs` and extension methods in `VHouse.Web/Extensions/`
- **Application layer**: Via `AddVHouseServices()` extension method
- **Infrastructure layer**: Via `AddInfrastructureServices()` extension method (registered in Program.cs line 41)

Registration happens in `Program.cs` with method chaining:
```csharp
builder.ConfigureWebHost()
       .ConfigureDatabase()
       .Services.AddVHouseServices(builder.Configuration, builder.Environment)
       .AddInfrastructureServices(builder.Configuration);
```

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

# 2. Si fallan las migraciones, ejecutar manualmente desde VHouse.Web:
cd VHouse.Web
dotnet ef migrations add InitialCreate --project ../src/VHouse.Infrastructure
dotnet ef database update --project ../src/VHouse.Infrastructure
cd ..

# 3. Si los puertos están ocupados:
netstat -ano | findstr :5000  # Ver qué proceso usa el puerto
taskkill /F /PID [PID_NUMBER] # Terminar ese proceso

# 4. Para limpiar completamente y empezar de nuevo:
taskkill /F /IM dotnet.exe
del /F /Q vhouse_clean.db* VHouse.Web\vhouse_clean.db*
rmdir /S /Q src\VHouse.Infrastructure\Migrations
dotnet clean
start-fresh.bat --hard

# 5. Si wait-for-vhouse.sh falla (en Git Bash/WSL):
# - Revisa logs de start-fresh.bat para errores de compilación
# - Verifica que el puerto 5000 no esté bloqueado por firewall
# - Asegúrate de que la BD se creó correctamente
```

### Migration Issues
```bash
# If migrations fail, check:
# 1. Infrastructure project builds successfully
dotnet build src/VHouse.Infrastructure

# 2. Connection string is valid (in appsettings.json or .env)
# 3. Database file/server is accessible

# 4. EF Tools are installed globally
dotnet tool install --global dotnet-ef

# Reset database (destructive - local dev only)
# SQLite:
del /F /Q vhouse_clean.db vhouse_clean.db-shm vhouse_clean.db-wal
del /F /Q VHouse.Web\vhouse_clean.db*

# Recreate from migrations
cd VHouse.Web
dotnet ef database update --project ../src/VHouse.Infrastructure
cd ..

# Or use start-fresh.bat --hard for complete reset
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
- **codeql.yml**: Security scanning (CodeQL analysis)

Build requirements:

- Must build with `-warnaserror` in CI (warnings treated as errors)
- All tests must pass
- Code coverage tracked (>80% target for critical paths)
- Security scans must be clean (no critical/high vulnerabilities)

## Common Workflows

### Daily Development

```bash
# Quick iteration (use daily)
start-fresh.bat             # Windows - fast mode
dotnet watch run --project VHouse.Web  # Alternative with hot reload

# After pulling changes
dotnet restore
dotnet build
```

### After Schema Changes

```bash
# When you or someone else added/modified entities
start-fresh.bat --hard      # Complete reset with migrations

# Or manually:
cd VHouse.Web
dotnet ef migrations add DescriptiveName --project ../src/VHouse.Infrastructure
dotnet ef database update --project ../src/VHouse.Infrastructure
cd ..
```

### Before Committing

```bash
# Run tests
dotnet test

# Check for build warnings
dotnet build

# Run security checks (if tools installed)
dotnet format --verify-no-changes
```

### Creating a Pull Request

```bash
# 1. Ensure tests pass
dotnet test

# 2. Ensure build succeeds
dotnet build

# 3. Check for any uncommitted files
git status

# 4. Commit with conventional format
git add .
git commit -m "feat: add price list management for multi-tenant pricing"
# Formats: feat:, fix:, refactor:, docs:, test:, chore:
```

## Project-Specific Conventions

### Naming Patterns

- **Entities**: Singular nouns (Product, Order, Customer)
- **DbSets**: Plural (Products, Orders, Customers)
- **Commands**: VerbNounCommand (CreateProductCommand, UpdateOrderCommand)
- **Queries**: GetNounQuery, ListNounsQuery (GetProductQuery, ListProductsQuery)
- **Handlers**: CommandName + Handler (CreateProductCommandHandler)
- **DTOs**: NounDto (ProductDto, OrderDto)

### Money Fields

Always use `decimal(18,2)` precision:

```csharp
entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
```

### Tenant Isolation

When working with multi-tenant features:

- Always filter queries by ClientTenant
- Validate tenant access in handlers
- Include `ClientTenant` field in audit logs

### Seeded Data

The database seeds gallery albums automatically. Don't manually create these in code:

- Products, Sales Receipts, Purchase Receipts, Invoices, Suppliers, Customers, Misc
