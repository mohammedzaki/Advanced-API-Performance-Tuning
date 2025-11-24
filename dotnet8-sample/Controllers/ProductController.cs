using Microsoft.AspNetCore.Mvc;
using dotnet_sample.Services;
using dotnet_sample.Models;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _svc;

    public ProductsController(ProductService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        return Ok(_svc.GetAll());
    }

    [HttpGet("{id:long}")]
    public ActionResult<Product?> GetById(long id)
    {
        var p = _svc.GetById(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpGet("/actuator/health")]
    public ActionResult<string> Health() => Ok("UP");
}
