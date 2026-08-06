namespace ReflectionInspector.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
    }
}
