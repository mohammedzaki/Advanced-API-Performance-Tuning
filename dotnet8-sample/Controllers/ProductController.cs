using Microsoft.AspNetCore.Mvc;
using dotnet_sample.Services;
using dotnet_sample.Models;
using System.Diagnostics;

namespace dotnet_sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _svc;
    private static readonly ActivitySource ActivitySource = new("dotnet-sample.ProductController");

    public ProductsController(ProductService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        using var activity = ActivitySource.StartActivity("get-all-products");
        var products = _svc.GetAll();
        activity?.SetTag("product.count", products.Count());
        return Ok(products);
    }

    [HttpGet("delayed")]
    public async Task<ActionResult<IEnumerable<Product>>> GetDelayed()
    {
        using var activity = ActivitySource.StartActivity("get-all-products-delayed");
        activity?.SetTag("operation.type", "slow");
        var products = await _svc.GetAllDelayed();
        activity?.SetTag("products.returned", products.Count());
        return Ok(products);
    }

    [HttpGet("{id:long}")]
    public ActionResult<Product?> GetById(long id)
    {
        using var activity = ActivitySource.StartActivity("get-product-by-id");
        activity?.SetTag("product.id", id);
        var p = _svc.GetById(id);
        if (p == null) 
        {
            activity?.SetTag("product.found", false);
            return NotFound();
        }
        activity?.SetTag("product.found", true);
        activity?.SetTag("product.name", p.Name);
        return Ok(p);
    }

    [HttpGet("/actuator/health")]
    public ActionResult<string> Health() => Ok("UP");
}
