using System.Reflection;

public static partial class DebugUtils
{
        private static bool IsSetType<T>(T obj)
    {
        var type = obj?.GetType();
        return type?.IsGenericType == true &&
               (type.GetGenericTypeDefinition() == typeof(HashSet<>) ||
                type.GetGenericTypeDefinition() == typeof(SortedSet<>) ||
                type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(ISet<>)));
    }
    private static bool IsQueueType<T>(T obj)
    {
        var type = obj?.GetType();
        return type?.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(Queue<>);
    }
    private static bool IsStackType<T>(T obj)
    {
        var type = obj?.GetType();
        return type?.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(Stack<>);
    }
    private static bool IsRecord(Type type)
    {
        // Records have a synthesized method called <Clone>$
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                   .Any(m => m.Name == "<Clone>$") ||
               // Alternative: check for EqualityContract property (records have this)
               type.GetProperty("EqualityContract",
                   BindingFlags.NonPublic | BindingFlags.Instance) != null;
    }
    private static bool IsEnum(Type type)
    {
        return type.IsEnum;
    }
    private static bool OverridedToString<T>(T t) where T : notnull
    {
        var type = t.GetType();
        // Check for explicit ToString() override
        var toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
        return toStringMethod?.DeclaringType == type;
    }
    private static string ReprWithFields<T>(T t, Type type, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        var parts = new List<string>();

        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        // Get public fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(t);
            parts.Add($"{field.Name}: {value.Repr(floatReprConfig, intReprConfig)}");
        }

        // Get public properties with getters
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetMethod?.IsPublic == true);
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(t);
                parts.Add($"{prop.Name}: {value.Repr(floatReprConfig, intReprConfig)}");
            }
            catch
            {
                parts.Add($"{prop.Name}: <error>");
            }
        }

        var content = parts.Count > 0
            ? string.Join(", ", parts)
            : "";
        return $"{type.Name}({content})";
    }
}