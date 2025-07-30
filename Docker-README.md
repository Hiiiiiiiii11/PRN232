# Drug Prevention System - Docker Deployment

This guide explains how to deploy the Drug Prevention System using Docker containers.

## 🏗️ Architecture

The system consists of 3 main services:
- **API**: Drug Prevention API (ASP.NET Core Web API)
- **UI**: Drug Prevention UI (ASP.NET Core Razor Pages)
- **Database**: SQL Server 2022 Express

## 📋 Prerequisites

- Docker Desktop installed
- Docker Compose installed
- At least 4GB RAM available for containers
- Ports 3000, 5000, and 1433 available on your system

## 🚀 Quick Start

### Windows
```bash
# Build and run all services
docker-build.bat

# Stop all services
docker-stop.bat
```

### Linux/Mac
```bash
# Make scripts executable (one time only)
chmod +x docker-build.sh docker-stop.sh

# Build and run all services
./docker-build.sh

# Stop all services
./docker-stop.sh
```

### Manual Docker Compose
```bash
# Build and start all services
docker-compose up --build -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f
```

## 🌐 Access Points

After successful deployment:

- **UI Application**: http://localhost:3000
- **API Documentation**: http://localhost:5000/swagger
- **Database**: localhost:1433
  - Username: `sa`
  - Password: `DrugPrevention@2024`
  - Database: `DrugUsePreventionDB`

## 📊 Container Details

| Service | Container Name | Port | Internal Port |
|---------|---------------|------|---------------|
| UI | drugprevention-ui | 3000 | 8082 |
| API | drugprevention-api | 5000 | 8080 |
| Database | drugprevention-db | 1433 | 1433 |

## 🔧 Configuration

### Environment Variables

#### API Container
- `ConnectionStrings__DefaultConnectionString`: Database connection
- `AppSettings__Token`: JWT secret key
- `MailSettings__*`: Email configuration

#### UI Container
- `ApiSettings__BaseUrl`: API base URL for container communication

### Persistent Data

The following data is persisted in Docker volumes:
- `sqlserver_data`: Database files
- `api_uploads`: API uploaded files
- `ui_uploads`: UI uploaded files (avatars, etc.)

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Check if database is ready
docker-compose logs db

# Wait for database to be fully started (look for "SQL Server is now ready")
```

### API Not Responding
```bash
# Check API logs
docker-compose logs api

# Verify API health
curl http://localhost:5000/swagger
```

### UI Cannot Connect to API
```bash
# Check network connectivity
docker network ls
docker network inspect drugprevention_drugprevention-network

# Verify API is accessible from UI container
docker-compose exec ui ping api
```

### Port Conflicts
If ports are already in use, modify `docker-compose.yml`:
```yaml
services:
  ui:
    ports:
      - "3001:8082"  # Change external port
  api:
    ports:
      - "5001:8080"  # Change external port
```

## 🔍 Monitoring

### View Real-time Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f ui
docker-compose logs -f db
```

### Container Status
```bash
# Check running containers
docker-compose ps

# Detailed container info
docker-compose top
```

### Resource Usage
```bash
# Monitor resource usage
docker stats
```

## 🔄 Updates and Maintenance

### Update Application
```bash
# Stop services
docker-compose down

# Pull latest code changes
git pull

# Rebuild and start
docker-compose up --build -d
```

### Database Backup
```bash
# Backup database
docker-compose exec db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "DrugPrevention@2024" \
  -Q "BACKUP DATABASE DrugUsePreventionDB TO DISK = '/var/opt/mssql/backup/drugprevention.bak'"
```

### Clean Up
```bash
# Remove all containers and images
docker-compose down --rmi all --volumes

# Remove unused Docker resources
docker system prune -a
```

## 🏷️ Production Deployment

For production deployment:

1. **Security**: Change default passwords and secrets
2. **SSL/TLS**: Set up HTTPS certificates
3. **Database**: Use external managed database
4. **Monitoring**: Add application monitoring
5. **Backup**: Implement automated backup strategy

### Environment-specific Configuration

Create `docker-compose.prod.yml` for production:
```yaml
version: '3.8'
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AppSettings__Token=your-production-secret-key
  ui:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

Run with:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 📞 Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review container logs for error messages
3. Ensure all prerequisites are met
4. Verify port availability

## 📝 Notes

- First startup may take 2-3 minutes for database initialization
- The system automatically creates the database schema on first run
- Container-to-container communication uses internal networking
- External access uses mapped ports (3000, 5000, 1433)