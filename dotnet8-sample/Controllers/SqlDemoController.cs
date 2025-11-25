using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/sql")]
public class SqlDemoController : ControllerBase
{
    private readonly string _connString;

    public SqlDemoController(IConfiguration cfg)
    {
        // connection string is taken from ConnectionStrings__Default env var or appsettings
        _connString = cfg.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? "Server=mssql,1433;Database=AdventureWorks;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
    }

    [HttpGet("horrible")]
    public async Task<IActionResult> Horrible()
    {
        // Intentionally horrible query: non-sargable, nested subqueries, leading wildcard, ORDER BY NEWID()
        var sql = @"
SELECT TOP (1000) *
FROM Sales.SalesOrderHeader soh
WHERE soh.SalesOrderID IN (
    SELECT SalesOrderID FROM Sales.SalesOrderDetail sod WHERE sod.ProductID IN (
        SELECT ProductID FROM Production.Product WHERE Name LIKE '%Bike%' OR ProductNumber LIKE '%BK%'
    )
)
AND YEAR(soh.OrderDate) >= 2010
AND EXISTS (
    SELECT 1 FROM Sales.SalesPerson sp WHERE sp.BusinessEntityID = soh.SalesPersonID AND sp.SalesQuota > 0
)
ORDER BY NEWID();
";

        using var conn = new SqlConnection(_connString);
        var rows = await conn.QueryAsync(sql);
        return Ok(new { rows = rows, note = "Horrible query executed (intentionally slow). Do not use in production." });
    }

    [HttpGet("optimized")]
    public async Task<IActionResult> Optimized(int offset = 0, int limit = 100)
    {
        // Optimized: joins, projection, sargable predicates, pagination
        var sql = @"
SELECT soh.SalesOrderID,
       soh.OrderDate,
       soh.SubTotal,
       soh.TotalDue,
       p.ProductID,
       p.Name AS ProductName
FROM Sales.SalesOrderHeader soh
JOIN Sales.SalesOrderDetail sod ON sod.SalesOrderID = soh.SalesOrderID
JOIN Production.Product p ON p.ProductID = sod.ProductID
LEFT JOIN Sales.SalesPerson sp ON sp.BusinessEntityID = soh.SalesPersonID
WHERE (p.Name LIKE @namePrefix OR p.ProductNumber LIKE @numPrefix)
  AND soh.OrderDate >= @fromDate
  AND sp.SalesQuota > 0
ORDER BY soh.OrderDate DESC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
";
        var parameters = new {
            namePrefix = "Bike%",
            numPrefix = "BK%",
            fromDate = new DateTime(2010,1,1),
            offset = offset,
            limit = limit
        };

        using var conn = new SqlConnection(_connString);
        var rows = await conn.QueryAsync(sql, parameters);
        return Ok(new { rows = rows, note = "Optimized query with joins, sargable predicates, and pagination." });
    }
}
