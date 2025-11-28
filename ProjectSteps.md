# Project Steps - Building the Advanced API Performance Tuning Demo

This guide walks you through creating the complete Advanced API Performance Tuning Demo project from scratch. Follow these steps to build a comprehensive performance testing and monitoring environment.

## 📋 Prerequisites

Before starting, ensure you have:
- **Docker Desktop** installed and running
- **.NET 8.0 SDK** or later
- **Java 17** or later (for Spring Boot)
- **Node.js 18+** (for React dashboard)
- **Git** for version control
- **Visual Studio Code** or preferred IDE
- **PowerShell** (Windows) or **Bash** (Linux/macOS)

## 🏁 Step 1: Project Structure Setup

### 1.1 Create Root Directory and Initialize Git
```bash
mkdir Advanced-API-Performance-Tuning-demo
cd Advanced-API-Performance-Tuning-demo
git init
```

### 1.2 Create Directory Structure
```bash
mkdir dotnet8-sample
mkdir spring-boot-sample
mkdir grpc-client-test
mkdir react-reports-dashboard
mkdir k6
mkdir db
mkdir proto
```

### 1.3 Create Environment Configuration
Create `.env` file in root:
```env
# Service Ports
SPRING_PORT=8081
DOTNET_REST_PORT=8080
DOTNET_GRPC_PORT=8083
GRPC_CLIENT_TEST_PORT=8084

# Database Configuration
SQL_SERVER_HOST=mssql
SQL_SERVER_PORT=1433
SQL_SERVER_DATABASE=AdventureWorks2022
SQL_SERVER_USER=sa
SQL_SERVER_PASSWORD=YourStrong!Passw0rd

# OpenTelemetry Configuration
OTEL_SERVICE_NAME_SPRING=spring-sample
OTEL_SERVICE_NAME_DOTNET=dotnet-sample
OTEL_SERVICE_NAME_GRPC_CLIENT=grpc-client-test
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318/v1/traces

# Observability Ports
JAEGER_UI_PORT=16686
JAEGER_COLLECTOR_PORT=14250
OTEL_COLLECTOR_GRPC_PORT=4317
OTEL_COLLECTOR_HTTP_PORT=4318
PROMETHEUS_PORT=9090
GRAFANA_PORT=3000

# Performance Testing
K6_BASE_URL=http://dotnet-app:8080
K6_CONCURRENT_USERS=100
K6_DURATION=30s
K6_SLEEP_DURATION=0.2

# Report API Configuration
REPORT_DEFAULT_MONTHS=3
REPORT_DEFAULT_RECORDS=500
REPORT_MAX_COMPLEXITY=10
```

## 🐳 Step 2: Docker Orchestration Setup

### 2.1 Create docker-compose.yml
```yaml
version: '3.8'

services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: jaeger
    ports:
      - "${JAEGER_UI_PORT:-16686}:16686"
      - "${JAEGER_COLLECTOR_PORT:-14250}:14250"
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  otel-collector:
    image: otel/opentelemetry-collector:latest
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml:ro
    ports:
      - "${OTEL_COLLECTOR_GRPC_PORT:-4317}:4317"
      - "${OTEL_COLLECTOR_HTTP_PORT:-4318}:4318"
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  # Additional services will be added in subsequent steps

networks:
  perf-net:
    driver: bridge
```

### 2.2 Create OpenTelemetry Collector Configuration
Create `otel-collector-config.yaml`:
```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:

exporters:
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [jaeger]
```

## 🏗️ Step 3: .NET 8 Application Development

### 3.1 Initialize .NET Project
```bash
cd dotnet8-sample
dotnet new webapi --name dotnet8-sample
cd ..
```

### 3.2 Add Required NuGet Packages
Edit `dotnet8-sample/dotnet8-sample.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>dotnet_sample</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.5.1-beta.1" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.5.1-beta.1" />
    <PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.5.0-beta.1" />
    <PackageReference Include="Grpc.AspNetCore" Version="2.59.0" />
    <PackageReference Include="Grpc.AspNetCore.Server.Reflection" Version="2.59.0" />
    <PackageReference Include="Polly.Extensions" Version="8.6.5" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.2" />
  </ItemGroup>

  <ItemGroup>
    <Protobuf Include="proto\product.proto" GrpcServices="Server" />
  </ItemGroup>
</Project>
```

### 3.3 Create Product Model
Create `dotnet8-sample/Models/Product.cs`:
```csharp
namespace dotnet_sample.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
```

### 3.4 Create Product Service
Create `dotnet8-sample/Services/ProductService.cs`:
```csharp
using System.Diagnostics;
using dotnet_sample.Models;

namespace dotnet_sample.Services;

public class ProductService
{
    private static readonly ActivitySource ActivitySource = new("dotnet-sample.ProductService");
    private readonly List<Product> _products;

    public ProductService()
    {
        _products = GenerateSampleProducts();
    }

    public async Task<List<Product>> GetProductsAsync(string? category = null, int? limit = null)
    {
        using var activity = ActivitySource.StartActivity("GetProducts");
        activity?.SetTag("category", category ?? "all");
        activity?.SetTag("limit", limit?.ToString() ?? "none");

        // Simulate some processing time
        await Task.Delay(Random.Shared.Next(10, 50));

        var query = _products.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return query.ToList();
    }

    private static List<Product> GenerateSampleProducts()
    {
        var categories = new[] { "Electronics", "Clothing", "Books", "Home", "Sports" };
        var products = new List<Product>();

        for (int i = 1; i <= 100; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                Price = Random.Shared.Next(10, 1000),
                Category = categories[Random.Shared.Next(categories.Length)],
                CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 365)),
                IsActive = Random.Shared.Next(0, 10) > 1
            });
        }

        return products;
    }
}
```

### 3.5 Create Controllers

#### Product Controller
Create `dotnet8-sample/Controllers/ProductController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using dotnet_sample.Services;
using System.Diagnostics;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("dotnet-sample.ProductController");
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? category = null, [FromQuery] int? limit = null)
    {
        using var activity = ActivitySource.StartActivity("GetProducts");
        
        try
        {
            var products = await _productService.GetProductsAsync(category, limit);
            return Ok(products);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

#### Health Controller
Create `dotnet8-sample/Controllers/HealthController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Service = "dotnet-sample"
        });
    }
}
```

#### Circuit Breaker Controller
Create `dotnet8-sample/Controllers/CircuitBreakerController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CircuitBreakerController : ControllerBase
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public CircuitBreakerController(ResiliencePipelineProvider<string> pipelineProvider)
    {
        _pipelineProvider = pipelineProvider;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { Status = "Circuit Breaker API is running", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("database-test")]
    public async Task<IActionResult> TestDatabaseCircuitBreaker()
    {
        var pipeline = _pipelineProvider.GetPipeline("database-circuit-breaker");
        
        try
        {
            var result = await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                // Simulate database operation
                await Task.Delay(100, cancellationToken);
                
                // Simulate random failures for testing
                if (Random.Shared.Next(1, 10) > 7)
                {
                    throw new InvalidOperationException("Simulated database failure");
                }
                
                return "Database operation completed successfully";
            });
            
            return Ok(new { Success = true, Message = result, Timestamp = DateTime.UtcNow });
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(503, new { 
                Success = false, 
                Message = "Circuit breaker is open - database service unavailable",
                Timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = ex.Message,
                Timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpGet("api-test")]
    public async Task<IActionResult> TestApiCircuitBreaker()
    {
        var pipeline = _pipelineProvider.GetPipeline("api-circuit-breaker");
        
        try
        {
            var result = await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                // Simulate API call
                await Task.Delay(50, cancellationToken);
                
                // Simulate random failures for testing
                if (Random.Shared.Next(1, 10) > 8)
                {
                    throw new HttpRequestException("Simulated API failure");
                }
                
                return "API call completed successfully";
            });
            
            return Ok(new { Success = true, Message = result, Timestamp = DateTime.UtcNow });
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(503, new { 
                Success = false, 
                Message = "Circuit breaker is open - API service unavailable",
                Timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = ex.Message,
                Timestamp = DateTime.UtcNow 
            });
        }
    }
}
```

### 3.6 Create Protocol Buffers Definition
Create `dotnet8-sample/proto/product.proto`:
```protobuf
syntax = "proto3";

option csharp_namespace = "dotnet_sample.Grpc";

package product;

service ProductService {
  rpc GetProducts (GetProductsRequest) returns (GetProductsResponse);
  rpc GetProduct (GetProductRequest) returns (ProductReply);
}

message GetProductsRequest {
  string category = 1;
  int32 limit = 2;
}

message GetProductsResponse {
  repeated ProductReply products = 1;
}

message GetProductRequest {
  int32 id = 1;
}

message ProductReply {
  int32 id = 1;
  string name = 2;
  string description = 3;
  double price = 4;
  string category = 5;
  string created_date = 6;
  bool is_active = 7;
}
```

### 3.7 Create gRPC Service Implementation
Create `dotnet8-sample/ProtoServices/ProductService.cs`:
```csharp
using Grpc.Core;
using dotnet_sample.Grpc;
using dotnet_sample.Services;

namespace dotnet_sample.ProtoServices;

public class ProductService : dotnet_sample.Grpc.ProductService.ProductServiceBase
{
    private readonly Services.ProductService _productService;

    public ProductService(Services.ProductService productService)
    {
        _productService = productService;
    }

    public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var products = await _productService.GetProductsAsync(
            string.IsNullOrEmpty(request.Category) ? null : request.Category,
            request.Limit == 0 ? null : request.Limit);

        var response = new GetProductsResponse();
        
        foreach (var product in products)
        {
            response.Products.Add(new ProductReply
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                Category = product.Category,
                CreatedDate = product.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                IsActive = product.IsActive
            });
        }

        return response;
    }

    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var products = await _productService.GetProductsAsync();
        var product = products.FirstOrDefault(p => p.Id == request.Id);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        return new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            Category = product.Category,
            CreatedDate = product.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsActive = product.IsActive
        };
    }
}
```

### 3.8 Configure Program.cs
Create `dotnet8-sample/Program.cs`:
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;
using Polly;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

// Configure Kestrel ports - Environment variables take precedence
var httpPort = builder.Configuration.GetValue<int>("DOTNET_HTTP_PORT", 
    builder.Configuration.GetValue<int>("DOTNET_CONTAINER_HTTP_PORT", 8080));

var grpcPort = builder.Configuration.GetValue<int>("DOTNET_GRPC_PORT", 
    builder.Configuration.GetValue<int>("DOTNET_CONTAINER_GRPC_PORT", 8083));

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(httpPort, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
    options.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

Console.WriteLine($"[KESTREL] HTTP/1.1 endpoint configured on port: {httpPort}");
Console.WriteLine($"[KESTREL] gRPC HTTP/2 endpoint configured on port: {grpcPort}");

// Register services
builder.Services.AddSingleton<dotnet_sample.Services.ProductService>();

// Configure Circuit Breaker policies
builder.Services.AddResiliencePipeline("database-circuit-breaker", builder =>
{
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 3,
        BreakDuration = TimeSpan.FromSeconds(30),
        OnOpened = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] Database circuit opened at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] Database circuit closed at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        }
    });
});

// Configure OpenTelemetry
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME_DOTNET") ?? "dotnet-sample";
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4318/v1/traces";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource("dotnet-sample.ProductController")
        .AddSource("dotnet-sample.ProductService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

// Add gRPC reflection (only in Development for security)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<dotnet_sample.ProtoServices.ProductService>();

// Enable reflection endpoint in Development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
```

### 3.9 Create Dockerfile for .NET Application
Create `dotnet8-sample/Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["dotnet8-sample.csproj", "."]
RUN dotnet restore "./dotnet8-sample.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "dotnet8-sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet8-sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dotnet8-sample.dll"]
```

## ☕ Step 4: Spring Boot Application Development

### 4.1 Initialize Spring Boot Project
```bash
cd spring-boot-sample
# Use Spring Initializr or create manually
# Dependencies: Web, Actuator, OpenTelemetry
```

### 4.2 Create pom.xml
Create `spring-boot-sample/pom.xml`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0">
    <modelVersion>4.0.0</modelVersion>
    
    <parent>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-parent</artifactId>
        <version>3.2.0</version>
        <relativePath/>
    </parent>
    
    <groupId>com.example</groupId>
    <artifactId>spring-boot-sample</artifactId>
    <version>0.0.1-SNAPSHOT</version>
    <name>spring-boot-sample</name>
    
    <properties>
        <java.version>17</java.version>
    </properties>
    
    <dependencies>
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-web</artifactId>
        </dependency>
        <dependency>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-starter-actuator</artifactId>
        </dependency>
        <dependency>
            <groupId>io.opentelemetry.instrumentation</groupId>
            <artifactId>opentelemetry-spring-boot-starter</artifactId>
            <version>1.32.0-alpha</version>
        </dependency>
    </dependencies>
    
    <build>
        <plugins>
            <plugin>
                <groupId>org.springframework.boot</groupId>
                <artifactId>spring-boot-maven-plugin</artifactId>
            </plugin>
        </plugins>
    </build>
</project>
```

### 4.3 Create Application Properties
Create `spring-boot-sample/src/main/resources/application.properties`:
```properties
server.port=${SERVER_PORT:8080}
spring.main.web-application-type=servlet
management.endpoints.web.exposure.include=health,info

# OpenTelemetry Configuration
otel.service.name=${OTEL_SERVICE_NAME_SPRING:spring-sample}
otel.exporter.otlp.endpoint=${OTEL_EXPORTER_OTLP_ENDPOINT:http://localhost:4318/v1/traces}

# Report API Configuration  
app.report.default-months=${REPORT_DEFAULT_MONTHS:3}
app.report.default-records=${REPORT_DEFAULT_RECORDS:500}
app.report.max-complexity=${REPORT_MAX_COMPLEXITY:10}
```

### 4.4 Create Spring Boot Main Class
Create `spring-boot-sample/src/main/java/com/example/SpringBootSampleApplication.java`:
```java
package com.example;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class SpringBootSampleApplication {
    public static void main(String[] args) {
        SpringApplication.run(SpringBootSampleApplication.class, args);
    }
}
```

### 4.5 Create Product Model and Controller
Create `spring-boot-sample/src/main/java/com/example/model/Product.java`:
```java
package com.example.model;

import java.time.LocalDateTime;

public class Product {
    private int id;
    private String name;
    private String description;
    private double price;
    private String category;
    private LocalDateTime createdDate;
    private boolean active;
    
    // Constructors, getters, and setters
    public Product() {}
    
    public Product(int id, String name, String description, double price, String category) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.category = category;
        this.createdDate = LocalDateTime.now();
        this.active = true;
    }
    
    // Getters and setters...
}
```

Create `spring-boot-sample/src/main/java/com/example/controller/ProductController.java`:
```java
package com.example.controller;

import com.example.model.Product;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api")
public class ProductController {
    
    private final List<Product> products = generateSampleProducts();
    
    @GetMapping("/products")
    public List<Product> getProducts(@RequestParam(required = false) String category) {
        if (category != null) {
            return products.stream()
                    .filter(p -> p.getCategory().equalsIgnoreCase(category))
                    .collect(Collectors.toList());
        }
        return products;
    }
    
    @GetMapping("/products-delayed")
    public List<Product> getProductsDelayed() throws InterruptedException {
        Thread.sleep(2000); // Simulate slow operation
        return products;
    }
    
    private List<Product> generateSampleProducts() {
        List<Product> products = new ArrayList<>();
        String[] categories = {"Electronics", "Clothing", "Books", "Home", "Sports"};
        
        for (int i = 1; i <= 50; i++) {
            products.add(new Product(
                i,
                "Product " + i,
                "Description for product " + i,
                Math.random() * 1000,
                categories[i % categories.length]
            ));
        }
        
        return products;
    }
}
```

### 4.6 Create Dockerfile for Spring Boot
Create `spring-boot-sample/Dockerfile`:
```dockerfile
FROM openjdk:17-jdk-slim AS build
WORKDIR /app
COPY pom.xml .
COPY src ./src
RUN apt-get update && apt-get install -y maven
RUN mvn clean package -DskipTests

FROM openjdk:17-jre-slim
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "app.jar"]
```

## 🧪 Step 5: Performance Testing with K6

### 5.1 Create K6 Baseline Test
Create `k6/baseline.js`:
```javascript
import http from 'k6/http';
import { sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const VUS = __ENV.VUS || 100;
const DURATION = __ENV.DURATION || '10s';
const SLEEP_DURATION = __ENV.SLEEP_DURATION || 0.2;

export const options = {
  vus: parseInt(VUS),
  duration: DURATION,
  thresholds: {
    http_req_duration: [
      'p(50)<150',
      'p(99)<300'
    ],
  },
};

export default function () {
  http.get(`${BASE_URL}/api/blocking-sql/non-blocking-optimized`);
  sleep(parseFloat(SLEEP_DURATION));
}
```

### 5.2 Create K6 Report Performance Test
Create `k6/report-performance-testing.js`:
```javascript
import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const VUS = __ENV.VUS || 100;
const DURATION = __ENV.DURATION || '60s';
const REPORT_DEFAULT_MONTHS = __ENV.REPORT_DEFAULT_MONTHS || 2;
const REPORT_DEFAULT_RECORDS = __ENV.REPORT_DEFAULT_RECORDS || 1000;

export const options = {
  vus: parseInt(VUS),
  duration: DURATION,
  thresholds: {
    http_req_duration: [
      'p(50)<150',
      'p(99)<300'
    ],
  },
};

export default function () {
  // Quick sales report
  let response1 = http.get(`${BASE_URL}/api/report/sales-report?months=${REPORT_DEFAULT_MONTHS}`);
  check(response1, { 'Quick report status is 200': (r) => r.status === 200 });

  // Large analytics dataset
  let response2 = http.get(`${BASE_URL}/api/report/detailed-analytics?records=${REPORT_DEFAULT_RECORDS}`);
  check(response2, { 'Analytics report status is 200': (r) => r.status === 200 });

  sleep(1);
}
```

## 📊 Step 6: Monitoring Configuration

### 6.1 Create Prometheus Configuration
Create `prometheus.yml`:
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
  
  - job_name: 'dotnet-app'
    static_configs:
      - targets: ['dotnet-app:8080']
    scrape_interval: 5s
    metrics_path: '/metrics'
  
  - job_name: 'spring-app'
    static_configs:
      - targets: ['spring-app:8080']
    scrape_interval: 5s
    metrics_path: '/actuator/prometheus'
```

### 6.2 Update Docker Compose with All Services
Add to `docker-compose.yml`:
```yaml
  spring-app:
    build:
      context: ./spring-boot-sample
    container_name: spring-app
    environment:
      - OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT:-http://otel-collector:4318/v1/traces}
      - OTEL_SERVICE_NAME=${OTEL_SERVICE_NAME_SPRING:-spring-sample}
      - SERVER_PORT=8080
    ports:
      - "${SPRING_PORT:-8081}:8080"
    depends_on:
      - otel-collector
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  dotnet-app:
    build:
      context: ./dotnet8-sample
    container_name: dotnet-app
    environment:
      - OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT:-http://otel-collector:4318/v1/traces}
      - OTEL_SERVICE_NAME_DOTNET=${OTEL_SERVICE_NAME_DOTNET:-dotnet-sample}
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - DOTNET_CONTAINER_HTTP_PORT=${DOTNET_CONTAINER_HTTP_PORT:-8080}
      - DOTNET_CONTAINER_GRPC_PORT=${DOTNET_CONTAINER_GRPC_PORT:-8083}
    ports:
      - "${DOTNET_GRPC_PORT:-8083}:${DOTNET_CONTAINER_GRPC_PORT:-8083}"
      - "${DOTNET_REST_PORT:-8080}:${DOTNET_CONTAINER_HTTP_PORT:-8080}"
    depends_on:
      - otel-collector
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  k6:
    image: grafana/k6:latest
    container_name: k6
    environment:
      - BASE_URL=${K6_BASE_URL:-http://dotnet-app:8080}
      - VUS=${K6_CONCURRENT_USERS:-10}
      - DURATION=${K6_DURATION:-30s}
    volumes:
      - ./k6:/k6
    entrypoint: ["k6", "run", "/k6/baseline.js"]
    depends_on:
      - spring-app
      - dotnet-app
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml:ro
    ports:
      - "${PROMETHEUS_PORT:-9090}:9090"
    networks:
      - ${DOCKER_NETWORK:-perf-net}

  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    ports:
      - "${GRAFANA_PORT:-3000}:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    depends_on:
      - prometheus
    networks:
      - ${DOCKER_NETWORK:-perf-net}
```

## 🎯 Step 7: Automation Scripts

### 7.1 Create PowerShell Automation Script
Create `run-demo.ps1` (see previous implementation)

### 7.2 Create Bash Automation Script  
Create `run-demo.sh` (see previous implementation)

### 7.3 Make Scripts Executable
```bash
chmod +x run-demo.sh
```

## 📚 Step 8: Documentation

### 8.1 Create Main README
Create comprehensive `README.md` (see previous implementation)

### 8.2 Create Environment Variables Documentation
Create `ENVIRONMENT_VARIABLES.md` (see previous implementation)

### 8.3 Create .gitignore
Create `.gitignore`:
```gitignore
# Build outputs
**/bin/
**/obj/
**/target/
**/dist/
**/build/

# IDE files
.vs/
.vscode/
*.suo
*.user
*.userosscache
*.sln.docstates
.idea/

# Environment files
.env.local
.env.production

# Logs
**/*.log
logs/

# Database
*.db
*.sqlite

# Node modules
node_modules/

# Docker
.docker/

# OS generated files
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
```

## 🚀 Step 9: Build and Test

### 9.1 Build All Services
```powershell
# Windows
.\run-demo.ps1 build

# Linux/macOS  
./run-demo.sh build
```

### 9.2 Start the Complete Environment
```powershell
# Windows
.\run-demo.ps1 start

# Linux/macOS
./run-demo.sh start
```

### 9.3 Run Performance Tests
```powershell
# Windows
.\run-demo.ps1 test

# Linux/macOS
./run-demo.sh test
```

### 9.4 Verify All Services
```powershell
# Windows
.\run-demo.ps1 health

# Linux/macOS
./run-demo.sh health
```

## 🔧 Step 10: Advanced Features (Optional)

### 10.1 Add React Dashboard
- Create React application for monitoring
- Implement real-time metrics display
- Add performance test controls

### 10.2 Add Database Integration
- Set up SQL Server container
- Implement Entity Framework Core
- Add database performance testing

### 10.3 Add Security Features
- Implement JWT authentication
- Add API rate limiting
- Configure HTTPS/TLS

### 10.4 Add CI/CD Pipeline
- Create GitHub Actions workflows
- Implement automated testing
- Set up container registry publishing

## 🎉 Completion

After following all these steps, you'll have a complete Advanced API Performance Tuning demonstration environment with:

✅ **Multi-language APIs** (.NET 8, Spring Boot)  
✅ **gRPC Services** with reflection and client testing  
✅ **Circuit Breakers** and resilience patterns  
✅ **Distributed Tracing** with OpenTelemetry and Jaeger  
✅ **Metrics Collection** with Prometheus  
✅ **Performance Testing** with K6  
✅ **Visualization** with Grafana  
✅ **Environment Configuration** with flexible variable management  
✅ **Automation Scripts** for easy setup and testing  
✅ **Comprehensive Documentation** for all features  

The project demonstrates real-world performance optimization techniques, monitoring strategies, and resilience patterns essential for building scalable, reliable distributed systems.

## 🔄 Next Steps

1. **Experiment** with different performance scenarios
2. **Monitor** application behavior under various loads
3. **Optimize** based on observed bottlenecks  
4. **Scale** services horizontally using Docker Swarm or Kubernetes
5. **Implement** additional resilience patterns (retry, timeout, bulkhead)
6. **Add** security features and authentication
7. **Create** custom Grafana dashboards for specific metrics
8. **Integrate** with cloud platforms (Azure, AWS, GCP) for production deployment