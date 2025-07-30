using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DebugUtils.Formatters;

internal static class TypeInspector
{
    #region Type Check By Type

    public static bool IsSignedPrimitiveType(this Type type)
    {
        return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) ||
               type == typeof(long);
    }
    public static bool IsIntegerPrimitiveType(this Type type)
    {
        return type.IsSignedPrimitiveType() || type == typeof(byte) || type == typeof(uint) ||
               type == typeof(ulong) || type == typeof(ushort);
    }
    #if NET7_0_OR_GREATER
    public static bool IsSignedPrimitiveTypeAfter7(this Type type)
    {
        return type == typeof(Int128);
    }
    public static bool IsIntegerPrimitiveTypeAfter7(this Type type)
    {
        return type == typeof(Int128) || type == typeof(UInt128);
    }
    #endif
    public static bool IsFloatType(this Type type)
    {
        return type == typeof(float) || type == typeof(double) || type == typeof(Half);
    }
    public static bool IsDictionaryType(this Type type)
    {
        return type?.IsGenericType == true &&
               type.GetInterfaces()
                   .Any(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }
    public static bool IsSetType(this Type type)
    {
        return type?.IsGenericType == true &&
               type.GetInterfaces()
                   .Any(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(ISet<>));
    }
    public static bool IsRecordType(this Type type)
    {
        // Check for EqualityContract property (records have this)
        var equalityContract = type.GetProperty("EqualityContract",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
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
        var toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
        return toStringMethod?.DeclaringType == type;
    }
    public static bool NeedsTypePrefixType(this Type type)
    {
        // nothing is attached
        if (type.IsNullableStructType())
        {
            return false;
        }

        if (type == typeof(string) || type == typeof(char) || type == typeof(bool))
        {
            return false;
        }

        if (type == typeof(Rune) || type == typeof(TimeSpan) || type == typeof(DateTime) ||
            type == typeof(DateTimeOffset))
        {
            return true;
        }

        #if NET7_0_OR_GREATER
        if (type == typeof(Int128) || type == typeof(UInt128))
        {
            return true;
        }
        #endif
        
        if (type.IsIntegerPrimitiveType() || type.IsFloatType() || type == typeof(decimal) ||
            type == typeof(BigInteger))
        {
            return true;
        }

        // will use a special type prefix
        if (type.IsArray)
        {
            return true;
        }

        // nothing is attached
        if (type.IsGenericTypeOf(typeof(List<>)) || type.IsGenericTypeOf(typeof(Dictionary<,>)) ||
            type.IsGenericTypeOf(typeof(HashSet<>)))
        {
            return false;
        }


        if (type.IsAssignableTo(typeof(ITuple)) ||
            type.IsEnum)
        {
            return false;
        }

        if (type.IsRecordType())
        {
            return true;
        }

        if (type.IsValueType && type.OverridesToStringType())
        {
            return false;
        }

        if (type.OverridesToStringType())
        {
            return false;
        }

        return true;
    }
    public static bool IsGenericTypeOf(this Type type, Type genericTypeDefinition)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == genericTypeDefinition;
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