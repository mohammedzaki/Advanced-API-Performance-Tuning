# Environment Variables Configuration

This project has been fully configured to use environment variables for all ports, URLs, and configuration settings. This provides flexibility for different deployment environments and easier container orchestration.

## Quick Start

All environment variables are defined in the `.env` file with sensible defaults. You can:

1. **Use defaults**: Run `docker-compose up` without any changes
2. **Override specific values**: Edit the `.env` file or set environment variables before running
3. **Runtime overrides**: Use `docker-compose` environment variable syntax

## Key Environment Variables

### Service Ports (Host side - for external access)
```bash
SPRING_PORT=8081                  # Spring Boot service
DOTNET_REST_PORT=8080            # .NET REST API  
DOTNET_GRPC_PORT=8083            # .NET gRPC service
GRPC_CLIENT_TEST_PORT=8084       # gRPC client test service
```

### Container Internal Ports (Inside Docker network)
```bash
DOTNET_CONTAINER_HTTP_PORT=8080  # .NET HTTP inside container
DOTNET_CONTAINER_GRPC_PORT=8083  # .NET gRPC inside container
SPRING_CONTAINER_PORT=8080       # Spring Boot inside container
```

### Performance Testing
```bash
K6_BASE_URL=http://dotnet-app:8080   # Target for K6 tests (Docker internal)
K6_LOCALHOST_URL=http://localhost:8080  # Target for local K6 tests
K6_CONCURRENT_USERS=100          # Number of virtual users
K6_DURATION=30s                  # Test duration
K6_SLEEP_DURATION=0.2           # Sleep between requests
```

### Observability Stack
```bash
JAEGER_UI_PORT=16686            # Jaeger UI
PROMETHEUS_PORT=9090            # Prometheus
GRAFANA_PORT=3000              # Grafana dashboard
OTEL_COLLECTOR_HTTP_PORT=4318  # OpenTelemetry collector
```

### Database Configuration
```bash
SQL_SERVER_HOST=mssql           # Database host
SQL_SERVER_PORT=1433           # Database port
SQL_SERVER_DATABASE=AdventureWorks2022
SQL_SERVER_USER=sa
SQL_SERVER_PASSWORD=YourStrong!Passw0rd
```

## Usage Examples

### 1. Default Configuration
```bash
# Use all defaults from .env file
docker-compose up
```

### 2. Override Specific Ports
```bash
# Change .NET REST port to 9080
export DOTNET_REST_PORT=9080
docker-compose up

# Or directly in docker-compose command
DOTNET_REST_PORT=9080 docker-compose up
```

### 3. Local Development vs Container
```bash
# For local .NET development (outside Docker)
export DOTNET_REST_PORT=5000
export DOTNET_GRPC_PORT=5001

# For K6 testing against local service  
export K6_BASE_URL=http://localhost:5000
```

### 4. Different Environments
```bash
# Production-like settings
export ASPNETCORE_ENVIRONMENT=Production
export K6_CONCURRENT_USERS=500
export K6_DURATION=300s

# Development settings
export ASPNETCORE_ENVIRONMENT=Development
export K6_CONCURRENT_USERS=10
export K6_DURATION=30s
```

## Service Access URLs

After running `docker-compose up`, access services at:

- **.NET REST API**: `http://localhost:${DOTNET_REST_PORT:-8080}`
- **.NET gRPC**: `http://localhost:${DOTNET_GRPC_PORT:-8083}`  
- **Spring Boot**: `http://localhost:${SPRING_PORT:-8081}`
- **gRPC Client Test**: `http://localhost:${GRPC_CLIENT_TEST_PORT:-8084}`
- **React Dashboard**: `http://localhost:3001`
- **Jaeger UI**: `http://localhost:${JAEGER_UI_PORT:-16686}`
- **Prometheus**: `http://localhost:${PROMETHEUS_PORT:-9090}`
- **Grafana**: `http://localhost:${GRAFANA_PORT:-3000}`

## K6 Performance Testing

The K6 scripts now support environment variables:

```bash
# Run baseline test with defaults
docker-compose run k6

# Run with custom parameters
docker-compose run -e VUS=200 -e DURATION=60s -e BASE_URL=http://dotnet-app:8080 k6

# Local K6 testing (outside Docker)
export BASE_URL=http://localhost:8080
export VUS=50
export DURATION=30s
k6 run k6/baseline.js
```

## Configuration Files

The following files are now environment-variable aware:
- `docker-compose.yml` - All service ports and configurations
- `k6/baseline.js` & `k6/report-performance-testing.js` - Test parameters  
- `.NET appsettings.json` - Database and service configuration
- `Spring application.properties` - Server and OpenTelemetry configuration
- `React apiService.js` - API endpoint configuration (already was)

## Development Tips

1. **Port Conflicts**: Change host ports in `.env` if you have conflicts
2. **Service Discovery**: Use container names (`dotnet-app`, `spring-app`) for inter-service communication
3. **Health Checks**: All services expose health endpoints for monitoring
4. **Logs**: Use `docker-compose logs <service-name>` to view service logs
5. **Environment**: Set `ASPNETCORE_ENVIRONMENT=Development` for enhanced logging

## Troubleshooting

1. **Port already in use**: Change the host port in `.env`
2. **Service not accessible**: Check if the service is running: `docker-compose ps`
3. **K6 tests failing**: Verify the `BASE_URL` points to the correct service
4. **Database connection issues**: Ensure SQL Server container is running and environment variables are correct

## Example .env Override
```bash
# Copy .env to .env.local and modify
cp .env .env.local

# Edit .env.local with your custom values
DOTNET_REST_PORT=9080
DOTNET_GRPC_PORT=9083
K6_CONCURRENT_USERS=50
SQL_SERVER_PASSWORD=MyStrongPassword123

# Load custom environment
set -a; source .env.local; set +a
docker-compose up
```