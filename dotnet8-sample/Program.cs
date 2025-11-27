using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
//builder.Services.AddSwaggerGen();

// Configure Kestrel for gRPC (HTTP/2) 
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
    options.ListenAnyIP(8085, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// register product service
builder.Services.AddSingleton<dotnet_sample.Services.ProductService>();

// Add OpenTelemetry
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "dotnet-sample";
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";

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
