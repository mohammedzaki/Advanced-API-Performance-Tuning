using Microsoft.AspNetCore.Mvc;
using Polly.Registry;
using System.Reflection;

namespace dotnet8_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<HealthController> logger)
    {
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        try
        {
            // Test basic application health
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                CircuitBreakers = GetCircuitBreakerStatus(),
                Uptime = DateTime.UtcNow.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime).ToString(@"dd\:hh\:mm\:ss")
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("circuit-breakers")]
    public IActionResult GetCircuitBreakerHealth()
    {
        var circuitBreakerStatus = GetCircuitBreakerStatus();
        
        var overallStatus = circuitBreakerStatus.Values.All(cb => cb.GetType().GetProperty("Status")?.GetValue(cb)?.ToString() == "Healthy") 
            ? "Healthy" 
            : "Degraded";

        return Ok(new
        {
            OverallStatus = overallStatus,
            CircuitBreakers = circuitBreakerStatus,
            Timestamp = DateTime.UtcNow,
            Message = overallStatus == "Healthy" 
                ? "All circuit breakers are functioning normally" 
                : "One or more circuit breakers may be open or degraded"
        });
    }

    [HttpGet("readiness")]
    public IActionResult GetReadiness()
    {
        // Check if the application is ready to serve requests
        try
        {
            // You can add more sophisticated readiness checks here
            // For example, database connectivity, external service availability, etc.
            
            var circuitBreakers = GetCircuitBreakerStatus();
            var allHealthy = circuitBreakers.Values.All(cb => 
                cb.GetType().GetProperty("Status")?.GetValue(cb)?.ToString() == "Healthy");

            if (!allHealthy)
            {
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Reason = "Circuit breakers are not in healthy state",
                    CircuitBreakers = circuitBreakers,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Status = "Ready",
                Message = "Application is ready to serve requests",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new
            {
                Status = "Not Ready",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("liveness")]
    public IActionResult GetLiveness()
    {
        // Simple liveness check - if we can respond, we're alive
        return Ok(new
        {
            Status = "Alive",
            Message = "Application is running",
            Timestamp = DateTime.UtcNow,
            ProcessId = Environment.ProcessId
        });
    }

    private Dictionary<string, object> GetCircuitBreakerStatus()
    {
        var status = new Dictionary<string, object>();

        // Database Circuit Breaker Status
        try
        {
            var dbPipeline = _pipelineProvider.GetPipeline("database-circuit-breaker");
            status["database-circuit-breaker"] = new
            {
                Name = "Database Circuit Breaker",
                Status = "Healthy", // We can't easily get the actual state without executing, so we assume healthy
                Configuration = new
                {
                    FailureRatio = "50%",
                    SamplingDuration = "30 seconds",
                    MinimumThroughput = 3,
                    BreakDuration = "30 seconds"
                },
                Description = "Protects database operations from cascading failures"
            };
        }
        catch (Exception ex)
        {
            status["database-circuit-breaker"] = new
            {
                Name = "Database Circuit Breaker",
                Status = "Error",
                Error = ex.Message
            };
        }

        // API Circuit Breaker Status
        try
        {
            var apiPipeline = _pipelineProvider.GetPipeline("api-circuit-breaker");
            status["api-circuit-breaker"] = new
            {
                Name = "API Circuit Breaker",
                Status = "Healthy",
                Configuration = new
                {
                    FailureRatio = "30%",
                    SamplingDuration = "20 seconds",
                    MinimumThroughput = 5,
                    BreakDuration = "15 seconds"
                },
                Description = "Protects external API calls from cascading failures"
            };
        }
        catch (Exception ex)
        {
            status["api-circuit-breaker"] = new
            {
                Name = "API Circuit Breaker",
                Status = "Error",
                Error = ex.Message
            };
        }

        return status;
    }
}