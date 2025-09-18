# 🚀 Guía de Deployment VHouse

## 🐳 Docker Compose (Desarrollo Local)

### Stack Completo
```yaml
# docker-compose.yml
version: '3.8'
services:
  vhouse-web:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres;Database=vhouse;User Id=postgres;Password=postgres123;
    depends_on:
      - postgres
      - redis

  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=vhouse
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - vhouse-web

volumes:
  postgres_data:
  redis_data:
```

### Comandos Docker
```bash
# Levantar stack completo
docker-compose up -d

# Ver logs
docker-compose logs -f vhouse-web

# Rebuild y restart
docker-compose up --build -d

# Limpiar todo
docker-compose down -v
docker system prune -f
```

## ☸️ Kubernetes (Producción)

### Namespace y ConfigMaps
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: vhouse
  labels:
    name: vhouse

---
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: vhouse-config
  namespace: vhouse
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Logging__LogLevel__Default: "Information"
  AllowedHosts: "*"
```

### Database Deployment
```yaml
# k8s/postgres.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: vhouse
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:15-alpine
        env:
        - name: POSTGRES_DB
          value: "vhouse"
        - name: POSTGRES_USER
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: username
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc

---
apiVersion: v1
kind: Service
metadata:
  name: postgres-service
  namespace: vhouse
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### Application Deployment
```yaml
# k8s/vhouse-app.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vhouse-api
  namespace: vhouse
spec:
  replicas: 3
  selector:
    matchLabels:
      app: vhouse-api
  template:
    metadata:
      labels:
        app: vhouse-api
    spec:
      containers:
      - name: vhouse-api
        image: bernardouriza/vhouse:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        envFrom:
        - configMapRef:
            name: vhouse-config
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
apiVersion: v1
kind: Service
metadata:
  name: vhouse-service
  namespace: vhouse
spec:
  selector:
    app: vhouse-api
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP
```

### Ingress Configuration
```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: vhouse-ingress
  namespace: vhouse
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.vhouse.app
    secretName: vhouse-tls
  rules:
  - host: api.vhouse.app
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: vhouse-service
            port:
              number: 80
```

### Deployment Commands
```bash
# Apply all manifests
kubectl apply -f k8s/

# Check deployment status
kubectl get pods -n vhouse

# View logs
kubectl logs -f deployment/vhouse-api -n vhouse

# Scale deployment
kubectl scale deployment/vhouse-api --replicas=5 -n vhouse

# Rolling update
kubectl set image deployment/vhouse-api vhouse-api=bernardouriza/vhouse:v1.2.0 -n vhouse

# Rollback
kubectl rollout undo deployment/vhouse-api -n vhouse
```

## 🏗️ Terraform (Infrastructure as Code)

### Provider Configuration
```hcl
# terraform/main.tf
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "vhouse-terraform"
    storage_account_name = "vhouseterraform"
    container_name       = "tfstate"
    key                  = "prod.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}
```

### Resource Groups
```hcl
# terraform/resource-groups.tf
resource "azurerm_resource_group" "vhouse" {
  name     = "vhouse-${var.environment}"
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "VHouse"
    Purpose     = "AnimalLiberation"
  }
}
```

### AKS Cluster
```hcl
# terraform/aks.tf
resource "azurerm_kubernetes_cluster" "vhouse" {
  name                = "vhouse-aks-${var.environment}"
  location            = azurerm_resource_group.vhouse.location
  resource_group_name = azurerm_resource_group.vhouse.name
  dns_prefix          = "vhouse-${var.environment}"

  default_node_pool {
    name       = "default"
    node_count = var.node_count
    vm_size    = "Standard_D2_v2"
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin = "azure"
  }

  tags = {
    Environment = var.environment
  }
}
```

### Database
```hcl
# terraform/database.tf
resource "azurerm_postgresql_flexible_server" "vhouse" {
  name                   = "vhouse-db-${var.environment}"
  resource_group_name    = azurerm_resource_group.vhouse.name
  location              = azurerm_resource_group.vhouse.location
  version               = "15"
  administrator_login    = var.db_admin_username
  administrator_password = var.db_admin_password
  zone                  = "1"

  storage_mb = 32768
  sku_name   = "GP_Standard_D2s_v3"

  backup_retention_days = 7
}

resource "azurerm_postgresql_flexible_server_database" "vhouse" {
  name      = "vhouse"
  server_id = azurerm_postgresql_flexible_server.vhouse.id
  collation = "en_US.utf8"
  charset   = "utf8"
}
```

### Terraform Commands
```bash
# Initialize
terraform init

# Plan changes
terraform plan -var-file="environments/prod.tfvars"

# Apply changes
terraform apply -var-file="environments/prod.tfvars"

# Destroy (careful!)
terraform destroy -var-file="environments/prod.tfvars"
```

## 🔧 Environment Configuration

### Development (.env.development)
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=vhouse_dev;User Id=postgres;Password=postgres123;
Redis__ConnectionString=localhost:6379
OpenAI__ApiKey=sk-dev-key
Logging__LogLevel__Default=Debug
```

### Staging (.env.staging)
```bash
ASPNETCORE_ENVIRONMENT=Staging
ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
Redis__ConnectionString=${REDIS_CONNECTION_STRING}
OpenAI__ApiKey=${OPENAI_API_KEY}
Logging__LogLevel__Default=Information
```

### Production (Kubernetes Secrets)
```yaml
# k8s/secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-secret
  namespace: vhouse
type: Opaque
data:
  connection-string: <base64-encoded-connection-string>

---
apiVersion: v1
kind: Secret
metadata:
  name: api-keys
  namespace: vhouse
type: Opaque
data:
  openai-key: <base64-encoded-openai-key>
  jwt-secret: <base64-encoded-jwt-secret>
```

## 🚀 Deployment Strategies

### Blue-Green Deployment
```bash
# Deploy to green environment
kubectl apply -f k8s/green/

# Verify green health
kubectl get pods -l version=green -n vhouse

# Switch traffic
kubectl patch service vhouse-service -n vhouse -p '{"spec":{"selector":{"version":"green"}}}'

# Cleanup blue
kubectl delete -f k8s/blue/
```

### Canary Deployment
```yaml
# 10% traffic to new version
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: vhouse-rollout
spec:
  replicas: 5
  strategy:
    canary:
      steps:
      - setWeight: 10
      - pause: {}
      - setWeight: 50
      - pause: {duration: 10m}
      - setWeight: 100
```

## 📊 Monitoring

### Health Checks
```csharp
// Program.cs
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Prometheus Metrics
```yaml
# k8s/monitoring.yaml
apiVersion: v1
kind: Service
metadata:
  name: vhouse-metrics
  annotations:
    prometheus.io/scrape: "true"
    prometheus.io/port: "8080"
    prometheus.io/path: "/metrics"
spec:
  selector:
    app: vhouse-api
  ports:
  - port: 8080
```

---

**🎯 Deploy Activista**: Cada deployment debe ser confiable y rápido. Los clientes como Mona la Dona dependen de que el sistema funcione 24/7 para servir productos veganos.