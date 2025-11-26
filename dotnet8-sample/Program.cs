using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

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
