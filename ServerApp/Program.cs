using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add in-memory caching service
builder.Services.AddMemoryCache();

var app = builder.Build();

const string cacheKey = "ProductListCache";

// READ: Get all products
app.MapGet("/api/productslist", (IMemoryCache cache) =>
{
    if (!cache.TryGetValue(cacheKey, out List<Product>? products))
    {
        // Data not in cache, so generate it
        products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25, Category = new Category { Id = 101, Name = "Electronics" } },
            new Product { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100, Category = new Category { Id = 102, Name = "Accessories" } },
        };

        // Set cache options
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        // Save data in the cache
        cache.Set(cacheKey, products, cacheEntryOptions);
    }

    return products;
});

// CREATE: Add a new product
app.MapPost("/api/productslist", (IMemoryCache cache, Product newProduct) =>
{
    if (!cache.TryGetValue(cacheKey, out List<Product>? products))
    {
        products = new List<Product>();
    }

    // Add the new product
    products!.Add(newProduct);

    // Update the cache
    cache.Set(cacheKey, products, new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    });

    return Results.Created($"/api/productslist/{newProduct.Id}", newProduct);
});

// UPDATE: Update an existing product
app.MapPut("/api/productslist/{id}", (IMemoryCache cache, int id, Product updatedProduct) =>
{
    if (!cache.TryGetValue(cacheKey, out List<Product>? products))
    {
        return Results.NotFound("Product list not found in cache.");
    }

    var product = products?.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }

    // Update the product
    product.Name = updatedProduct.Name;
    product.Price = updatedProduct.Price;
    product.Stock = updatedProduct.Stock;
    product.Category = updatedProduct.Category;

    // Update the cache
    cache.Set(cacheKey, products, new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    });

    return Results.Ok(product);
});

// DELETE: Remove a product
app.MapDelete("/api/productslist/{id}", (IMemoryCache cache, int id) =>
{
    if (!cache.TryGetValue(cacheKey, out List<Product>? products))
    {
        return Results.NotFound("Product list not found in cache.");
    }

    var product = products?.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }

    // Remove the product
    products?.Remove(product);

    // Update the cache
    cache.Set(cacheKey, products, new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    });

    return Results.NoContent();
});

app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.Run();