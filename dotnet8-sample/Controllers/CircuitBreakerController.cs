using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using System.Diagnostics;

namespace dotnet8_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CircuitBreakerController : ControllerBase
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<CircuitBreakerController> _logger;
    private static int _failureCount = 0;
    private static int _requestCount = 0;

    public CircuitBreakerController(
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<CircuitBreakerController> logger)
    {
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }

    [HttpGet("database-test")]
    public async Task<IActionResult> TestDatabaseCircuitBreaker([FromQuery] bool simulateFailure = false)
    {
        var pipeline = _pipelineProvider.GetPipeline("database-circuit-breaker");
        var requestId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            var result = await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                Interlocked.Increment(ref _requestCount);
                _logger.LogInformation($"[{requestId}] Executing database operation. Request #{_requestCount}");

                // Simulate database operation
                await Task.Delay(100, cancellationToken);

                // Simulate failure if requested
                if (simulateFailure)
                {
                    var currentFailureCount = Interlocked.Increment(ref _failureCount);
                    _logger.LogWarning($"[{requestId}] Simulated database failure #{currentFailureCount}");
                    throw new InvalidOperationException($"Simulated database failure #{currentFailureCount}");
                }

                return new
                {
                    RequestId = requestId,
                    Message = "Database operation successful",
                    RequestCount = _requestCount,
                    FailureCount = _failureCount,
                    Timestamp = DateTime.UtcNow,
                    CircuitState = "Closed" // If we reach here, circuit is closed
                };
            });

            return Ok(result);
        }
        catch (Exception ex) when (ex.GetType().Name == "BrokenCircuitException")
        {
            _logger.LogError($"[{requestId}] Circuit breaker is OPEN - rejecting request");
            return StatusCode(503, new
            {
                RequestId = requestId,
                Error = "Circuit breaker is OPEN",
                Message = "Database service is temporarily unavailable",
                CircuitState = "Open",
                RequestCount = _requestCount,
                FailureCount = _failureCount,
                Timestamp = DateTime.UtcNow,
                RetryAfter = "30 seconds"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{requestId}] Database operation failed");
            return StatusCode(500, new
            {
                RequestId = requestId,
                Error = ex.Message,
                CircuitState = "Closed", // Exception occurred, but circuit might still be closed
                RequestCount = _requestCount,
                FailureCount = _failureCount,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("api-test")]
    public async Task<IActionResult> TestApiCircuitBreaker([FromQuery] bool simulateFailure = false)
    {
        var pipeline = _pipelineProvider.GetPipeline("api-circuit-breaker");
        var requestId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            var result = await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                var currentRequest = Interlocked.Increment(ref _requestCount);
                _logger.LogInformation($"[{requestId}] Executing API call. Request #{currentRequest}");

                // Simulate API call
                await Task.Delay(50, cancellationToken);

                // Simulate failure if requested
                if (simulateFailure)
                {
                    var currentFailureCount = Interlocked.Increment(ref _failureCount);
                    _logger.LogWarning($"[{requestId}] Simulated API failure #{currentFailureCount}");
                    throw new HttpRequestException($"Simulated API failure #{currentFailureCount}");
                }

                return new
                {
                    RequestId = requestId,
                    Message = "API call successful",
                    RequestCount = currentRequest,
                    FailureCount = _failureCount,
                    Timestamp = DateTime.UtcNow,
                    CircuitState = "Closed"
                };
            });

            return Ok(result);
        }
        catch (Exception ex) when (ex.GetType().Name == "BrokenCircuitException")
        {
            _logger.LogError($"[{requestId}] API Circuit breaker is OPEN - rejecting request");
            return StatusCode(503, new
            {
                RequestId = requestId,
                Error = "API Circuit breaker is OPEN",
                Message = "External API service is temporarily unavailable",
                CircuitState = "Open",
                RequestCount = _requestCount,
                FailureCount = _failureCount,
                Timestamp = DateTime.UtcNow,
                RetryAfter = "15 seconds"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{requestId}] API call failed");
            return StatusCode(500, new
            {
                RequestId = requestId,
                Error = ex.Message,
                CircuitState = "Closed",
                RequestCount = _requestCount,
                FailureCount = _failureCount,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("status")]
    public IActionResult GetCircuitBreakerStatus()
    {
        return Ok(new
        {
            DatabaseCircuitBreaker = new
            {
                Name = "database-circuit-breaker",
                Configuration = new
                {
                    FailureRatio = 0.5,
                    SamplingDuration = "30 seconds",
                    MinimumThroughput = 3,
                    BreakDuration = "30 seconds"
                }
            },
            ApiCircuitBreaker = new
            {
                Name = "api-circuit-breaker",
                Configuration = new
                {
                    FailureRatio = 0.3,
                    SamplingDuration = "20 seconds",
                    MinimumThroughput = 5,
                    BreakDuration = "15 seconds"
                }
            },
            Statistics = new
            {
                TotalRequests = _requestCount,
                TotalFailures = _failureCount,
                SuccessRate = _requestCount > 0 ? (double)(_requestCount - _failureCount) / _requestCount * 100 : 100,
                Timestamp = DateTime.UtcNow
            }
        });
    }

    [HttpPost("reset")]
    public IActionResult ResetCounters()
    {
        var oldRequestCount = _requestCount;
        var oldFailureCount = _failureCount;
        
        Interlocked.Exchange(ref _requestCount, 0);
        Interlocked.Exchange(ref _failureCount, 0);

        _logger.LogInformation($"Circuit breaker counters reset. Previous: Requests={oldRequestCount}, Failures={oldFailureCount}");

        return Ok(new
        {
            Message = "Counters reset successfully",
            PreviousValues = new
            {
                Requests = oldRequestCount,
                Failures = oldFailureCount
            },
            NewValues = new
            {
                Requests = _requestCount,
                Failures = _failureCount
            },
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("simulate-load")]
    public async Task<IActionResult> SimulateLoad([FromQuery] int requests = 10, [FromQuery] double failureRate = 0.5)
    {
        var tasks = new List<Task<object>>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < requests; i++)
        {
            var shouldFail = Random.Shared.NextDouble() < failureRate;
            var delay = Random.Shared.Next(50, 200); // Random delay between requests
            
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(delay);
                
                try
                {
                    var pipeline = _pipelineProvider.GetPipeline("database-circuit-breaker");
                    var requestId = i + 1;
                    
                    var result = await pipeline.ExecuteAsync(async (cancellationToken) =>
                    {
                        await Task.Delay(10, cancellationToken);
                        
                        if (shouldFail)
                        {
                            throw new InvalidOperationException($"Simulated failure for request {requestId}");
                        }
                        
                        return (object)new { RequestId = requestId, Status = "Success" };
                    });
                    
                    return result;
                }
                catch (Exception ex) when (ex.GetType().Name == "BrokenCircuitException")
                {
                    return (object)new { RequestId = i + 1, Status = "CircuitOpen" };
                }
                catch (Exception ex)
                {
                    return (object)new { RequestId = i + 1, Status = "Failed", Error = ex.Message };
                }
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var successful = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() == "Success");
        var failed = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() == "Failed");
        var circuitOpen = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() == "CircuitOpen");

        return Ok(new
        {
            LoadTest = new
            {
                TotalRequests = requests,
                TargetFailureRate = failureRate,
                ExecutionTime = stopwatch.ElapsedMilliseconds + "ms"
            },
            Results = new
            {
                Successful = successful,
                Failed = failed,
                CircuitOpen = circuitOpen
            },
            CircuitBreakerStats = new
            {
                TotalRequests = _requestCount,
                TotalFailures = _failureCount,
                CurrentSuccessRate = _requestCount > 0 ? (double)(_requestCount - _failureCount) / _requestCount * 100 : 100
            },
            Timestamp = DateTime.UtcNow
        });
    }
}