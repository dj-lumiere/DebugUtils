using System.Reflection;
using System.Runtime.CompilerServices;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.TypeLibraries;

internal static class TypeInspector
{
    #region Type Check By Type

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
        return type?.IsGenericType == true &&
               type.GetInterfaces()
                   .Any(predicate: i => i.IsGenericType &&
                                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }
    public static bool IsSetType(this Type type)
    {
        return type?.IsGenericType == true &&
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
    public static bool IsEnumType(this Type type)
    {
        return type.IsEnum;
    }
    public static bool IsNullableStructType(this Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
        var formatter = ReprFormatterRegistry.GetFormatter(type, ReprConfig.GlobalDefaults);
        var formatterAttr = formatter.GetType().GetCustomAttribute<ReprOptionsAttribute>();
        if (formatterAttr != null)
        {
            return formatterAttr.NeedsPrefix;
        }
        
        // Record types and types that doesn't override ToString need type prefix.
        return type.IsRecordType() &&!type.OverridesToStringType();
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
               && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic
               && type.Name.Contains(value: "AnonymousType")
               && (type.Name.StartsWith(value: "<>") || type.Name.StartsWith(value: "VB$"));
    }

    #endregion

    #region Type Check By Object

    public static bool IsSignedPrimitive<T>(this T obj)
    {
        var type = obj?.GetType();
        return type?.IsSignedPrimitiveType() ?? false;
    }
    public static bool IsIntegerPrimitive<T>(this T obj)
    {
        var type = obj?.GetType();
        return type?.IsIntegerPrimitiveType() ?? false;
    }
    public static bool IsFloat<T>(this T obj)
    {
        var type = obj?.GetType();
        return type?.IsFloatType() ?? false;
    }
    public static bool IsDictionary<T>(this T obj)
    {
        var type = obj?.GetType();
        return type?.IsDictionaryType() ?? false;
    }
    public static bool IsSet<T>(this T obj)
    {
        var type = obj?.GetType();
        return type?.IsSetType() ?? false;
    }
    public static bool IsRecord<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);
        return type.IsRecordType();
    }
    public static bool IsEnum<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);
        return type.IsEnumType();
    }
    public static bool IsNullableStruct<T>(this T obj)
    {
        // Check the generic parameter first
        var compileType = typeof(T);
        // For cases where T is object but obj is actually nullable
        var runtimeType = obj?.GetType();
        return compileType.IsNullableStructType() ||
               (runtimeType?.IsNullableStructType() ?? false);
    }
    public static bool OverridesToString<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);
        return type.OverridesToStringType();
    }
    public static bool NeedsTypePrefix<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);
        return type.NeedsTypePrefixType();
    }

    #endregion
}