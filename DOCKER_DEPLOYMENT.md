# 🐳 Docker Deployment Guide

## Quick Start

### 1. **Clone and Configure**
```bash
# Clone repository
git clone <your-repo-url>
cd API-PDF

# Copy environment template
cp .env.example .env

# Edit .env with your values
nano .env
```

### 2. **Build and Run**
```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f pdf-api

# Check health
curl http://localhost:5018/api/s3/health
```

### 3. **Access API**
- **API**: http://localhost:5018
- **Swagger**: http://localhost:5018/swagger
- **SQL Server**: localhost:1433

---

## Configuration

### Environment Variables

The API supports configuration via environment variables. If not specified, it uses defaults from `appsettings.json`.

#### **Database** (Required)
```bash
DB_CONNECTION_STRING=Server=sqlserver;Database=PdfApiLogs;User Id=sa;Password=YourPassword;TrustServerCertificate=True
SQL_SA_PASSWORD=YourStrong@Passw0rd
```

#### **AWS S3** (Optional - uses local fallback if empty)
```bash
AWS_ACCESS_KEY=your-access-key
AWS_SECRET_KEY=your-secret-key
AWS_BUCKET_NAME=your-bucket-name
AWS_REGION=us-east-1
```

#### **PDF Settings** (Optional)
```bash
PDF_TEMP_FOLDER=/app/temp/pdfs
PDF_FALLBACK_FOLDER=/app/pdfs/fallback
PDF_MAX_FILE_SIZE_MB=100
PDF_AUTO_CREATE_FOLDERS=true
```

#### **Logging** (Optional)
```bash
LOG_LEVEL=Information
LOG_LEVEL_ASPNET=Warning
```

---

## Deployment Scenarios

### Scenario 1: Development (Local with Docker)

```bash
# Use docker-compose with SQL Server included
docker-compose up -d

# API runs on http://localhost:5018
# SQL Server on localhost:1433
```

**Features**:
- ✅ Includes SQL Server container
- ✅ Local file storage (volumes)
- ✅ Swagger UI enabled
- ✅ Easy debugging

---

### Scenario 2: Production (External Database)

```bash
# Create .env file
cat > .env << EOF
DB_CONNECTION_STRING=Server=your-prod-db.com;Database=PdfApiLogs;User Id=apiuser;Password=SecurePassword;TrustServerCertificate=True
AWS_ACCESS_KEY=your-access-key
AWS_SECRET_KEY=your-secret-key
AWS_BUCKET_NAME=prod-pdf-bucket
AWS_REGION=us-east-1
LOG_LEVEL=Warning
EOF

# Remove SQL Server from docker-compose
# Comment out the 'sqlserver' service and 'depends_on' in docker-compose.yml

# Start only API
docker-compose up -d pdf-api
```

**Features**:
- ✅ Uses external SQL Server
- ✅ Uses AWS S3 for storage
- ✅ Production logging
- ✅ No local SQL Server

---

### Scenario 3: Kubernetes Deployment

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pdf-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: pdf-api
  template:
    metadata:
      labels:
        app: pdf-api
    spec:
      containers:
      - name: pdf-api
        image: your-registry/pdf-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: pdf-api-secrets
              key: db-connection
        - name: AWS__AccessKey
          valueFrom:
            secretKeyRef:
              name: pdf-api-secrets
              key: aws-access-key
        - name: AWS__SecretKey
          valueFrom:
            secretKeyRef:
              name: pdf-api-secrets
              key: aws-secret-key
        - name: AWS__BucketName
          value: "prod-pdf-bucket"
        - name: PdfSettings__MaxFileSizeMb
          value: "100"
        volumeMounts:
        - name: temp-storage
          mountPath: /app/temp/pdfs
        - name: fallback-storage
          mountPath: /app/pdfs/fallback
      volumes:
      - name: temp-storage
        emptyDir: {}
      - name: fallback-storage
        persistentVolumeClaim:
          claimName: pdf-fallback-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: pdf-api-service
spec:
  selector:
    app: pdf-api
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```

---

## Docker Commands

### Build
```bash
# Build image
docker build -t pdf-api:latest .

# Build with specific tag
docker build -t pdf-api:v1.0.0 .
```

### Run
```bash
# Run with defaults
docker run -p 5018:8080 pdf-api:latest

# Run with environment variables
docker run -p 5018:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=PdfApiLogs;..." \
  -e AWS__AccessKey="your-key" \
  -e AWS__SecretKey="your-secret" \
  -e AWS__BucketName="your-bucket" \
  pdf-api:latest

# Run with volume mounts
docker run -p 5018:8080 \
  -v pdf-temp:/app/temp/pdfs \
  -v pdf-fallback:/app/pdfs/fallback \
  pdf-api:latest
```

### Manage
```bash
# View logs
docker logs pdf-api -f

# Execute commands in container
docker exec -it pdf-api bash

# Stop container
docker stop pdf-api

# Remove container
docker rm pdf-api

# Remove image
docker rmi pdf-api:latest
```

### Docker Compose
```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f

# Rebuild and restart
docker-compose up -d --build

# Scale API instances
docker-compose up -d --scale pdf-api=3

# Remove everything (including volumes)
docker-compose down -v
```

---

## Health Checks

### API Health
```bash
# Check S3 connectivity
curl http://localhost:5018/api/s3/health

# Expected response:
{
  "isAvailable": true,
  "status": "Connected",
  "message": "S3 service is available and connected",
  "timestamp": "2025-10-29T19:00:00Z"
}
```

### Container Health
```bash
# Check container health status
docker ps

# Should show "healthy" in STATUS column
```

---

## Troubleshooting

### Issue: Container won't start
```bash
# Check logs
docker logs pdf-api

# Common causes:
# 1. Database connection failed
# 2. Port already in use
# 3. Invalid environment variables
```

### Issue: Database connection failed
```bash
# If using docker-compose SQL Server:
# 1. Wait for SQL Server to be ready (takes ~30 seconds)
docker-compose logs sqlserver

# 2. Check connection string
docker exec -it pdf-api printenv | grep ConnectionStrings

# 3. Test connection from container
docker exec -it pdf-api curl http://sqlserver:1433
```

### Issue: S3 connection failed
```bash
# API will automatically fall back to local storage
# Check logs:
docker logs pdf-api | grep "S3"

# Verify AWS credentials:
docker exec -it pdf-api printenv | grep AWS
```

### Issue: Permission denied on volumes
```bash
# Fix permissions
docker exec -it pdf-api chmod -R 777 /app/temp
docker exec -it pdf-api chmod -R 777 /app/pdfs
```

---

## Production Checklist

### Before Deployment:
- [ ] Update `.env` with production values
- [ ] Use strong database password
- [ ] Configure AWS S3 credentials
- [ ] Set appropriate file size limits
- [ ] Configure logging level (Warning or Error)
- [ ] Enable HTTPS (use reverse proxy like nginx)
- [ ] Set up database backups
- [ ] Configure monitoring/alerting
- [ ] Test health checks
- [ ] Review security settings

### Security:
- [ ] Never commit `.env` file
- [ ] Use secrets management (Azure Key Vault, AWS Secrets Manager)
- [ ] Enable authentication/authorization
- [ ] Use HTTPS only
- [ ] Implement rate limiting
- [ ] Regular security updates
- [ ] Monitor logs for suspicious activity

---

## Monitoring

### Prometheus Metrics (Future Enhancement)
```yaml
# Add to docker-compose.yml
prometheus:
  image: prom/prometheus
  volumes:
    - ./prometheus.yml:/etc/prometheus/prometheus.yml
  ports:
    - "9090:9090"

grafana:
  image: grafana/grafana
  ports:
    - "3000:3000"
```

### Log Aggregation
```bash
# Send logs to external service
docker-compose logs -f | your-log-aggregator
```

---

## Backup & Recovery

### Database Backup
```bash
# Backup SQL Server database
docker exec pdf-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourPassword" \
  -Q "BACKUP DATABASE PdfApiLogs TO DISK='/var/opt/mssql/backup/PdfApiLogs.bak'"

# Copy backup out of container
docker cp pdf-sqlserver:/var/opt/mssql/backup/PdfApiLogs.bak ./backup/
```

### Volume Backup
```bash
# Backup PDF files
docker run --rm \
  -v pdf-fallback:/data \
  -v $(pwd)/backup:/backup \
  alpine tar czf /backup/pdf-fallback-$(date +%Y%m%d).tar.gz /data
```

---

## Performance Tuning

### Resource Limits
```yaml
# Add to docker-compose.yml
services:
  pdf-api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

### Scaling
```bash
# Run multiple instances behind load balancer
docker-compose up -d --scale pdf-api=3
```

---

## Support

### Logs Location
- **Container**: `/app/logs/` (if configured)
- **Host**: `docker logs pdf-api`
- **Database**: `ApiCallLogs` table

### Useful Commands
```bash
# Get container IP
docker inspect pdf-api | grep IPAddress

# Check resource usage
docker stats pdf-api

# View container details
docker inspect pdf-api
```

---

## Next Steps

1. ✅ Configure environment variables
2. ✅ Start services with `docker-compose up -d`
3. ✅ Test API endpoints
4. ✅ Set up monitoring
5. ✅ Configure backups
6. ✅ Deploy to production

---

**Last Updated**: 2025-10-29  
**Version**: 1.0.0  
**Docker Image**: pdf-api:latest
