using Microsoft.AspNetCore.Mvc;

namespace dotnet8_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CorsTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Message = "CORS is working!",
            Timestamp = DateTime.UtcNow,
            Origin = Request.Headers["Origin"].ToString(),
            Method = Request.Method,
            Headers = Request.Headers.Select(h => new { h.Key, Value = h.Value.ToString() }).ToList()
        });
    }

    [HttpOptions]
    public IActionResult Options()
    {
        return Ok(new
        {
            Message = "CORS preflight successful",
            Timestamp = DateTime.UtcNow,
            AllowedOrigins = new[] { "http://localhost:3000", "http://localhost:3001" },
            AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
            AllowedHeaders = new[] { "Content-Type", "Authorization" }
        });
    }

    [HttpPost]
    public IActionResult Post([FromBody] object data)
    {
        return Ok(new
        {
            Message = "POST request successful with CORS",
            Timestamp = DateTime.UtcNow,
            ReceivedData = data,
            Origin = Request.Headers["Origin"].ToString()
        });
    }
}