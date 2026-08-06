using System.Diagnostics;
using System.Reflection;
using ReflectionInspector.Models;

namespace ReflectionInspector.Services;

public class PerformanceTester
{
    public static void TestReflectionPerformance()
    {
        Product product = new Product { Name = "Laptop" };
        Type type = typeof(Product);
        PropertyInfo? nameProperty = type.GetProperty("Name");
        
        if (nameProperty == null)
        {
            Console.WriteLine("Свойство Name не найдено!");
            return;
        }
        
        int iterations = 1_000_000;
        
        Console.WriteLine("=== Тест производительности ===\n");
        
        // Прямой вызов
        Stopwatch sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            string name = product.Name;
        }
        sw1.Stop();
        Console.WriteLine($"Прямой вызов: {sw1.ElapsedMilliseconds} мс");
        
        // Через рефлексию
        Stopwatch sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            object? name = nameProperty.GetValue(product);
        }
        sw2.Stop();
        Console.WriteLine($"Рефлексия: {sw2.ElapsedMilliseconds} мс");
        
        double slowdown = sw2.ElapsedMilliseconds / (double)Math.Max(sw1.ElapsedMilliseconds, 1);
        Console.WriteLine($"Замедление: в {slowdown:F1} раз");
    }
}
