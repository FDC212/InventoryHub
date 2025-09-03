// filepath: c:\Users\fabri\FullStackApp\ClientAppdotnet\Models\Product.cs
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }
    public required Category Category { get; set; }
}
public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
}