# Advanced API Performance Tuning - Demo Environment

This project provides a comprehensive environment for demonstrating API performance tuning techniques using containerized applications, monitoring, and load testing tools.

## 🚀 **NEW: Environment Variable Configuration**

**This demo has been fully updated to use environment variables for all configurations!** 
- All ports, URLs, and settings are now configurable via the `.env` file
- Easy customization for different environments (development, staging, production)
- Automated setup scripts for Windows and Linux/macOS
- See [ENVIRONMENT_VARIABLES.md](ENVIRONMENT_VARIABLES.md) for detailed documentation

## Architecture Overview

The demo environment includes:

- **Spring Boot Application** - Sample REST API with performance scenarios
- **.NET 8 Application** - Full-featured API with Circuit Breakers, gRPC, and performance testing endpoints
- **gRPC Client Test** - gRPC client testing service  
- **React Dashboard** - Interactive dashboard for monitoring and testing
- **Jaeger** - Distributed tracing and monitoring
- **OpenTelemetry Collector** - Telemetry data collection and processing
- **Prometheus** - Metrics collection and storage
- **Grafana** - Visualization and dashboarding
- **k6** - Load testing and performance benchmarking

## Prerequisites

- Docker Desktop or Docker with Docker Compose
- Git (to clone the repository)
- At least 4GB of available RAM for all containers
- PowerShell (Windows) or Bash (Linux/macOS) for automated scripts

## Quick Start

### 🎯 **Recommended: Use the Automated Setup Scripts**

**Windows (PowerShell):**
```powershell
# Start all services with environment variable configuration
.\run-demo.ps1 start

# Show current configuration
.\run-demo.ps1 config

# Run performance tests  
.\run-demo.ps1 test
```

**Linux/macOS (Bash):**
```bash
# Make script executable and start all services
chmod +x run-demo.sh
./run-demo.sh start

# Show current configuration
./run-demo.sh config

# Run performance tests
./run-demo.sh test
```

### 📋 **Manual Setup (Alternative)**

1. **Clone and Navigate to the Project**
```bash
git clone https://github.com/mohammedzaki/Advanced-API-Performance-Tuning.git
cd Advanced-API-Performance-Tuning/demos
```

2. **Review and Customize Configuration**
```bash
# View current environment variables
cat .env

# Copy and customize if needed
cp .env .env.local
# Edit .env.local with your preferences
```

3. **Start the Environment**
```bash
docker compose up --build -d
```

4. **Verify Services are Running**
```bash
docker compose ps
```

## Accessing the Applications

Once the environment is running, you can access the following services (ports configurable via `.env`):

| Service | Default URL | Environment Variable | Description |
|---------|-------------|----------------------|-------------|
| **.NET REST API** | http://localhost:8080 | `DOTNET_REST_PORT` | Main sample API with Circuit Breakers |
| **.NET gRPC Service** | http://localhost:8083 | `DOTNET_GRPC_PORT` | gRPC service endpoints |
| **Spring Boot API** | http://localhost:8081 | `SPRING_PORT` | Alternative sample API |
| **gRPC Client Test** | http://localhost:8084 | `GRPC_CLIENT_TEST_PORT` | gRPC client testing service |
| **React Dashboard** | http://localhost:3001 | N/A (fixed) | Interactive monitoring dashboard |
| **Jaeger UI** | http://localhost:16686 | `JAEGER_UI_PORT` | Distributed tracing interface |
| **Prometheus** | http://localhost:9090 | `PROMETHEUS_PORT` | Metrics collection interface |
| **Grafana** | http://localhost:3000 | `GRAFANA_PORT` | Visualization dashboards (admin/admin) |

### 🎛️ **Quick Access Commands**
```bash
# Show all current URLs with your configuration
./run-demo.sh config      # Linux/macOS
.\run-demo.ps1 config     # Windows
```

## API Endpoints

### .NET Application (Primary - Port 8080)
**REST APIs:**
- `GET /api/health` - Health check endpoint
- `GET /api/products` - Product listing with optional filtering
- `GET /api/blocking-sql/blocking-optimized` - Synchronous SQL operations
- `GET /api/blocking-sql/non-blocking-optimized` - Asynchronous SQL operations  
- `GET /api/report/sales-report?months=N` - Sales report generation
- `GET /api/report/detailed-analytics?records=N` - Analytics data processing
- `GET /api/circuit-breaker/status` - Circuit breaker status
- `GET /api/circuit-breaker/database-test` - Test database circuit breaker
- `GET /api/circuit-breaker/api-test` - Test API circuit breaker

**gRPC Services (Port 8083):**
- `ProductService.GetProducts` - gRPC product service
- Reflection enabled for testing tools

### Spring Boot Application (Port 8081)
- `GET /api/products` - Standard product listing  
- `GET /api/products-delayed` - Simulated slow endpoint for testing
- `GET /actuator/health` - Spring Boot health check

### gRPC Client Test (Port 8084)
- `GET /api/grpc-test` - Test gRPC communication with .NET service

## Running Performance Tests

### 🎯 **Using Automated Scripts (Recommended)**
```bash
# Run baseline performance test
./run-demo.sh test           # Linux/macOS
.\run-demo.ps1 test          # Windows

# Run comprehensive report performance test
./run-demo.sh test-report    # Linux/macOS  
.\run-demo.ps1 test-report   # Windows

# Check service health before testing
./run-demo.sh health         # Linux/macOS
.\run-demo.ps1 health        # Windows
```

### Manual k6 Tests
```bash
# Basic performance test
docker compose run --rm k6

# Custom test with environment variables
docker compose run --rm -e VUS=200 -e DURATION=60s k6

# Run specific test file
docker compose run --rm k6 k6 run /k6/report-peformance-testing.js
```

### 📊 **Test Configuration via Environment Variables**
Customize performance tests by setting variables in `.env`:
```bash
K6_CONCURRENT_USERS=100       # Number of virtual users
K6_DURATION=30s               # Test duration
K6_BASE_URL=http://dotnet-app:8080  # Target service
K6_SLEEP_DURATION=0.2         # Sleep between requests
```

## Monitoring and Observability

### Jaeger (Distributed Tracing)
1. Open http://localhost:16686
2. Select "spring-sample" from the Service dropdown
3. Click "Find Traces" to view API call traces
4. Analyze request flows and performance bottlenecks

### Prometheus (Metrics)
1. Open http://localhost:9090
2. Use the query interface to explore metrics
3. Example queries:
   - `http_requests_total` - Total HTTP requests
   - `http_request_duration_seconds` - Request duration metrics

### Grafana (Dashboards)
1. Open http://localhost:3000
2. Login with username: `admin`, password: `admin`
3. Create or import dashboards to visualize performance data

## Configuration Files

- **docker-compose.yml** - Main orchestration file
- **otel-collector-config.yaml** - OpenTelemetry Collector configuration
- **prometheus.yml** - Prometheus scraping configuration
- **k6/baseline.js** - Load test scenario

## Enabling .NET Application

To also run the .NET 8 sample application:

1. Uncomment the `dotnet-app` service in `docker-compose.yml`
2. Update k6 tests to include .NET endpoints
3. Restart the environment:
```bash
docker compose down
docker compose up --build -d
```

The .NET API will be available at http://localhost:8082

## Troubleshooting

### Check Service Status
```bash
docker compose ps
```

### View Service Logs
```bash
# All services
docker compose logs

# Specific service
docker compose logs spring-app
docker compose logs k6
```

### Restart Services
```bash
# Restart all services
docker compose restart

# Restart specific service
docker compose restart spring-app
```

### Clean Restart
```bash
docker compose down
docker compose up --build -d
```

## Stopping the Environment

To stop all services:
```bash
docker compose down
```

To stop and remove all data:
```bash
docker compose down -v
```

## Performance Testing Scenarios

The environment supports various performance testing scenarios:

1. **Baseline Performance** - Measure normal API response times
2. **Load Testing** - Test API behavior under increased load
3. **Stress Testing** - Find breaking points and resource limits
4. **Endurance Testing** - Long-running stability tests
5. **Spike Testing** - Sudden load increases

## Development and Debugging

### Rebuilding Applications
```bash
# Rebuild specific service
docker compose build spring-app

# Rebuild all services
docker compose build

# Force rebuild without cache
docker compose build --no-cache
```

### Accessing Container Shells
```bash
# Spring Boot application
docker compose exec spring-app sh

# View k6 files
docker compose exec k6 sh
```

## Next Steps

1. Explore the API endpoints and observe traces in Jaeger
2. Run load tests and monitor performance metrics
3. Modify the applications to introduce performance issues
4. Practice identifying and resolving bottlenecks using the monitoring tools
5. Experiment with different k6 test scenarios

For detailed course materials and advanced scenarios, refer to the course documentation.