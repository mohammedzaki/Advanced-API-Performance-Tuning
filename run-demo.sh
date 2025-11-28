#!/bin/bash

# Advanced API Performance Tuning Demo - Environment Setup Script
# This script helps set up and run the demonstration with proper environment configuration

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running or not accessible. Please start Docker and try again."
        exit 1
    fi
    print_success "Docker is running"
}

# Function to check if docker-compose is available
check_docker_compose() {
    if command -v docker-compose > /dev/null 2>&1; then
        print_success "docker-compose found"
        COMPOSE_CMD="docker-compose"
    elif docker compose version > /dev/null 2>&1; then
        print_success "docker compose (plugin) found"
        COMPOSE_CMD="docker compose"
    else
        print_error "Neither docker-compose nor 'docker compose' is available. Please install Docker Compose."
        exit 1
    fi
}

# Function to load environment variables
load_environment() {
    if [ -f ".env" ]; then
        print_info "Loading environment variables from .env file..."
        set -a  # Automatically export all variables
        source .env
        set +a
        print_success "Environment variables loaded"
    else
        print_warning ".env file not found, using defaults"
    fi
}

# Function to show current configuration
show_configuration() {
    print_info "Current Configuration:"
    echo "  .NET REST API: http://localhost:${DOTNET_REST_PORT:-8080}"
    echo "  .NET gRPC: http://localhost:${DOTNET_GRPC_PORT:-8083}"
    echo "  Spring Boot: http://localhost:${SPRING_PORT:-8081}"
    echo "  gRPC Client Test: http://localhost:${GRPC_CLIENT_TEST_PORT:-8084}"
    echo "  React Dashboard: http://localhost:3001"
    echo "  Jaeger UI: http://localhost:${JAEGER_UI_PORT:-16686}"
    echo "  Prometheus: http://localhost:${PROMETHEUS_PORT:-9090}"
    echo "  Grafana: http://localhost:${GRAFANA_PORT:-3000}"
    echo ""
    echo "Performance Testing:"
    echo "  K6 Users: ${K6_CONCURRENT_USERS:-100}"
    echo "  K6 Duration: ${K6_DURATION:-30s}"
    echo "  K6 Target: ${K6_BASE_URL:-http://dotnet-app:8080}"
    echo ""
}

# Function to build all services
build_services() {
    print_info "Building all services..."
    $COMPOSE_CMD build --no-cache
    print_success "All services built successfully"
}

# Function to start all services
start_services() {
    print_info "Starting all services..."
    $COMPOSE_CMD up -d
    
    print_info "Waiting for services to be ready..."
    sleep 10
    
    # Check service health
    check_service_health
    print_success "All services started successfully"
}

# Function to check service health
check_service_health() {
    print_info "Checking service health..."
    
    # Check .NET API
    if curl -s -f "http://localhost:${DOTNET_REST_PORT:-8080}/api/health" > /dev/null 2>&1; then
        print_success ".NET API is healthy"
    else
        print_warning ".NET API is not responding (this may take a moment)"
    fi
    
    # Check Spring Boot (if it has health endpoint)
    if curl -s -f "http://localhost:${SPRING_PORT:-8081}/actuator/health" > /dev/null 2>&1; then
        print_success "Spring Boot API is healthy"
    else
        print_warning "Spring Boot API is not responding (this may take a moment)"
    fi
    
    # Check Jaeger UI
    if curl -s -f "http://localhost:${JAEGER_UI_PORT:-16686}" > /dev/null 2>&1; then
        print_success "Jaeger UI is accessible"
    else
        print_warning "Jaeger UI is not accessible yet"
    fi
}

# Function to run K6 performance test
run_k6_test() {
    local test_script=${1:-baseline.js}
    print_info "Running K6 performance test: $test_script"
    
    # Run K6 test with environment variables
    $COMPOSE_CMD run --rm -e BASE_URL="${K6_BASE_URL:-http://dotnet-app:8080}" \
                            -e VUS="${K6_CONCURRENT_USERS:-100}" \
                            -e DURATION="${K6_DURATION:-30s}" \
                            k6 k6 run /k6/$test_script
    
    print_success "K6 test completed"
}

# Function to stop all services
stop_services() {
    print_info "Stopping all services..."
    $COMPOSE_CMD down
    print_success "All services stopped"
}

# Function to clean up everything
cleanup() {
    print_info "Cleaning up containers, images, and volumes..."
    $COMPOSE_CMD down --volumes --remove-orphans
    # Optionally remove images (uncomment if needed)
    # $COMPOSE_CMD down --volumes --remove-orphans --rmi all
    print_success "Cleanup completed"
}

# Function to show logs
show_logs() {
    local service=${1:-}
    if [ -n "$service" ]; then
        print_info "Showing logs for service: $service"
        $COMPOSE_CMD logs -f "$service"
    else
        print_info "Showing logs for all services"
        $COMPOSE_CMD logs -f
    fi
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  start     Build and start all services"
    echo "  stop      Stop all services"
    echo "  restart   Restart all services"
    echo "  build     Build all services"
    echo "  status    Show service status"
    echo "  config    Show current configuration"
    echo "  logs      Show logs for all services"
    echo "  logs SERVICE  Show logs for specific service"
    echo "  test      Run K6 baseline performance test"
    echo "  test-report   Run K6 report performance test"
    echo "  health    Check service health"
    echo "  cleanup   Stop services and remove containers/volumes"
    echo "  help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 start                    # Start all services"
    echo "  $0 logs dotnet-app          # Show .NET app logs"
    echo "  $0 test                     # Run performance test"
    echo "  DOTNET_REST_PORT=9080 $0 start  # Start with custom port"
    echo ""
    echo "Environment Variables:"
    echo "  See .env file for all configurable options"
    echo "  See ENVIRONMENT_VARIABLES.md for detailed documentation"
}

# Main script logic
main() {
    local command=${1:-help}
    
    print_info "Advanced API Performance Tuning Demo"
    print_info "======================================"
    
    case $command in
        "start")
            check_docker
            check_docker_compose
            load_environment
            show_configuration
            build_services
            start_services
            print_success "Demo is ready! Check the URLs above to access the services."
            ;;
        "stop")
            check_docker_compose
            stop_services
            ;;
        "restart")
            check_docker_compose
            stop_services
            load_environment
            start_services
            ;;
        "build")
            check_docker
            check_docker_compose
            build_services
            ;;
        "status")
            check_docker_compose
            $COMPOSE_CMD ps
            ;;
        "config")
            load_environment
            show_configuration
            ;;
        "logs")
            check_docker_compose
            show_logs $2
            ;;
        "test")
            check_docker_compose
            run_k6_test "baseline.js"
            ;;
        "test-report")
            check_docker_compose
            run_k6_test "report-peformance-testing.js"
            ;;
        "health")
            load_environment
            check_service_health
            ;;
        "cleanup")
            check_docker_compose
            cleanup
            ;;
        "help"|"--help"|"-h")
            show_usage
            ;;
        *)
            print_error "Unknown command: $command"
            echo ""
            show_usage
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"