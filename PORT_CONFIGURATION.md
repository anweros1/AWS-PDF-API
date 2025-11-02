# 🔌 Port Configuration Guide

## Quick Reference

### **Default Ports**

| Environment | Port | URL | Notes |
|-------------|------|-----|-------|
| **Local HTTP** | 5018 | `http://localhost:5018` | ✅ Default |
| **Local HTTPS** | 7173 | `https://localhost:7173` | Optional |
| **Docker** | 5018 | `http://localhost:5018` | Mapped from 8080 |
| **Swagger UI** | 5018 | `http://localhost:5018/swagger` | Interactive docs |

---

## 🚀 Running the API

### **Method 1: Default (HTTP - Recommended)**

```bash
dotnet run --project API-PDF
```

**Result**:
```
Now listening on: http://localhost:5018
Application started. Press Ctrl+C to shut down.
```

✅ **Always port 5018**  
✅ **No certificate warnings**  
✅ **Swagger**: http://localhost:5018/swagger

---

### **Method 2: With HTTPS**

```bash
dotnet run --project API-PDF --launch-profile API-PDF-HTTPS
```

**Result**:
```
Now listening on: https://localhost:7173
Now listening on: http://localhost:5018
```

⚠️ May show certificate warning (safe to ignore in dev)

---

### **Method 3: Docker**

```bash
docker-compose up -d
```

**Result**:
- API: `http://localhost:5018`
- SQL Server: `localhost:1433`

---

## 📝 Configuration Files

### **launchSettings.json**

```json
{
  "profiles": {
    "API-PDF": {
      "applicationUrl": "http://localhost:5018"
    },
    "API-PDF-HTTPS": {
      "applicationUrl": "https://localhost:7173;http://localhost:5018"
    }
  }
}
```

### **docker-compose.yml**

```yaml
services:
  pdf-api:
    ports:
      - "5018:8080"  # Host:Container
```

---

## 🔧 Changing Ports

### **For Local Development**

Edit: `API-PDF/Properties/launchSettings.json`

```json
{
  "profiles": {
    "API-PDF": {
      "applicationUrl": "http://localhost:YOUR_PORT"
    }
  }
}
```

### **For Docker**

Edit: `docker-compose.yml`

```yaml
services:
  pdf-api:
    ports:
      - "YOUR_PORT:8080"
```

---

## ✅ Why Port 5018?

1. **Consistent**: Same port every time
2. **Memorable**: Easy to remember
3. **Available**: Unlikely to conflict
4. **Standard**: Common for .NET APIs

---

## 🆘 Troubleshooting

### **Port Already in Use**

```bash
# Windows: Find process using port 5018
netstat -ano | findstr :5018

# Kill the process
taskkill /PID <process_id> /F
```

### **Want Different Port**

```bash
# Run with custom port
dotnet run --project API-PDF --urls "http://localhost:YOUR_PORT"
```

### **Docker Port Issues**

```bash
# Stop all containers
docker-compose down

# Restart
docker-compose up -d
```

---

## 📊 Port Summary

**HTTP (Default)**: 5018  
**HTTPS (Optional)**: 7173  
**Docker Internal**: 8080  
**SQL Server**: 1433

---

**Last Updated**: October 29, 2025  
**Status**: ✅ Configured
