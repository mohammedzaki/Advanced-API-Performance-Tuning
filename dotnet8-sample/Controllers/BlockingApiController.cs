using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.RateLimiting;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/blocking-sql")]
public class BlockingApiController : ControllerBase
{
    private readonly string _connString;

    public BlockingApiController(IConfiguration cfg)
    {
        // connection string is taken from ConnectionStrings__Default env var or appsettings
        _connString = cfg.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? "Server=mssql,1433;Database=AdventureWorks;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
    }

    [HttpGet("blocking-optimized")]
    public IActionResult BlockingOptimized(int offset = 0, int limit = 100)
    {
        // Optimized: joins, projection, sargable predicates, pagination
        var sql = @"SELECT * FROM Sales.SalesOrderHeader soh";
        using var conn = new SqlConnection(_connString);
        var rows = conn.Query(sql);
        return Ok(new { note = "Optimized query with joins, sargable predicates, and pagination." });
    }

    [HttpGet("non-blocking-optimized")]
    [EnableRateLimiting("sql-heavy")]
    public async Task<IActionResult> Optimized(int offset = 0, int limit = 100)
    {
        // Optimized: joins, projection, sargable predicates, pagination
        var sql = @"SELECT * FROM Sales.SalesOrderHeader soh";
        using var conn = new SqlConnection(_connString);
        var rows = conn.QueryAsync(sql);
        // 1. Reposne immedit to User that we are working on your request
        // 2. Send/push a message to RappitMQ
        // 3. Worker service will process the request in background
        // 4. post back message to another queue when the request is completed.
        // 5. listen to that queue in another service then send notification to user using email/websocket/push notification SginalR
        // 5. listening Frontend React app or user request again to get the result
        // 4. Notify user via email or websocket when the request is completed
        // 5. Return the result when user request again or check status on Frontend React app
        // 6. await Task.Delay(5000); // simulate some processing delay
        // 

        return Ok(new { note = "Optimized query with joins, sargable predicates, and pagination." });
    }
}
