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
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // More permissive policy for development
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
//builder.Services.AddSwaggerGen();

// Configure Kestrel ports - Environment variables take precedence
var httpPort = builder.Configuration.GetValue<int>("DOTNET_HTTP_PORT", 
    builder.Configuration.GetValue<int>("DOTNET_CONTAINER_HTTP_PORT", 
        builder.Configuration.GetValue<int>("Kestrel:Endpoints:Http:Port", 8081)));

var grpcPort = builder.Configuration.GetValue<int>("DOTNET_GRPC_PORT", 
    builder.Configuration.GetValue<int>("DOTNET_CONTAINER_GRPC_PORT", 
        builder.Configuration.GetValue<int>("Kestrel:Endpoints:Grpc:Port", 8083)));

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

// Log the configured ports
Console.WriteLine($"[KESTREL] HTTP/1.1 endpoint configured on port: {httpPort}");
Console.WriteLine($"[KESTREL] gRPC HTTP/2 endpoint configured on port: {grpcPort}");

// register product service
builder.Services.AddSingleton<dotnet_sample.Services.ProductService>();

// RabbitMQ configuration
var rabbitHost = builder.Configuration["RABBITMQ_HOST"] ?? "rabbitmq";
var rabbitPort = builder.Configuration.GetValue<int>("RABBITMQ_PORT", 5672);
var rabbitUser = builder.Configuration["RABBITMQ_USER"] ?? "guest";
var rabbitPass = builder.Configuration["RABBITMQ_PASS"] ?? "guest";
var rabbitExchange = builder.Configuration["RABBITMQ_EXCHANGE"] ?? "reports-exchange";
var rabbitRoutingKey = builder.Configuration["RABBITMQ_ROUTING_KEY"] ?? "sales.report";
var rabbitQueue = builder.Configuration["RABBITMQ_QUEUE"] ?? "sales-reports";

builder.Services.AddSingleton(new dotnet_sample.Services.RabbitMqPublisher(
    rabbitHost, rabbitPort, rabbitUser, rabbitPass, rabbitExchange, rabbitRoutingKey, rabbitQueue));

// Configure Circuit Breaker policies
builder.Services.AddResiliencePipeline("database-circuit-breaker", builder =>
{
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5, // Open when 50% of requests fail
        SamplingDuration = TimeSpan.FromSeconds(30), // Sample period
        MinimumThroughput = 3, // Minimum requests before evaluation
        BreakDuration = TimeSpan.FromSeconds(30), // Stay open for 30 seconds
        OnOpened = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] Database circuit opened at {DateTime.UtcNow} - Reason: {args.Outcome}");
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] Database circuit closed at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] Database circuit half-opened at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        }
    });
});

builder.Services.AddResiliencePipeline("api-circuit-breaker", builder =>
{
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.3, // More sensitive for API calls
        SamplingDuration = TimeSpan.FromSeconds(20),
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(15), // Shorter break for API
        OnOpened = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] API circuit opened at {DateTime.UtcNow} - Reason: {args.Outcome}");
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] API circuit closed at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = args =>
        {
            Console.WriteLine($"[CIRCUIT BREAKER] API circuit half-opened at {DateTime.UtcNow}");
            return ValueTask.CompletedTask;
        }
    });
});

// Configure OpenTelemetry from environment
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME_DOTNET") ?? 
                  builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? 
                  "dotnet-sample";
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4318/v1/traces";

// Build connection string from environment variables
var sqlHost = builder.Configuration.GetValue<string>("SQL_SERVER_HOST") ?? "localhost";
var sqlPort = builder.Configuration.GetValue<string>("SQL_SERVER_PORT") ?? "1433";
var sqlDatabase = builder.Configuration.GetValue<string>("SQL_SERVER_DATABASE") ?? "AdventureWorks2022";
var sqlUser = builder.Configuration.GetValue<string>("SQL_SERVER_USER") ?? "sa";
var sqlPassword = builder.Configuration.GetValue<string>("SQL_SERVER_PASSWORD") ?? "YourStrong!Passw0rd";

var connectionString = $"Server={sqlHost},{sqlPort};Database={sqlDatabase};User Id={sqlUser};Password={sqlPassword};TrustServerCertificate=True;";

// Override the connection string if environment variables are provided
builder.Configuration["ConnectionStrings:Default"] = connectionString;

// Add gRPC reflection (only in Development for security)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyDotNetService"))
    .WithTracing(tracing => tracing
        .AddSource("dotnet-sample.ProductController")
        .AddSource("dotnet-sample.ProductService")
        .AddSqlClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

// Rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
    // Default status code when rejected
    options.RejectionStatusCode = 429;

    // GLOBAL limiter: per-client IP token bucket (100 tokens, replenished each minute)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Partition by client IP (fallback to "unknown")
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // You may wish to normalize IPv6::ffff:127.0.0.1 to 127.0.0.1 if needed
        if (IPAddress.TryParse(ip, out var addr) && addr.IsIPv4MappedToIPv6)
            ip = addr.MapToIPv4().ToString();

        return RateLimitPartition.GetTokenBucketLimiter(ip, key => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,                 // bucket capacity
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,                   // no queued requests (immediate rejection)
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 100,            // refill 100 tokens each minute
            AutoReplenishment = true
        });
    });

    // Named policy for heavy SQL endpoint: tighter per-IP limit
    options.AddPolicy("sql-heavy", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (IPAddress.TryParse(ip, out var addr) && addr.IsIPv4MappedToIPv6)
            ip = addr.MapToIPv4().ToString();

        return RateLimitPartition.GetTokenBucketLimiter(ip, key => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            TokensPerPeriod = 1,
            AutoReplenishment = true
        });
    });


    // Optional: emit rate-limit headers
    options.OnRejected = (context, ct) =>
    {
        // by default middleware returns 429; add Retry-After header (seconds) if you want
        context.HttpContext.Response.Headers["Retry-After"] = "10";
        return new ValueTask();
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

// Enable CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll"); // More permissive in development
}
else
{
    app.UseCors("AllowReactApp"); // Restricted in production
}

app.UseRateLimiter();

// Configure for both HTTP/1.1 and HTTP/2
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<ProtoServices.ProductService>();

// Enable reflection endpoint in Development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
