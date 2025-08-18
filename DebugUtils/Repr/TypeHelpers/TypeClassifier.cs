using System.Reflection;
using System.Runtime.CompilerServices;
using DebugUtils.Repr.Attributes;

namespace DebugUtils.Repr.TypeHelpers;

internal static class TypeClassifier
{
    public static bool IsSignedPrimitiveType(this Type type)
    {

        return type == typeof(sbyte)
               || type == typeof(short)
               || type == typeof(int)
               || type == typeof(long)
            #if NET7_0_OR_GREATER
               || type == typeof(Int128)
            #endif
            ;
    }
    public static bool IsIntegerPrimitiveType(this Type type)
    {
        return type.IsSignedPrimitiveType()
               || type == typeof(byte)
               || type == typeof(ushort)
               || type == typeof(uint)
               || type == typeof(ulong)
            #if NET7_0_OR_GREATER
               || type == typeof(Int128)
            #endif
            ;
    }
    public static bool IsFloatType(this Type type)
    {
        return type == typeof(float)
               || type == typeof(double)
               #if NET5_0_OR_GREATER
               || type == typeof(Half)
            #endif
            ;
    }
    public static bool IsDictionaryType(this Type type)
    {
        return type.IsGenericType &&
               type.GetInterfaces()
                   .Any(predicate: i => i.IsGenericType &&
                                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }
    public static bool IsSetType(this Type type)
    {
        return type.IsGenericType &&
               type.GetInterfaces()
                   .Any(predicate: i => i.IsGenericType &&
                                        i.GetGenericTypeDefinition() == typeof(ISet<>));
    }
    public static bool IsRecordType(this Type type)
    {
        // Check for EqualityContract property (records have this)
        var equalityContract = type.GetProperty(name: "EqualityContract",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return equalityContract != null;
    }
    public static bool IsTupleType(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var genericDef = type.GetGenericTypeDefinition();

        // ValueTuple types (modern tuples)
        return genericDef == typeof(ValueTuple<>) ||
               genericDef == typeof(ValueTuple<,>) ||
               genericDef == typeof(ValueTuple<,,>) ||
               genericDef == typeof(ValueTuple<,,,>) ||
               genericDef == typeof(ValueTuple<,,,,>) ||
               genericDef == typeof(ValueTuple<,,,,,>) ||
               genericDef == typeof(ValueTuple<,,,,,,>) ||
               genericDef == typeof(ValueTuple<,,,,,,,>) ||
               // Legacy Tuple types
               genericDef == typeof(Tuple<>) ||
               genericDef == typeof(Tuple<,>) ||
               genericDef == typeof(Tuple<,,>) ||
               genericDef == typeof(Tuple<,,,>) ||
               genericDef == typeof(Tuple<,,,,>) ||
               genericDef == typeof(Tuple<,,,,,>) ||
               genericDef == typeof(Tuple<,,,,,,>) ||
               genericDef == typeof(Tuple<,,,,,,,>);
    }
    public static bool IsPriorityQueueType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PriorityQueue<,>);
    }
    public static bool IsEnumType(this Type type)
    {
        return type.IsEnum;
    }
    public static bool IsNullableStructType(this Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    public static bool IsMemoryType(this Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Memory<>);
    }
    public static bool IsReadOnlyMemoryType(this Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>);
    }
    public static bool OverridesToStringType(this Type type)
    {
        // Check for explicit ToString() override
        var toStringMethod = type.GetMethod(name: "ToString", types: Type.EmptyTypes);
        return toStringMethod?.DeclaringType == type;
    }
    public static bool NeedsTypePrefixType(this Type type)
    {
        // Types that never need a prefix
        if (type.IsNullableStructType()
            || type.IsAssignableTo(targetType: typeof(Delegate))
            || type.IsGenericTypeOf(genericTypeDefinition: typeof(List<>))
            || type.IsGenericTypeOf(genericTypeDefinition: typeof(Dictionary<,>))
            || type.IsGenericTypeOf(genericTypeDefinition: typeof(HashSet<>))
            || type.IsAssignableTo(targetType: typeof(ITuple))
            || type.IsEnum
           )
        {
            return false;
        }

        // Check if the formatter for this type has a ReprOptions attribute
        var formatter = type.GetStandardFormatter();
        var formatterAttr = formatter.GetType()
                                     .GetCustomAttribute<ReprOptionsAttribute>();
        return formatterAttr?.NeedsPrefix ?? type.IsRecordType();
    }
    public static bool IsGenericTypeOf(this Type type, Type genericTypeDefinition)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == genericTypeDefinition;
    }

    public static bool IsAnonymousType(this Type type)
    {
        // An anonymous class is always generic and not public.
        // Also, its type name starts with "<>" or "VB$", and contains AnonymousType.
        // C# compiler marks anonymous types with the System.Runtime.CompilerServices.CompilerGeneratedAttribute
        return Attribute.IsDefined(element: type,
                   attributeType: typeof(CompilerGeneratedAttribute))
               && type.IsGenericType
               && type.Name.Contains(value: "AnonymousType")
               && (type.Name.StartsWith(value: "<>") || type.Name.StartsWith(value: "VB$"));
    }
}