namespace dotnet_sample.Models;

public class Product
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public double Price { get; set; }
    public string ProductCategory { get; set; }
    public Product() { }

    public Product(long id, string? name, string? category, double price)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
        ProductCategory = dotnet_sample.Models.ProductCategory.Toys.ToString();
    }
}

public enum ProductCategory
{
    Electronics,
    Books,
    Clothing,
    Home,
    Sports,
    Toys
}
