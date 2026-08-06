using ReflectionInspector.Models;
using ReflectionInspector.Services;

Console.WriteLine("=== Демонстрация работы с рефлексией ===\n");

// Создаём тестовые объекты
Product product1 = new Product 
{ 
    Id = 1, 
    Name = "Laptop", 
    Price = 999.99m 
};

Product product2 = new Product 
{ 
    Id = 1, 
    Name = "Desktop", 
    Price = 1299.99m 
};

// 1. Инспекция объекта
ObjectInspector.Inspect(product1);

// 2. Сравнение объектов
ObjectComparer.Compare(product1, product2);

// 3. Тест производительности
PerformanceTester.TestReflectionPerformance();
