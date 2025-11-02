# PDF Management API

A comprehensive ASP.NET Core Web API for PDF manipulation with AWS S3 storage integration.

## 🚀 Features

- **PDF Operations**:
  - Merge multiple PDFs
  - Add bookmarks
  - Add keywords/metadata
  - Fill form fields
  - Get page count
  - Validate PDF files
  - Extract bookmarks

- **Storage**:
  - AWS S3 integration
  - Local fallback storage
  - Automatic file management

- **Database Logging**:
  - SQL Server integration
  - Request/response logging
  - Error tracking

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0
- **PDF Library**: PdfSharp 6.2.2 (MIT License - Free!)
- **Cloud Storage**: AWS S3
- **Database**: SQL Server
- **Testing**: NUnit, Moq, FluentAssertions
- **Containerization**: Docker

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (or use Docker)
- AWS Account (Free Tier available)
- Docker (optional, for containerized deployment)

## ⚡ Quick Start

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd API-PDF
```

### 2. Configure Settings

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PdfApiLogs;..."
  },
  "AWS": {
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "BucketName": "your-bucket-name",
    "Region": "your-region"
  },
  "PdfSettings": {
    "TempFolder": "C:\\Temp\\PDFs",
    "LocalFallbackFolder": "C:\\PDFs\\Fallback"
  }
}
```

### 3. Run the API

```bash
dotnet run --project API-PDF
```

API will be available at: `http://localhost:5018`

### 4. Test the API

```bash
# Health check
curl http://localhost:5018/api/s3/health

# Swagger UI
http://localhost:5018/swagger
```

## 🐳 Docker Deployment

### Using Docker Compose:

```bash
# 1. Create .env file
cp .env.example .env
# Edit .env with your credentials

# 2. Start services
docker-compose up -d

# 3. Check logs
docker-compose logs -f pdf-api
```

See [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md) for detailed instructions.

## 📚 API Endpoints

### PDF Operations

- `POST /api/pdf/upload` - Upload PDF from URL
- `POST /api/pdf/upload-from-file` - Upload PDF from file
- `POST /api/pdf/merge` - Merge multiple PDFs
- `POST /api/pdf/add-bookmarks` - Add bookmarks to PDF
- `POST /api/pdf/add-keywords` - Add keywords to PDF
- `POST /api/pdf/assign-variables` - Fill form fields
- `GET /api/pdf/page-count/{guid}` - Get page count

### S3 Operations

- `GET /api/s3/health` - Check S3 connection
- `GET /api/s3/exists/{guid}` - Check if file exists
- `GET /api/s3/download/{guid}` - Download PDF
- `DELETE /api/s3/{guid}` - Delete PDF

## 🧪 Testing

### Run Unit Tests

```bash
dotnet test
```

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Current coverage: **79/79 tests passing** ✅

## 💰 Cost Savings

**Migrated from iText7 to PdfSharp**:
- **Before**: $1,500-$5,000/year (iText7 commercial license)
- **After**: $0/year (PdfSharp MIT license)
- **Savings**: $7,500-$25,000 over 5 years

## 🔒 Security

- AWS credentials stored in environment variables
- `.env` file excluded from Git
- Block all public access on S3 bucket
- IAM user with minimal permissions
- SQL Server with trusted connection

## 📖 Documentation

- [Docker Deployment Guide](DOCKER_DEPLOYMENT.md) - Complete Docker setup
- [Test Plan](TEST_PLAN.md) - Testing with local PDF files
- Development docs in `docs/` folder (gitignored)

## 🆓 AWS Free Tier

The API uses AWS S3 which includes:
- 5 GB storage (12 months free)
- 20,000 GET requests/month
- 2,000 PUT requests/month

Perfect for testing and small-scale production!

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📝 License

This project uses:
- **PdfSharp**: MIT License (Free for commercial use)
- **AWS SDK**: Apache 2.0 License

## 🆘 Support

For issues and questions:
1. Check the documentation in `docs/` folder
2. Review [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md)
3. Check [TEST_PLAN.md](TEST_PLAN.md) for testing examples

## ✅ Project Status

- ✅ All 79 unit tests passing
- ✅ Zero licensing costs
- ✅ Production-ready
- ✅ Docker-ready
- ✅ AWS S3 integrated
- ✅ Comprehensive logging

## 🎯 Version

**Current Version**: 1.0.0  
**Last Updated**: October 29, 2025  
**Status**: Production Ready ✅
