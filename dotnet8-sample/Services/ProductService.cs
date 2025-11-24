using dotnet_sample.Models;
using System.Collections.Concurrent;

namespace dotnet_sample.Services;

public class ProductService
{
    private readonly ConcurrentDictionary<long, Product> _items = new();
    private long _seq = 1;

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
        // small simulated processing delay
        System.Threading.Thread.Sleep(5);
        return _items.Values;
    }

    public Product? GetById(long id)
    {
        _items.TryGetValue(id, out var p);
        return p;
    }
}
