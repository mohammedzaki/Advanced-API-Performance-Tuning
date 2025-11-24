# Advanced API Performance Tuning - Demo Environment

This project provides a comprehensive environment for demonstrating API performance tuning techniques using containerized applications, monitoring, and load testing tools.

## Architecture Overview

The demo environment includes:

- **Spring Boot Application** - Sample REST API with performance scenarios
- **.NET 8 Application** - Alternative sample API (currently commented out)
- **Jaeger** - Distributed tracing and monitoring
- **OpenTelemetry Collector** - Telemetry data collection and processing
- **Prometheus** - Metrics collection and storage
- **Grafana** - Visualization and dashboarding
- **k6** - Load testing and performance benchmarking

## Prerequisites

- Docker Desktop or Docker with Docker Compose
- Git (to clone the repository)
- At least 4GB of available RAM for all containers

## Quick Start

### 1. Clone and Navigate to the Project
```bash
git clone https://github.com/mohammedzaki/Advanced-API-Performance-Tuning.git
cd Advanced-API-Performance-Tuning/demos
```

### 2. Start the Environment
```bash
docker compose up --build -d
```

This command will:
- Build the Spring Boot application image
- Pull all required images (Jaeger, Prometheus, Grafana, k6, etc.)
- Start all services in the background
- Set up networking between containers

### 3. Verify Services are Running
```bash
docker compose ps
```

You should see all services with "Up" status.

## Accessing the Applications

Once the environment is running, you can access the following services:

| Service | URL | Description |
|---------|-----|-------------|
| **Spring Boot API** | http://localhost:8081 | Main sample API |
| **Jaeger UI** | http://localhost:16686 | Distributed tracing interface |
| **Prometheus** | http://localhost:9090 | Metrics collection interface |
| **Grafana** | http://localhost:3000 | Visualization dashboards (admin/admin) |

## API Endpoints

The Spring Boot application provides these test endpoints:

- `GET http://localhost:8081/api/products` - Standard product listing
- `GET http://localhost:8081/api/products-delayed` - Simulated slow endpoint for testing

## Running Performance Tests

### Manual k6 Test
To run the k6 load test manually:
```bash
docker compose run --rm k6 run /k6/baseline.js
```

### Custom k6 Tests
You can modify the test script in `k6/baseline.js` or create new test files:
```bash
# Run a custom test file
docker compose run --rm k6 run /k6/your-test.js

# Run with different parameters
docker compose run --rm k6 run --vus 50 --duration 60s /k6/baseline.js
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