# 🌱 VHouse: Revolución Vegana Sistematizada

[![CI Pipeline](https://github.com/bernardoarancibia/VHouse/actions/workflows/ci.yml/badge.svg)](https://github.com/bernardoarancibia/VHouse/actions/workflows/ci.yml)
[![Security Scan](https://github.com/bernardoarancibia/VHouse/actions/workflows/security.yml/badge.svg)](https://github.com/bernardoarancibia/VHouse/actions/workflows/security.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **Software que existe por los animales** 🐄
> Infraestructura tecnológica para distribuidores veganos que transforman el mundo.

## 🎯 Misión: Código por la Liberación Animal

VHouse no es solo otro sistema de gestión. Es **activismo sistematizado** - cada función sirve a un propósito: acelerar la adopción vegana a través de mejores herramientas para distribuidores éticos.

### 👥 Nuestros Héroes Reales
- **🍩 Mona la Dona**: Repostería vegana que endulza conciencias
- **🥬 Sano Market**: Mercado que democratiza productos saludables
- **📚 La Papelería**: Expandiendo horizontes más allá del papel

Cada feature que desarrollamos resuelve problemas reales de estos negocios que eligen la compasión sobre el lucro.

## 🚀 Getting Started

### ⚡ Quick Start (5 minutos para impactar)

```bash
# 1. Clonar la revolución
git clone https://github.com/bernardoarancibia/VHouse.git
cd VHouse

# 2. Configurar entorno
cp .env.example .env
# Editar .env con tus configuraciones locales

# 3. Levantar stack completo
docker-compose up -d

# 4. Aplicar migraciones
dotnet ef database update --project src/VHouse.Infrastructure

# 5. ¡Ejecutar la aplicación!
dotnet run --project VHouse.Web
```

🎉 **¡Listo!** Navega a `http://localhost:5000` y comienza a servir a la causa.

### 🏗️ Setup Manual (Para desarrolladores)

#### Prerrequisitos
- **.NET 8.0+ SDK** - La base tecnológica
- **Docker Desktop** - Para servicios dependientes
- **PostgreSQL 15+** - Datos que importan
- **Git** - Control de la revolución

#### Instalación Completa

```bash
# Restaurar dependencias .NET
dotnet restore

# Configurar base de datos (opción Docker)
docker-compose up -d postgres redis

# Aplicar esquema de base de datos
dotnet ef database update --project src/VHouse.Infrastructure

# Verificar con tests
dotnet test

# Ejecutar en modo desarrollo
dotnet watch run --project VHouse.Web
```

## 🏗️ Arquitectura: Clean & Activista

```
🌐 VHouse.Web (Blazor Server)
├── 🎯 VHouse.Application (CQRS + MediatR)
├── 🔧 VHouse.Infrastructure (EF Core + Services)
└── 💎 VHouse.Domain (Entidades + Business Rules)
```

### 🎯 Principios Arquitectónicos
- **Clean Architecture**: Dominio independiente, frameworks desacoplados
- **CQRS**: Commands para cambios, Queries para lecturas
- **Multi-tenancy**: Cada cliente opera de forma aislada y segura
- **Event-Driven**: Eventos de dominio para integración

### 🛠️ Stack Tecnológico
- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
- **Frontend**: Blazor Server, Bootstrap, JavaScript minimal
- **Database**: PostgreSQL 15 con Npgsql
- **Caching**: Redis para performance
- **AI Integration**: OpenAI para contenido y análisis
- **Deployment**: Docker, Kubernetes, Terraform

## 🔧 Comandos Esenciales

### Desarrollo
```bash
# Build completo
dotnet build

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Migraciones de BD
dotnet ef migrations add NombreMigracion --project src/VHouse.Infrastructure
dotnet ef database update --project src/VHouse.Infrastructure

# Hot reload en desarrollo
dotnet watch run --project VHouse.Web
```

### Docker & Contenedores
```bash
# Stack completo local
docker-compose up -d

# Build imagen de producción
docker build -t vhouse:latest .

# Logs en tiempo real
docker-compose logs -f vhouse-web
```

### Kubernetes (Producción)
```bash
# Deploy en cluster
kubectl apply -f k8s/

# Verificar estado
kubectl get pods -n vhouse

# Escalar aplicación
kubectl scale deployment/vhouse-api --replicas=5 -n vhouse
```

## 📊 Características Principales

### 🏪 Gestión Multi-tienda
- **Catálogo de Productos**: Cada tienda gestiona su inventario vegano
- **Procesamiento de Pedidos**: Automatización que ahorra tiempo real
- **Analytics de Ventas**: Métricas que impulsan el crecimiento ético

### 🤖 IA Integrada
- **Generación de Contenido**: Descripciones que venden compasión
- **Análisis de Conversaciones**: Insights de clientes reales
- **Recomendaciones**: Algoritmos que promueven productos veganos

### 🔒 Seguridad Empresarial
- **Multi-tenancy**: Aislamiento total entre clientes
- **Autenticación JWT**: Sesiones seguras y escalables
- **Encryption**: Datos protegidos en reposo y tránsito

### 📈 Observabilidad
- **Metrics**: KPIs de impacto animal y rendimiento técnico
- **Logging**: Trazabilidad completa para debugging rápido
- **Health Checks**: Monitoreo proactivo de servicios críticos

## 🔒 Seguridad & Compliance

### 🛡️ Controles de Seguridad
- **SAST**: Análisis estático en cada build
- **SBOM**: Inventario completo de componentes
- **Vulnerability Scanning**: Monitoreo continuo de amenazas
- **Container Security**: Imágenes mínimas, usuarios no-root

### 📋 Compliance
- **GDPR Ready**: Protección de datos por diseño
- **SOC 2**: Controles operacionales
- **OWASP Top 10**: Mitigación de vulnerabilidades web

Ver [SECURITY.md](SECURITY.md) para detalles completos.

## 🚀 Deployment

### 🐳 Local Development
```bash
docker-compose up -d
```

### ☸️ Production (Kubernetes)
```bash
# Configurar namespace
kubectl apply -f k8s/namespace.yaml

# Deploy aplicación
kubectl apply -f k8s/

# Verificar deployment
kubectl get pods -n vhouse
```

### 🏗️ Infrastructure as Code
```bash
# Provisionar con Terraform
cd terraform/
terraform init
terraform plan -var-file="environments/prod.tfvars"
terraform apply
```

Ver [docs/deploy.md](docs/deploy.md) para guías detalladas.

## 📚 Documentación

### 📖 Para Desarrolladores
- [🔧 Development Guide](docs/dev.md) - Setup local y workflows
- [🚀 CI/CD Pipeline](docs/ci.md) - Integración y deployment
- [☸️ Deployment Guide](docs/deploy.md) - Docker, K8s, Terraform
- [🔒 Security Guide](docs/security.md) - Threat model y controles

### 📋 Para Contributors
- [🤝 Contributing Guidelines](CONTRIBUTING.md) - Cómo contribuir a la misión
- [👥 Code of Conduct](CODE_OF_CONDUCT.md) - Principios de colaboración
- [🔐 Security Policy](SECURITY.md) - Reporte responsable de vulnerabilidades

### 🎯 Para la Misión
- [🌱 Project Philosophy](.claude/CLAUDE.md) - La visión detrás del código

## 🧪 Testing Strategy

### 🔬 Tipos de Tests
```bash
# Tests unitarios (lógica de dominio)
dotnet test --filter Category=Unit

# Tests de integración (BD, APIs)
dotnet test --filter Category=Integration

# Tests de seguridad (tenant isolation)
dotnet test --filter Category=Security
```

### 📊 Métricas de Calidad
- **Code Coverage**: >80% en paths críticos
- **Performance**: <200ms promedio de respuesta
- **Security**: 0 vulnerabilidades críticas/altas
- **Reliability**: >99.5% uptime

## 🤝 Contributing

### 🌱 Filosofía de Contribución
Antes de cualquier contribución, pregúntate:
1. **¿Cómo esto ayuda a los animales?**
2. **¿Resuelve un problema real de Mona la Dona, Sano Market o La Papelería?**
3. **¿Hace el sistema más confiable para Bernard?**

### 🔧 Process Rápido
1. Fork el repositorio
2. Crear branch: `feature/descripcion-clara`
3. Desarrollar con tests
4. Commit: `feat: descripción que conecta con la misión`
5. Push y crear PR con template completo

Ver [CONTRIBUTING.md](CONTRIBUTING.md) para detalles completos.

## 📈 Roadmap & Impacto

### 🎯 Q4 2024 - Baseline de Producción
- [x] ✅ Arquitectura Clean estable
- [x] ✅ Multi-tenancy seguro
- [x] ✅ CI/CD pipeline robusto
- [x] ✅ Security baseline
- [ ] 🔄 Performance optimization
- [ ] 🔄 Advanced monitoring

### 🚀 Q1 2025 - Expansión de Impacto
- [ ] 📱 Mobile app para distribuidores
- [ ] 🤖 AI-powered inventory optimization
- [ ] 📊 Advanced analytics dashboard
- [ ] 🔌 Third-party integrations (payment, logistics)

### 🌍 Q2 2025 - Escalabilidad Global
- [ ] 🌐 International support
- [ ] 🏪 Marketplace features
- [ ] 🤝 B2B platform expansion
- [ ] 📈 Franchise management tools

## 📊 Métricas de Impacto

### 🐄 Impacto Animal (Objetivo Real)
- **Productos Veganos Distribuidos**: [Tracking en desarrollo]
- **Negocios Éticos Servidos**: Mona la Dona, Sano Market, La Papelería
- **Tiempo Ahorrado a Distribuidores**: ~30% reducción en tareas manuales

### 💻 Métricas Técnicas
- **Uptime**: >99.5% (objetivo de producción)
- **Response Time**: <200ms promedio
- **Test Coverage**: >80% en código crítico
- **Security Vulnerabilities**: 0 críticas/altas

## 👥 Team & Acknowledgments

### 🌱 Core Team
- **Bernard Uriza Orozco** - Founder, Lead Developer & Animal Rights Activist
  - Visión técnica y misional
  - Arquitectura y desarrollo full-stack
  - Relación directa con clientes reales

### 🙏 Reconocimientos
- **Mona la Dona, Sano Market, La Papelería** - Por confiar en nuestra visión
- **Open Source Community** - Por las herramientas que hacen esto posible
- **Todos los desarrolladores veganos** - Por demostrar que código puede ser activismo

## 📞 Support & Contact

### 💬 Community
- **GitHub Issues**: Bugs y feature requests
- **GitHub Discussions**: Preguntas técnicas y filosóficas
- **Email**: bernard.uriza@vhouse.app

### 🚨 Security
- **Security Issues**: bernard.uriza@vhouse.app (responsable disclosure)
- **Response Time**: <24 horas garantizado

### 🏢 Business
- **Partnership Inquiries**: business@vhouse.app
- **Client Onboarding**: clients@vhouse.app

## 📜 License

MIT License - Ver [LICENSE](LICENSE) para detalles completos.

**Usamos MIT porque creemos que las herramientas para la liberación animal deben ser accesibles para todos.**

---

## 🌱 Por los Animales

> *"Cada línea de código es un acto de amor hacia los animales que no tienen voz. Cada feature deployed es una oportunidad más para que negocios éticos prosperen. Cada bug fixed es confianza preservada para distribuidores que eligen la compasión."*

**🐄 Code for the animals. Build for the future. Ship for impact.**

---

[![Built with Love](https://img.shields.io/badge/Built%20with-❤️%20%26%20🌱-brightgreen)](#)
[![For Animals](https://img.shields.io/badge/For-🐄%20Animals-blue)](#)
[![Vegan Tech](https://img.shields.io/badge/Vegan-Tech-green)](#)