using System.Reflection;

namespace ReflectionInspector.Services;

public class ObjectComparer
{
    public static void Compare(object obj1, object obj2)
    {
        Type type1 = obj1.GetType();
        Type type2 = obj2.GetType();
        
        if (type1 != type2)
        {
            Console.WriteLine("Объекты разных типов!");
            return;
        }
        
        Console.WriteLine($"=== Сравнение объектов типа {type1.Name} ===\n");
        
        PropertyInfo[] properties = type1.GetProperties();
        bool hasDifferences = false;
        
        foreach (PropertyInfo prop in properties)
        {
            object? value1 = prop.GetValue(obj1);
            object? value2 = prop.GetValue(obj2);
            
            if (!Equals(value1, value2))
            {
                Console.WriteLine($"  {prop.Name}: {value1} → {value2}");
                hasDifferences = true;
            }
        }
        
        if (!hasDifferences)
        {
            Console.WriteLine("  Объекты идентичны");
        }
        
        Console.WriteLine();
    }
}
