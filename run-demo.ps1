# Advanced API Performance Tuning Demo - PowerShell Script
# This script helps set up and run the demonstration with proper environment configuration

param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    [Parameter(Position=1)]
    [string]$Service = ""
)

# Colors for output (PowerShell)
function Write-Info { 
    Write-Host "[INFO] $args" -ForegroundColor Blue 
}
function Write-Success { 
    Write-Host "[SUCCESS] $args" -ForegroundColor Green 
}
function Write-Warning { 
    Write-Host "[WARNING] $args" -ForegroundColor Yellow 
}
function Write-Error { 
    Write-Host "[ERROR] $args" -ForegroundColor Red 
}

# Function to check if Docker is running
function Test-Docker {
    try {
        docker info | Out-Null
        Write-Success "Docker is running"
        return $true
    }
    catch {
        Write-Error "Docker is not running or not accessible. Please start Docker and try again."
        return $false
    }
}

# Function to check if docker-compose is available
function Get-ComposeCommand {
    if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
        Write-Success "docker-compose found"
        return "docker-compose"
    }
    elseif ((docker compose version) 2>$null) {
        Write-Success "docker compose (plugin) found"
        return "docker compose"
    }
    else {
        Write-Error "Neither docker-compose nor 'docker compose' is available. Please install Docker Compose."
        return $null
    }
}

# Function to load environment variables from .env file
function Import-EnvFile {
    if (Test-Path ".env") {
        Write-Info "Loading environment variables from .env file..."
        Get-Content ".env" | ForEach-Object {
            if ($_ -match "^([^#][^=]*?)=(.*)$") {
                [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
            }
        }
        Write-Success "Environment variables loaded"
    }
    else {
        Write-Warning ".env file not found, using defaults"
    }
}

# Function to show current configuration
function Show-Configuration {
    $dotnetRestPort = $env:DOTNET_REST_PORT ?? "8080"
    $dotnetGrpcPort = $env:DOTNET_GRPC_PORT ?? "8083"
    $springPort = $env:SPRING_PORT ?? "8081"
    $grpcClientPort = $env:GRPC_CLIENT_TEST_PORT ?? "8084"
    $jaegerPort = $env:JAEGER_UI_PORT ?? "16686"
    $prometheusPort = $env:PROMETHEUS_PORT ?? "9090"
    $grafanaPort = $env:GRAFANA_PORT ?? "3000"
    $k6Users = $env:K6_CONCURRENT_USERS ?? "100"
    $k6Duration = $env:K6_DURATION ?? "30s"
    $k6BaseUrl = $env:K6_BASE_URL ?? "http://dotnet-app:8080"
    
    Write-Info "Current Configuration:"
    Write-Host "  .NET REST API: http://localhost:$dotnetRestPort"
    Write-Host "  .NET gRPC: http://localhost:$dotnetGrpcPort"
    Write-Host "  Spring Boot: http://localhost:$springPort"
    Write-Host "  gRPC Client Test: http://localhost:$grpcClientPort"
    Write-Host "  React Dashboard: http://localhost:3001"
    Write-Host "  Jaeger UI: http://localhost:$jaegerPort"
    Write-Host "  Prometheus: http://localhost:$prometheusPort"
    Write-Host "  Grafana: http://localhost:$grafanaPort"
    Write-Host ""
    Write-Host "Performance Testing:"
    Write-Host "  K6 Users: $k6Users"
    Write-Host "  K6 Duration: $k6Duration"
    Write-Host "  K6 Target: $k6BaseUrl"
    Write-Host ""
}

# Function to build all services
function Build-Services {
    param([string]$ComposeCmd)
    
    Write-Info "Building all services..."
    & $ComposeCmd.Split() build --no-cache
    if ($LASTEXITCODE -eq 0) {
        Write-Success "All services built successfully"
    } else {
        Write-Error "Build failed"
        return $false
    }
    return $true
}

# Function to start all services
function Start-Services {
    param([string]$ComposeCmd)
    
    Write-Info "Starting all services..."
    & $ComposeCmd.Split() up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Waiting for services to be ready..."
        Start-Sleep -Seconds 10
        
        Test-ServiceHealth
        Write-Success "All services started successfully"
        return $true
    } else {
        Write-Error "Failed to start services"
        return $false
    }
}

# Function to check service health
function Test-ServiceHealth {
    Write-Info "Checking service health..."
    
    $dotnetPort = $env:DOTNET_REST_PORT ?? "8080"
    $springPort = $env:SPRING_PORT ?? "8081"
    $jaegerPort = $env:JAEGER_UI_PORT ?? "16686"
    
    # Check .NET API
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$dotnetPort/api/health" -TimeoutSec 5 -ErrorAction Stop
        Write-Success ".NET API is healthy"
    }
    catch {
        Write-Warning ".NET API is not responding (this may take a moment)"
    }
    
    # Check Spring Boot (if it has health endpoint)
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$springPort/actuator/health" -TimeoutSec 5 -ErrorAction Stop
        Write-Success "Spring Boot API is healthy"
    }
    catch {
        Write-Warning "Spring Boot API is not responding (this may take a moment)"
    }
    
    # Check Jaeger UI
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$jaegerPort" -TimeoutSec 5 -ErrorAction Stop
        Write-Success "Jaeger UI is accessible"
    }
    catch {
        Write-Warning "Jaeger UI is not accessible yet"
    }
}

# Function to run K6 performance test
function Start-K6Test {
    param([string]$ComposeCmd, [string]$TestScript = "baseline.js")
    
    Write-Info "Running K6 performance test: $TestScript"
    
    $baseUrl = $env:K6_BASE_URL ?? "http://dotnet-app:8080"
    $vus = $env:K6_CONCURRENT_USERS ?? "100"
    $duration = $env:K6_DURATION ?? "30s"
    
    & $ComposeCmd.Split() run --rm -e BASE_URL=$baseUrl -e VUS=$vus -e DURATION=$duration k6 k6 run /k6/$TestScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "K6 test completed"
    } else {
        Write-Error "K6 test failed"
    }
}

# Function to stop all services
function Stop-Services {
    param([string]$ComposeCmd)
    
    Write-Info "Stopping all services..."
    & $ComposeCmd.Split() down
    Write-Success "All services stopped"
}

# Function to clean up everything
function Remove-Everything {
    param([string]$ComposeCmd)
    
    Write-Info "Cleaning up containers, images, and volumes..."
    & $ComposeCmd.Split() down --volumes --remove-orphans
    Write-Success "Cleanup completed"
}

# Function to show logs
function Show-Logs {
    param([string]$ComposeCmd, [string]$ServiceName = "")
    
    if ($ServiceName) {
        Write-Info "Showing logs for service: $ServiceName"
        & $ComposeCmd.Split() logs -f $ServiceName
    }
    else {
        Write-Info "Showing logs for all services"
        & $ComposeCmd.Split() logs -f
    }
}

# Function to show usage
function Show-Usage {
    Write-Host "Usage: .\run-demo.ps1 [COMMAND] [SERVICE]"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  start     Build and start all services"
    Write-Host "  stop      Stop all services"
    Write-Host "  restart   Restart all services"
    Write-Host "  build     Build all services"
    Write-Host "  status    Show service status"
    Write-Host "  config    Show current configuration"
    Write-Host "  logs      Show logs for all services"
    Write-Host "  logs SERVICE  Show logs for specific service"
    Write-Host "  test      Run K6 baseline performance test"
    Write-Host "  test-report   Run K6 report performance test"
    Write-Host "  health    Check service health"
    Write-Host "  cleanup   Stop services and remove containers/volumes"
    Write-Host "  help      Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\run-demo.ps1 start                    # Start all services"
    Write-Host "  .\run-demo.ps1 logs dotnet-app          # Show .NET app logs"
    Write-Host "  .\run-demo.ps1 test                     # Run performance test"
    Write-Host ""
    Write-Host "Environment Variables:"
    Write-Host "  Set `$env:DOTNET_REST_PORT=9080 before running to customize ports"
    Write-Host "  See .env file for all configurable options"
    Write-Host "  See ENVIRONMENT_VARIABLES.md for detailed documentation"
}

# Main script logic
function Main {
    Write-Info "Advanced API Performance Tuning Demo"
    Write-Info "======================================"
    
    $composeCmd = Get-ComposeCommand
    if (-not $composeCmd) {
        exit 1
    }
    
    switch ($Command.ToLower()) {
        "start" {
            if (-not (Test-Docker)) { exit 1 }
            Import-EnvFile
            Show-Configuration
            if (Build-Services $composeCmd) {
                if (Start-Services $composeCmd) {
                    Write-Success "Demo is ready! Check the URLs above to access the services."
                }
            }
        }
        "stop" {
            Stop-Services $composeCmd
        }
        "restart" {
            Stop-Services $composeCmd
            Import-EnvFile
            Start-Services $composeCmd
        }
        "build" {
            if (-not (Test-Docker)) { exit 1 }
            Build-Services $composeCmd
        }
        "status" {
            & $composeCmd.Split() ps
        }
        "config" {
            Import-EnvFile
            Show-Configuration
        }
        "logs" {
            Show-Logs $composeCmd $Service
        }
        "test" {
            Start-K6Test $composeCmd "baseline.js"
        }
        "test-report" {
            Start-K6Test $composeCmd "report-peformance-testing.js"
        }
        "health" {
            Import-EnvFile
            Test-ServiceHealth
        }
        "cleanup" {
            Remove-Everything $composeCmd
        }
        default {
            if ($Command -ne "help") {
                Write-Error "Unknown command: $Command"
                Write-Host ""
            }
            Show-Usage
        }
    }
}

# Run main function
Main