using System.Reflection;

namespace ReflectionInspector.Services;

public class ObjectInspector
{
    public static void Inspect(object obj)
    {
        Type type = obj.GetType();
        
        Console.WriteLine($"=== Инспекция типа {type.Name} ===\n");
        
        // Свойства
        Console.WriteLine("СВОЙСТВА:");
        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo prop in properties)
        {
            object? value = prop.GetValue(obj);
            Console.WriteLine($"  {prop.Name} ({prop.PropertyType.Name}) = {value}");
        }
        
        // Методы
        Console.WriteLine("\nМЕТОДЫ:");
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (MethodInfo method in methods)
        {
            string parameters = string.Join(", ", 
                Array.ConvertAll(method.GetParameters(), 
                    p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  {method.ReturnType.Name} {method.Name}({parameters})");
        }
        Console.WriteLine();
    }
}
