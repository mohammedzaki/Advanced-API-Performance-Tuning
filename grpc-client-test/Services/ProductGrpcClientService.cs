using Microsoft.Extensions.Logging;

namespace GrpcClientTest.Services
{
    public class ProductGrpcClientService
    {
        private readonly Product.ProductClient _grpcClient;
        private readonly ILogger<ProductGrpcClientService> _logger;

        public ProductGrpcClientService(Product.ProductClient grpcClient, ILogger<ProductGrpcClientService> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }

        public async Task<ProductResponse> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Calling gRPC GetProduct for ID: {ProductId}", id);
            
            try
            {
                var request = new ProductRequest { Id = id };
                var response = await _grpcClient.GetProductAsync(request);
                
                _logger.LogInformation("Successfully received product: {ProductName}", response.Name);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC GetProduct for ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductListResponse> GetAllProductsAsync()
        {
            _logger.LogInformation("Calling gRPC GetProducts");
            
            try
            {
                var request = new Empty();
                var response = await _grpcClient.GetProductsAsync(request);
                
                _logger.LogInformation("Successfully received {ProductCount} products", response.Products.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling gRPC GetProducts");
                throw;
            }
        }
    }
}