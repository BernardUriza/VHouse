# 🔧 Guía de Desarrollo VHouse

## 🎯 Configuración del Entorno Local

### Prerrequisitos
- .NET 8.0+ SDK
- Docker Desktop
- PostgreSQL (o usar docker-compose)
- Git

### Setup Inicial
```bash
# Clonar el repositorio
git clone https://github.com/bernardoarancibia/VHouse.git
cd VHouse

# Restaurar dependencias
dotnet restore

# Configurar base de datos (opción 1: Docker)
docker-compose up -d postgres

# Configurar base de datos (opción 2: Local)
./start-postgres.sh

# Aplicar migraciones
dotnet ef database update --project src/VHouse.Infrastructure

# Ejecutar aplicación
dotnet run --project VHouse.Web
```

## 🏗️ Arquitectura de Desarrollo

### Estructura del Proyecto
```
src/
├── VHouse.Domain/           # Entidades y reglas de negocio
├── VHouse.Application/      # Casos de uso y CQRS
├── VHouse.Infrastructure/   # Datos y servicios externos
└── VHouse.Web/             # Interfaz Blazor

tests/
└── VHouse.Tests/           # Tests unitarios e integración

k8s/                        # Manifiestos Kubernetes
terraform/                  # Infraestructura como código
scripts/                    # Scripts de automatización
```

### Comandos de Desarrollo

#### Build y Test
```bash
# Construir solución
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Limpiar build artifacts
dotnet clean
```

#### Migraciones de Base de Datos
```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion --project src/VHouse.Infrastructure

# Aplicar migraciones
dotnet ef database update --project src/VHouse.Infrastructure

# Generar script SQL
dotnet ef migrations script --project src/VHouse.Infrastructure
```

#### Docker Local
```bash
# Stack completo con docker-compose
docker-compose up

# Solo servicios dependientes
docker-compose up postgres redis

# Build imagen local
docker build -t vhouse:local .
```

## 🔄 Flujo de Trabajo

### Branching Strategy
```bash
# Feature branch
git checkout -b feature/descripcion-breve
git commit -m "feat: descripción del cambio"
git push origin feature/descripcion-breve

# Hotfix
git checkout -b hotfix/descripcion-breve
git commit -m "fix: descripción del fix"
```

### Mensajes de Commit
Usar formato conventional commits:
- `feat:` nuevas funcionalidades
- `fix:` corrección de bugs
- `docs:` cambios en documentación
- `style:` formato, espacios en blanco
- `refactor:` cambios de código que no agregan funcionalidad
- `test:` agregar o corregir tests
- `chore:` cambios en build, dependencias

## 🧪 Testing Strategy

### Tipos de Tests
1. **Unit Tests**: Lógica de dominio y aplicación
2. **Integration Tests**: APIs y base de datos
3. **E2E Tests**: Flujos críticos de usuario

### Ejecutar Tests Específicos
```bash
# Solo tests unitarios
dotnet test --filter Category=Unit

# Solo tests de integración
dotnet test --filter Category=Integration

# Test específico
dotnet test --filter "FullyQualifiedName~ProductService"
```

## 🔧 Herramientas de Desarrollo

### IDEs Recomendados
- **Visual Studio 2022** (Windows)
- **Visual Studio Code** (Cross-platform)
- **JetBrains Rider** (Cross-platform)

### Extensions Útiles
- C# Dev Kit (VS Code)
- GitLens
- Docker Extension
- Kubernetes Extension

### Debugging
```bash
# Debug con hot reload
dotnet watch run --project VHouse.Web

# Debug specific project
dotnet run --project VHouse.Web --environment Development
```

## 📊 Métricas de Desarrollo

### Performance Targets
- Build time: <30 segundos
- Test execution: <2 minutos
- Hot reload: <3 segundos

### Quality Gates
- Code coverage: >80% líneas críticas
- No vulnerabilidades críticas/altas
- Todos los tests pasan
- No warnings en build release

## 🔍 Troubleshooting

### Problemas Comunes

#### "Database connection failed"
```bash
# Verificar que PostgreSQL esté ejecutándose
docker ps | grep postgres

# Verificar connection string en .env
cat .env | grep ConnectionString
```

#### "Port already in use"
```bash
# Encontrar proceso usando puerto 5000
netstat -ano | findstr :5000

# Matar proceso (Windows)
taskkill /PID <proceso_id> /F
```

#### "EF Migrations error"
```bash
# Verificar estado de migraciones
dotnet ef migrations list --project src/VHouse.Infrastructure

# Reset database (desarrollo solamente)
dotnet ef database drop --project src/VHouse.Infrastructure
dotnet ef database update --project src/VHouse.Infrastructure
```

## 📚 Recursos

- [Clean Architecture .NET](https://github.com/jasontaylordev/CleanArchitecture)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**💡 Tip Activista**: Cada feature debe resolverle un problema real a Mona la Dona, Sano Market o La Papelería. Siempre pregúntate: "¿Cómo esto ayuda a los animales?"