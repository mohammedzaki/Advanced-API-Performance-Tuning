using Microsoft.AspNetCore.Mvc;
using GrpcClientTest.Services;

namespace GrpcClientTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrpcTestController : ControllerBase
    {
        private readonly ProductGrpcClientService _grpcClientService;
        private readonly ILogger<GrpcTestController> _logger;

        public GrpcTestController(ProductGrpcClientService grpcClientService, ILogger<GrpcTestController> logger)
        {
            _grpcClientService = grpcClientService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "grpc-client-test",
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _grpcClientService.GetProductByIdAsync(id);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = product.Id,
                        name = product.Name,
                        price = product.Price,
                        description = product.Description
                    },
                    source = "grpc-call"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get product {ProductId} via gRPC", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get product via gRPC",
                    error = ex.Message
                });
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _grpcClientService.GetAllProductsAsync();
                return Ok(new
                {
                    success = true,
                    data = products.Products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        description = p.Description
                    }).ToList(),
                    count = products.Products.Count,
                    source = "grpc-call"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get products via gRPC");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get products via gRPC",
                    error = ex.Message
                });
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test with a simple product request
                var product = await _grpcClientService.GetProductByIdAsync(1);
                var products = await _grpcClientService.GetAllProductsAsync();

                return Ok(new
                {
                    success = true,
                    message = "gRPC connection test successful",
                    testResults = new
                    {
                        singleProduct = new
                        {
                            id = product.Id,
                            name = product.Name,
                            price = product.Price
                        },
                        totalProducts = products.Products.Count
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC connection test failed");
                return StatusCode(500, new
                {
                    success = false,
                    message = "gRPC connection test failed",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}