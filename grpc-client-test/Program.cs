using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using GrpcClientTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add gRPC client
builder.Services.AddGrpcClient<Product.ProductClient>(options =>
{
    options.Address = new Uri("http://dotnet-app:8080"); // Docker service name
});

// Register our gRPC client service
builder.Services.AddScoped<ProductGrpcClientService>();

// Add OpenTelemetry
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "grpc-client-test";
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4318/v1/traces";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();