# Database Admin Console (Adminer)

## Why
Quick, zero-config database browsing (tables, schema, data) for development and support without exposing the cluster. Essential for debugging VHouse's client inventory, order processing, and business metrics.

## Local Development (Docker)

### Quick Start
```bash
# Start database admin stack
docker compose -f docker-compose.dbadmin.yml up -d

# Open Adminer in browser
start http://localhost:8080
```

### Configuration
- **URL**: `http://localhost:8080`
- **System**: PostgreSQL (for production) or SQLite (for local development)
- **Server**: `db` (when using PostgreSQL container)
- **Username**: `vhouse` (default) or from your `.env` file
- **Password**: `vhouse` (default) or from your `.env` file
- **Database**: `vhouse`

### Environment Variables
Create a `.env` file in the root directory:
```bash
# Database Configuration
DB_USER=vhouse
DB_PASSWORD=your_secure_password
DB_NAME=vhouse
DB_PORT=5432
ADMINER_PORT=8080

# For SQLite Admin (alternative)
SQLITE_ADMIN_PASSWORD=your_admin_password
SQLITE_ADMIN_PORT=8081
SQLITE_DB_PATH=./VHouse.Web
```

### SQLite Alternative
For local SQLite development, uncomment the `phpliteadmin` service in `docker-compose.dbadmin.yml`:
```bash
# Access SQLite admin
start http://localhost:8081
# Password: set via SQLITE_ADMIN_PASSWORD
```

## Kubernetes Production

### Deploy Admin Console
```bash
# Deploy to your namespace
kubectl apply -n <your-namespace> -f k8s/db-admin.yaml

# Verify deployment
kubectl get pods -l app=adminer -n <your-namespace>
```

### Access via Port-Forward
```bash
# Forward port only when needed
kubectl -n <your-namespace> port-forward svc/adminer 8080:8080

# Open in browser
start http://localhost:8080
```

### Database Connection
- **System**: PostgreSQL/MySQL/SQL Server (based on your setup)
- **Server**: Value from `db-admin-config` ConfigMap (default: `postgresql`)
- **Username**: From your database secret
- **Password**: From your database secret
- **Database**: Your application database name

### Update Database Host
```bash
# Update ConfigMap if your database service name differs
kubectl patch configmap db-admin-config -n <your-namespace> --patch '{"data":{"DB_HOST":"your-db-service-name"}}'
```

## Security Best Practices

### Default Security Features
- ✅ **No public ingress** - Only localhost and port-forward access
- ✅ **Network policies** - Restricted network access
- ✅ **Resource limits** - Prevents resource exhaustion
- ✅ **Security context** - Runs as non-root user
- ✅ **ReadOnly filesystem** - Prevents tampering

### Recommended Practices
1. **Use read-only database users** for browsing and debugging
2. **Enable query logging** temporarily for audit trails
3. **Rotate credentials** if suspected exposure
4. **Tear down after use** to minimize attack surface
5. **Monitor access** through application logs

### Creating Read-Only User (PostgreSQL)
```sql
-- Create read-only user for admin console
CREATE USER vhouse_readonly WITH PASSWORD 'secure_password';
GRANT CONNECT ON DATABASE vhouse TO vhouse_readonly;
GRANT USAGE ON SCHEMA public TO vhouse_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO vhouse_readonly;
GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO vhouse_readonly;

-- For future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO vhouse_readonly;
```

## Cleanup

### Local Development
```bash
# Stop and remove containers
docker compose -f docker-compose.dbadmin.yml down

# Remove volumes (WARNING: destroys data)
docker compose -f docker-compose.dbadmin.yml down -v
```

### Kubernetes
```bash
# Remove admin console
kubectl delete -f k8s/db-admin.yaml -n <your-namespace>

# Verify cleanup
kubectl get pods -l app=adminer -n <your-namespace>
```

## Troubleshooting

### Common Issues

#### Cannot connect to database
- Verify database service is running: `kubectl get pods -l app=postgresql`
- Check ConfigMap DB_HOST value: `kubectl get configmap db-admin-config -o yaml`
- Verify credentials in database secret

#### Port-forward fails
- Check if port 8080 is already in use: `netstat -an | findstr 8080`
- Use alternative port: `kubectl port-forward svc/adminer 8081:8080`

#### Adminer UI loads but database connection fails
- Verify network policies allow database access
- Check database logs for connection attempts
- Ensure database accepts connections from pod IP range

### Health Checks
The main VHouse application provides health endpoints:
- **Liveness**: `GET /health/live` - Application health
- **Readiness**: `GET /health/ready` - Database connectivity
- **Full Health**: `GET /health` - Detailed health report (production only)

## Integration with VHouse

### Key Tables to Monitor
- `Products` - Client inventory items
- `Orders` - Customer orders and fulfillment
- `Customers` - Client base (Mona la Dona, Sano Market, etc.)
- `AspNetUsers` - Authentication and authorization
- `AuditLogs` - System activity tracking
- `BusinessMetrics` - Performance and adoption metrics

### Useful Queries
```sql
-- Check recent orders
SELECT * FROM Orders WHERE CreatedAt > NOW() - INTERVAL '24 hours' ORDER BY CreatedAt DESC;

-- Monitor client activity
SELECT CustomerId, COUNT(*) as OrderCount, MAX(CreatedAt) as LastOrder
FROM Orders
GROUP BY CustomerId
ORDER BY LastOrder DESC;

-- Audit trail for troubleshooting
SELECT * FROM AuditLogs WHERE CreatedAt > NOW() - INTERVAL '1 hour' ORDER BY CreatedAt DESC;
```

---

**Remember**: This is a powerful tool for debugging and monitoring VHouse's mission-critical systems. Use responsibly and always follow the principle of least privilege. Every query helps ensure Bernard can efficiently serve his clients and accelerate vegan adoption. 🌱