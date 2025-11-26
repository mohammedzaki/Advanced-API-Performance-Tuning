using dotnet_sample.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace dotnet_sample.Services;

public class ProductService
{
    private readonly ConcurrentDictionary<long, Product> _items = new();
    private long _seq = 1;
    private static readonly ActivitySource ActivitySource = new("dotnet-sample.ProductService");

    public ProductService()
    {
        // seed sample data
        for (int i = 1; i <= 200; i++)
        {
            var id = System.Threading.Interlocked.Increment(ref _seq);
            _items[id] = new Product(id, $"Product {i}", (i % 2 == 0) ? "Category A" : "Category B", Math.Round((10 + new Random().NextDouble() * 90) * 100.0) / 100.0);
        }
    }

    public IEnumerable<Product> GetAll()
    {
        using var activity = ActivitySource.StartActivity("get-all-products-service");
        // small simulated processing delay
        System.Threading.Thread.Sleep(5);
        activity?.SetTag("products.count", _items.Count);
        return _items.Values;
    }

    public async Task<IEnumerable<Product>> GetAllDelayed()
    {
        using var activity = ActivitySource.StartActivity("get-all-products-delayed-service");
        // Simulate random delay between 1-15 seconds
        var delayMs = Random.Shared.Next(1000, 15000);
        activity?.SetTag("delay.milliseconds", delayMs);
        
        await Task.Delay(delayMs);
        activity?.SetTag("products.count", _items.Count);
        return _items.Values;
    }

    public Product? GetById(long id)
    {
        using var activity = ActivitySource.StartActivity("get-product-by-id-service");
        activity?.SetTag("product.id", id);
        _items.TryGetValue(id, out var p);
        activity?.SetTag("product.found", p != null);
        return p;
    }
}
