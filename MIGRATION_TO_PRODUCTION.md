# VHouse - Plan de Migración a Sistema de Gestión de Inventarios B2B para Distribuidores

## Resumen Ejecutivo

VHouse es un sistema de gestión de inventarios diseñado para distribuidores mayoristas de productos veganos que manejan múltiples marcas y abastecen a diversos puntos de venta. El sistema permite gestionar inventarios centralizados y por cliente, manejar múltiples niveles de precios, y automatizar el procesamiento de pedidos. Actualmente se encuentra en estado MVP y requiere mejoras significativas en seguridad, funcionalidades B2B y escalabilidad antes de ser apto para producción.

## Estado Actual del Proyecto

### Stack Tecnológico
- **Framework**: ASP.NET Core 8.0 con Blazor Server
- **Base de Datos**: PostgreSQL con Entity Framework Core 9.0.4
- **Interfaz**: Bootstrap CSS + Font Awesome
- **Despliegue**: Docker + Fly.io

### Características Actuales para Distribución
- **Gestión de Precios Multinivel**: Costo, Mayoreo, Sugerido, Público
- **Inventario Dual**: General + Por Cliente (ideal para consignación)
- **Procesamiento de Órdenes**: Con actualización automática de inventarios
- **Gestión de Clientes B2B**: Clasificación retail/mayorista
- **Trazabilidad**: Fechas de caducidad y vinculación con facturas
- **Procesamiento con IA**: Interpretación de pedidos por texto

### Limitaciones Críticas para Operación B2B
- Sin gestión de proveedores ni órdenes de compra
- Sin soporte multi-bodega
- Sin control de créditos ni cobranza
- Sin trazabilidad por lotes
- Sin sistema de autenticación/autorización

## Vulnerabilidades Críticas a Resolver

### 🔴 PRIORIDAD CRÍTICA

1. **Credenciales Expuestas**
   - API Key de OpenAI hardcodeada en `ChatbotService.cs`
   - Contraseña de base de datos en `appsettings.json`
   - **Acción**: Migrar a variables de entorno o Azure Key Vault

2. **Sin Autenticación**
   - Toda la aplicación es pública
   - Sin control de acceso a operaciones CRUD
   - **Acción**: Implementar ASP.NET Core Identity

3. **Sin HTTPS Enforcement**
   - Datos transmitidos en texto plano
   - **Acción**: Habilitar HTTPS y HSTS

## Plan de Migración por Fases

### Fase 1: Seguridad Crítica (1-2 semanas)

#### 1.1 Gestión de Secretos
```csharp
// Reemplazar credenciales hardcodeadas con:
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

// Para producción:
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### 1.2 Implementar Autenticación
- Agregar ASP.NET Core Identity
- Crear roles: Admin, Employee, Customer
- Proteger endpoints CRUD con `[Authorize]`
- Implementar registro y login de usuarios

#### 1.3 Configurar HTTPS
```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseHsts();

// Configurar headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### Fase 2: Calidad de Código (2-3 semanas)

#### 2.1 Refactoring de Servicios
- Crear interfaces para todos los servicios
- Implementar patrón Repository genérico
- Reemplazar `Console.WriteLine` con `ILogger`
- Estandarizar idioma (inglés) en código

#### 2.2 Manejo de Errores
```csharp
// Implementar middleware global de manejo de errores
public class GlobalExceptionMiddleware
{
    // Capturar y loggear excepciones
    // Retornar respuestas apropiadas al usuario
}
```

#### 2.3 Validación de Datos
- Implementar FluentValidation
- Validación en capa de servicios
- Mensajes de error localizados

### Fase 3: Funcionalidades B2B Esenciales (3-4 semanas)

#### 3.1 Gestión de Proveedores y Marcas
```csharp
// Nuevas entidades
public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; }
    public virtual ICollection<Product> Products { get; set; }
}

public class Supplier
{
    public int Id { get; set; }
    public string BusinessName { get; set; }
    public string TaxId { get; set; }
    public string ContactEmail { get; set; }
    public int PaymentTermsDays { get; set; }
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
}
```

#### 3.2 Sistema de Órdenes de Compra
- CRUD de órdenes de compra a proveedores
- Recepción de mercancía con verificación
- Actualización automática de inventario general
- Gestión de backorders

#### 3.3 Multi-Bodega
```csharp
public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public bool IsMainWarehouse { get; set; }
}

// Modificar inventario para incluir ubicación
public class InventoryLocation
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
}
```

#### 3.4 Sistema de Control de Mermas
```csharp
public class Shrinkage // Merma
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public ShrinkageType Type { get; set; }
    public string Reason { get; set; }
    public string AuthorizedBy { get; set; }
    public decimal CostValue { get; set; }
    public string BatchNumber { get; set; }
    public virtual Product Product { get; set; }
    public virtual Warehouse Warehouse { get; set; }
}

public enum ShrinkageType
{
    Expiration,      // Caducidad
    Damage,          // Daño
    Theft,           // Robo
    Loss,            // Pérdida
    Quality,         // Problemas de calidad
    Temperature,     // Ruptura cadena de frío
    Administrative,  // Ajuste administrativo
    Sample,          // Muestras
    Donation         // Donación
}

// Servicio para gestión de mermas
public interface IShrinkageService
{
    Task<Shrinkage> RegisterShrinkageAsync(ShrinkageDto shrinkage);
    Task<IEnumerable<Shrinkage>> GetShrinkageByPeriodAsync(DateTime start, DateTime end);
    Task<ShrinkageReport> GenerateShrinkageReportAsync(int warehouseId, DateTime start, DateTime end);
    Task<decimal> CalculateShrinkagePercentageAsync(int productId, DateTime start, DateTime end);
}
```

**Funcionalidades de control de mermas:**
- Registro detallado por tipo de merma
- Autorización requerida para mermas superiores a cierto monto
- Trazabilidad por lote afectado
- Reportes de mermas por período, producto, bodega
- Alertas automáticas por mermas excesivas
- Integración con inventario para ajustes automáticos
- Análisis de tendencias de mermas

### Fase 4: Performance y Escalabilidad (2-3 semanas)

#### 3.1 Optimización de Base de Datos
- Implementar paginación en todas las consultas
- Agregar índices necesarios
- Usar `AsNoTracking()` para lecturas
- Implementar caché con Redis

#### 3.2 Arquitectura Mejorada
```
VHouse.sln
├── VHouse.Domain (Entidades y lógica de negocio)
├── VHouse.Application (Servicios y DTOs)
├── VHouse.Infrastructure (EF Core, Repositorios)
├── VHouse.Web (Blazor UI)
├── VHouse.API (REST API opcional)
└── VHouse.Tests (Unit e Integration tests)
```

#### 3.3 Testing
- Agregar xUnit para pruebas unitarias
- Implementar pruebas de integración
- Configurar CI/CD con GitHub Actions
- Objetivo: 80% cobertura de código

### Fase 5: Características Avanzadas de Distribución (3-4 semanas)

#### 5.1 Gestión de Crédito y Cobranza
```csharp
public class CustomerCredit
{
    public int CustomerId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
}

// Sistema de estados de cuenta
// Alertas de cobranza
// Bloqueo automático por límite de crédito
```

#### 5.2 Sistema de Rutas y Entregas
- Gestión de rutas de entrega
- Asignación de pedidos a rutas
- Tracking de entregas
- Optimización de rutas

#### 5.3 Precios y Promociones por Cliente
- Listas de precios personalizadas
- Descuentos por volumen
- Promociones temporales
- Gestión de bonificaciones

### Fase 6: Características de Producción (3-4 semanas)

#### 4.1 Monitoreo y Observabilidad
- Implementar Application Insights
- Agregar Health Checks
- Configurar Serilog con sinks apropiados
- Implementar distributed tracing

#### 4.2 Localización
- Implementar IStringLocalizer
- Crear archivos de recursos (es-MX, en-US)
- UI multiidioma

#### 4.3 Características Adicionales
- Sistema de notificaciones por email
- Reportes y analytics
- Backup automático de base de datos
- Rate limiting para APIs

### Fase 7: Infraestructura y DevOps (2-3 semanas)

#### 5.1 Configuración de Ambientes
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DB_CONNECTION_STRING}"
  },
  "ApplicationInsights": {
    "ConnectionString": "${APPINSIGHTS_CONNECTION_STRING}"
  }
}
```

#### 5.2 CI/CD Pipeline
```yaml
# .github/workflows/deploy.yml
name: Build and Deploy
on:
  push:
    branches: [main]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Test
        run: dotnet test
      - name: Build
        run: dotnet publish -c Release
      - name: Deploy to Azure
        # Configuración de despliegue
```

#### 5.3 Infraestructura como Código
- Terraform/Bicep para recursos Azure
- Configuración de App Service/AKS
- Azure SQL Database con geo-replicación
- Azure CDN para assets estáticos

## Módulos Específicos para Distribución B2B

### 1. Módulo de Compras
- Catálogo de proveedores
- Gestión de órdenes de compra
- Recepción de mercancía
- Control de facturas de compra
- Historial de precios de compra

### 2. Módulo de Ventas B2B
- Catálogo de clientes mayoristas
- Listas de precios por cliente
- Pedidos con múltiples direcciones de entrega
- Gestión de backorders
- Pre-ventas y apartados

### 3. Módulo de Inventarios Avanzado
- Control multi-bodega
- Transferencias entre bodegas
- Inventarios cíclicos
- Trazabilidad por lote
- **Gestión integral de mermas:**
  - Registro por tipo (caducidad, daño, robo, etc.)
  - Control por lotes y fechas de caducidad
  - Autorización electrónica para mermas
  - Reportes de pérdidas por período
  - Análisis de causas y tendencias
  - Integración con contabilidad
- Consignación y comodatos

### 4. Módulo de Logística
- Planificación de rutas
- Control de flota (opcional)
- Documentos de entrega
- Confirmación de entregas
- Gestión de devoluciones

### 5. Módulo Financiero
- Control de créditos
- Estados de cuenta
- Conciliación de pagos
- Reportes de cartera
- Comisiones de ventas

### 6. Módulo de Reportes y BI
- Ventas por marca/categoría
- Rotación de inventarios
- Análisis ABC de productos
- Rentabilidad por cliente
- Proyecciones de demanda
- KPIs de distribución
- **Reportes de mermas:**
  - Mermas por tipo y causa
  - Mermas por producto/marca
  - Análisis de costos de mermas
  - Comparativo mermas vs. ventas
  - Tendencias históricas

## Checklist Pre-Producción para Sistema B2B

### Seguridad
- [ ] Remover todas las credenciales hardcodeadas
- [ ] Implementar autenticación y autorización
- [ ] Configurar HTTPS y headers de seguridad
- [ ] Auditoría de seguridad completa
- [ ] Implementar WAF (Web Application Firewall)

### Código
- [ ] 100% de servicios con interfaces
- [ ] Sin Console.WriteLine en el código
- [ ] Manejo de errores consistente
- [ ] Validación completa de datos
- [ ] Código en inglés estándar

### Performance
- [ ] Paginación implementada
- [ ] Caché configurado
- [ ] Queries optimizados
- [ ] Load testing completado
- [ ] CDN para assets

### Testing
- [ ] >80% cobertura de tests
- [ ] Tests de integración
- [ ] Tests de carga
- [ ] Tests de seguridad
- [ ] UAT completado

### Infraestructura
- [ ] Alta disponibilidad configurada
- [ ] Backups automáticos
- [ ] Monitoreo completo
- [ ] Alertas configuradas
- [ ] Plan de recuperación ante desastres

### Documentación
- [ ] README completo
- [ ] API documentada
- [ ] Manual de usuario
- [ ] Runbook de operaciones
- [ ] Documentación de arquitectura

### Funcionalidades B2B
- [ ] Gestión completa de proveedores
- [ ] Sistema de órdenes de compra
- [ ] Multi-bodega implementado
- [ ] Control de créditos activo
- [ ] Trazabilidad por lotes
- [ ] Sistema de control de mermas implementado
- [ ] Reportes gerenciales

## Estimación de Tiempo y Recursos

### Timeline Total: 14-18 semanas

- **Fase 1**: 1-2 semanas (CRÍTICO - Seguridad)
- **Fase 2**: 2-3 semanas (Calidad de código)
- **Fase 3**: 3-4 semanas (Funcionalidades B2B)
- **Fase 4**: 2-3 semanas (Performance)
- **Fase 5**: 3-4 semanas (Features avanzadas)
- **Fase 6**: 3-4 semanas (Características producción)
- **Fase 7**: 2-3 semanas (DevOps)

### Recursos Recomendados
- 3-4 desarrolladores senior
- 1 arquitecto de software
- 1 especialista en logística/supply chain
- 1 DevOps engineer
- 1 QA engineer
- 1 analista de negocio con experiencia en distribución

### Costos Estimados (Mensual en Producción)
- **Azure App Service**: $100-300 USD
- **Azure SQL Database**: $150-500 USD
- **Application Insights**: $50-100 USD
- **Azure Key Vault**: $10-20 USD
- **CDN**: $50-100 USD
- **Total**: ~$360-1020 USD/mes

## Recomendaciones Finales para Distribución B2B

1. **Priorizar seguridad y multi-usuario** - Crítico para operación con múltiples empleados
2. **Implementar trazabilidad completa** - Esencial para productos alimenticios
3. **Enfocarse en automatización** - Reducir trabajo manual en procesos repetitivos
4. **Integración con sistemas externos** - SAT, bancos, transportistas
5. **Escalabilidad horizontal** - Prepararse para crecimiento de SKUs y clientes
6. **Mobile-first para vendedores** - App móvil para toma de pedidos en ruta
7. **Analytics en tiempo real** - Dashboards para toma de decisiones

## Ventajas Competitivas del Sistema

1. **Inventario por cliente**: Único para gestión de consignaciones
2. **Múltiples niveles de precio**: Flexibilidad comercial
3. **Procesamiento con IA**: Agilidad en captura de pedidos
4. **Especializado en productos veganos**: Conocimiento del nicho
5. **Arquitectura moderna**: Fácil mantenimiento y actualización

## Próximos Pasos Inmediatos

1. Crear branch `feature/security-phase-1`
2. Remover credenciales hardcodeadas
3. Configurar variables de entorno locales
4. Implementar autenticación básica
5. Configurar HTTPS

Este plan transforma el MVP actual en un sistema robusto de gestión de inventarios para distribuidores B2B, incorporando las mejores prácticas de la industria de distribución mayorista mientras mantiene las fortalezas únicas del sistema actual. El enfoque en la gestión multi-marca y multi-cliente posicionará a la empresa como líder en distribución de productos veganos con tecnología de punta.