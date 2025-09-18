# 🔒 Security Documentation VHouse

## 🎯 Security Philosophy

VHouse maneja datos sensibles de clientes reales (Mona la Dona, Sano Market, La Papelería) y debe proteger su información como protegemos a los animales: con dedicación absoluta.

## 🛡️ Threat Model

### Assets to Protect
1. **Customer Data**: Información de clientes y pedidos
2. **Business Logic**: Algoritmos de pricing y recomendaciones
3. **API Keys**: OpenAI, payment processors, etc.
4. **Infrastructure**: Kubernetes cluster, databases
5. **Source Code**: Intellectual property activista

### Threat Actors
1. **External Attackers**: Buscando datos de clientes
2. **Malicious Insiders**: Empleados con acceso indebido
3. **Supply Chain**: Dependencias comprometidas
4. **Infrastructure**: Cloud provider vulnerabilities

### Attack Vectors
1. **Web Application**: XSS, CSRF, SQL Injection
2. **API**: Authentication bypass, data exposure
3. **Container**: Image vulnerabilities, escape
4. **Network**: Man-in-the-middle, DDoS
5. **Social Engineering**: Phishing, credential theft

## 🔐 Authentication & Authorization

### Multi-tenancy Security
```csharp
// Tenant isolation at application level
public class TenantContext
{
    public string TenantId { get; set; }
    public string TenantName { get; set; } // "mona-la-dona", "sano-market"
}

// Query filter to ensure data isolation
modelBuilder.Entity<Product>()
    .HasQueryFilter(p => p.TenantId == CurrentTenant.Id);
```

### JWT Implementation
```csharp
// JWT Configuration with strong security
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // No tolerance for expired tokens
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
```

### Role-Based Access Control
```csharp
public enum Roles
{
    TenantAdmin,    // Full access to tenant data
    TenantUser,     // Limited access to tenant data
    SystemAdmin,    // Cross-tenant admin (Bernard only)
    ReadOnly        // View-only access
}

[Authorize(Roles = "TenantAdmin")]
public async Task<IActionResult> CreateProduct(CreateProductCommand command)
{
    // Ensure user can only create products for their tenant
    command.TenantId = CurrentUser.TenantId;
    return await mediator.Send(command);
}
```

## 🛡️ Data Protection

### Encryption at Rest
```yaml
# Database encryption (Azure PostgreSQL)
resource "azurerm_postgresql_flexible_server" "vhouse" {
  # Transparent Data Encryption enabled by default
  storage_mb = 32768

  # Customer-managed keys for additional security
  customer_managed_key {
    key_vault_key_id = azurerm_key_vault_key.vhouse.id
  }
}
```

### Encryption in Transit
```csharp
// Enforce HTTPS everywhere
app.UseHttpsRedirection();
app.UseHsts();

// Strong TLS configuration
services.Configure<HttpsRedirectionOptions>(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});
```

### Sensitive Data Handling
```csharp
// Never log sensitive information
public class SensitiveDataFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Remove sensitive fields from logs
        var sanitized = context.Result.SanitizeSensitiveData();
        logger.LogInformation("Action completed: {Result}", sanitized);
    }
}

// Mask sensitive data in responses
public class CustomerDto
{
    public string Id { get; set; }
    public string Name { get; set; }

    [JsonIgnore] // Never serialize in API responses
    public string TaxId { get; set; }

    public string Phone => MaskPhoneNumber(OriginalPhone);
}
```

## 🔍 Input Validation & Sanitization

### Command Validation
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidProductName).WithMessage("Invalid characters in product name");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(10000); // Reasonable business limit

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .Must(NotContainHtml).WithMessage("HTML not allowed in description");
    }

    private bool BeValidProductName(string name)
    {
        return !Regex.IsMatch(name, @"[<>""'&]"); // Prevent XSS
    }
}
```

### SQL Injection Prevention
```csharp
// Always use parameterized queries through EF Core
public async Task<List<Product>> GetProductsByCategory(string category, string tenantId)
{
    return await context.Products
        .Where(p => p.Category == category && p.TenantId == tenantId) // Safe parameterization
        .ToListAsync();
}

// For raw SQL (avoid when possible)
public async Task<List<Product>> GetProductsRaw(string tenantId)
{
    return await context.Products
        .FromSqlRaw("SELECT * FROM Products WHERE TenantId = {0}", tenantId)
        .ToListAsync();
}
```

## 🐳 Container Security

### Secure Dockerfile
```dockerfile
# Use specific version tags (not latest)
FROM mcr.microsoft.com/dotnet/aspnet:8.0.1-bookworm-slim

# Create non-root user
RUN adduser --disabled-password --gecos '' --uid 1000 appuser

# Set working directory
WORKDIR /app

# Copy application files
COPY --from=build --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose port (non-privileged)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "VHouse.Web.dll"]
```

### Kubernetes Security Context
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vhouse-api
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 1000
      containers:
      - name: vhouse-api
        securityContext:
          allowPrivilegeEscalation: false
          capabilities:
            drop:
            - ALL
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000
        volumeMounts:
        - name: tmp
          mountPath: /tmp
        - name: app-cache
          mountPath: /app/cache
      volumes:
      - name: tmp
        emptyDir: {}
      - name: app-cache
        emptyDir: {}
```

## 🔐 Secrets Management

### Azure Key Vault Integration
```csharp
// Key Vault configuration
builder.Configuration.AddAzureKeyVault(
    vaultUri: new Uri($"https://{keyVaultName}.vault.azure.net/"),
    credential: new DefaultAzureCredential());

// Usage in application
services.Configure<OpenAIOptions>(options =>
{
    options.ApiKey = builder.Configuration["OpenAI--ApiKey"]; // From Key Vault
});
```

### Kubernetes Secrets
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: vhouse-secrets
  namespace: vhouse
type: Opaque
stringData:
  database-connection: "Server=postgres.internal;Database=vhouse;User Id=app;Password=secure_password_123;"
  openai-api-key: "sk-secure-key-here"
  jwt-secret-key: "very-long-random-secret-key-256-bits"

---
# Secret reference in deployment
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
      - name: vhouse-api
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: vhouse-secrets
              key: database-connection
```

## 🔍 Security Monitoring

### Application Security Events
```csharp
public class SecurityEventLogger
{
    public void LogAuthenticationFailure(string username, string ipAddress)
    {
        logger.LogWarning("Authentication failed for {Username} from {IpAddress}",
            username, ipAddress);
    }

    public void LogSuspiciousActivity(string activity, string details)
    {
        logger.LogCritical("SECURITY: Suspicious activity detected: {Activity}. Details: {Details}",
            activity, details);
    }

    public void LogDataAccess(string userId, string resource, string action)
    {
        logger.LogInformation("Data access: User {UserId} performed {Action} on {Resource}",
            userId, action, resource);
    }
}
```

### Security Headers
```csharp
app.Use(async (context, next) =>
{
    // Security headers
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline'");

    await next();
});
```

## 🚨 Incident Response

### Security Incident Playbook

#### 1. Detection & Assessment (0-30 minutes)
```bash
# Check for indicators of compromise
kubectl logs -f deployment/vhouse-api -n vhouse | grep -i "error\|exception\|attack"

# Review recent access logs
kubectl get events -n vhouse --sort-by='.lastTimestamp'

# Check resource usage anomalies
kubectl top pods -n vhouse
```

#### 2. Containment (30-60 minutes)
```bash
# Isolate affected pods
kubectl scale deployment/vhouse-api --replicas=0 -n vhouse

# Block suspicious traffic (example)
kubectl apply -f - <<EOF
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: block-suspicious-ip
  namespace: vhouse
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  ingress:
  - from:
    - ipBlock:
        cidr: 0.0.0.0/0
        except:
        - 192.168.1.100/32  # Block this IP
EOF
```

#### 3. Investigation (1-4 hours)
```bash
# Collect logs for analysis
kubectl logs deployment/vhouse-api -n vhouse --previous > incident-logs.txt

# Export security events
kubectl get events -n vhouse -o json > security-events.json

# Database audit trail
psql -h $DB_HOST -U $DB_USER -c "SELECT * FROM audit_log WHERE created_at > NOW() - INTERVAL '24 hours';"
```

#### 4. Recovery (2-8 hours)
```bash
# Deploy clean version
kubectl set image deployment/vhouse-api vhouse-api=bernardouriza/vhouse:known-good -n vhouse

# Verify clean deployment
kubectl rollout status deployment/vhouse-api -n vhouse

# Restore normal operations
kubectl scale deployment/vhouse-api --replicas=3 -n vhouse
```

### Communication Plan
1. **Immediate**: Notify Bernard via SMS/WhatsApp
2. **15 minutes**: Brief email to stakeholders
3. **1 hour**: Detailed incident report
4. **24 hours**: Post-incident review

## 📋 Security Checklist

### Development
- [ ] All inputs validated and sanitized
- [ ] No secrets in source code
- [ ] Dependencies regularly updated
- [ ] SAST tools integrated
- [ ] Code reviews include security perspective

### Deployment
- [ ] HTTPS enforced everywhere
- [ ] Security headers configured
- [ ] Non-root containers
- [ ] Network policies applied
- [ ] Secrets properly managed

### Operations
- [ ] Security monitoring active
- [ ] Incident response plan tested
- [ ] Regular vulnerability scans
- [ ] Backup and recovery tested
- [ ] Security training current

---

**🔒 Security Activista**: Protegemos los datos como protegemos a los animales - con vigilancia constante y compasión absoluta. La seguridad no es opcional cuando clientes reales dependen de nosotros.