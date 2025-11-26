using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ProtoServices
{
    public class ProductService : Product.ProductBase
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public override Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);
            
            var response = new ProductResponse
            {
                Id = request.Id,
                Name = "Sample Product",
                Price = 99.99,
                Description = "This is a sample product"
            };

            return Task.FromResult(response);
        }

        public override Task<ProductListResponse> GetProducts(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("Getting all products");
            
            var response = new ProductListResponse();
            response.Products.Add(new ProductResponse
            {
                Id = 1,
                Name = "Product 1",
                Price = 50.00,
                Description = "First product"
            });
            response.Products.Add(new ProductResponse
            {
                Id = 2,
                Name = "Product 2", 
                Price = 75.00,
                Description = "Second product"
            });

            return Task.FromResult(response);
        }
    }
}