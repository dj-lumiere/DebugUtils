using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Records;
using DebugUtils.Interfaces;

namespace DebugUtils.Formatters;

public static class ReprFormatterRegistry
{
    private static readonly Dictionary<Type, IReprFormatter> Formatters;
    private static readonly IReprFormatter intFormatter = new IntegerFormatter();
    private static readonly IReprFormatter floatFormatter = new FloatFormatter();
    private static readonly IReprFormatter degenerateFormatter = new DegenerateFormatter();

    static ReprFormatterRegistry()
    {
        Formatters = new Dictionary<Type, IReprFormatter>
        {
            [key: typeof(string)] = new StringFormatter(),
            [key: typeof(char)] = new CharFormatter(),
            [key: typeof(bool)] = new BoolFormatter(),
            [key: typeof(decimal)] = new DecimalFormatter(),
            [key: typeof(Array)] = new ArrayFormatter(),
            [key: typeof(IDictionary)] = new DictionaryFormatter(),
            [key: typeof(IEnumerable)] = new EnumerableFormatter(),
            [key: typeof(ITuple)] = new TupleFormatter(),
            [key: typeof(Rune)] = new RuneFormatter(),
            [key: typeof(nint)] = new IntPtrFormatter(),
            [key: typeof(nuint)] = new UIntPtrFormatter(),
            [key: typeof(DateTime)] = new DateTimeFormatter(),
            [key: typeof(DateTimeOffset)] = new DateTimeOffsetFormatter(),
            [key: typeof(TimeSpan)] = new TimeSpanFormatter(),
            [key: typeof(Enum)] = new EnumFormatter(),
            [key: typeof(BigInteger)] = intFormatter,
            [key: typeof(int)] = intFormatter, [key: typeof(uint)] = intFormatter,
            [key: typeof(long)] = intFormatter, [key: typeof(ulong)] = intFormatter,
            [key: typeof(short)] = intFormatter, [key: typeof(ushort)] = intFormatter,
            [key: typeof(sbyte)] = intFormatter, [key: typeof(byte)] = intFormatter,
            [key: typeof(double)] = floatFormatter, [key: typeof(float)] = floatFormatter,
#if NET5_0_OR_GREATER
            [key: typeof(Half)] = floatFormatter,
#endif
#if NET6_0_OR_GREATER
            [key: typeof(DateOnly)] = new DateOnlyFormatter(),
            [key: typeof(TimeOnly)] = new TimeOnlyFormatter(),
#endif
#if NET7_0_OR_GREATER
            [key: typeof(Int128)] = intFormatter, [key: typeof(UInt128)] = intFormatter,
#endif
        };
    }

    public static IReprFormatter GetFormatter(Type type)
    {
        if (Formatters.TryGetValue(key: type, value: out var formatter))
        {
            return formatter;
        }

        if (type.IsEnum)
        {
            return Formatters[key: typeof(Enum)];
        }

        if (type.IsRecordType())
        {
            return new RecordFormatter();
        }

        if (type.IsDictionaryType())
        {
            return Formatters[key: typeof(IDictionary)];
        }

        if (type.IsTupleType())
        {
            return Formatters[key: typeof(ITuple)];
        }

        if (type.IsArray)
        {
            return Formatters[key: typeof(Array)];
        }

        if (typeof(IEnumerable).IsAssignableFrom(c: type))
        {
            return Formatters[key: typeof(IEnumerable)];
        }

        if (type.OverridesToStringType())
        {
            return degenerateFormatter;
        }

        return new DefaultObjectFormatter();
    }

    public static void RegisterFormatter<T>(IReprFormatter formatter)
    {
        Formatters[key: typeof(T)] = formatter;
    }
}

#region Formatters

public class DegenerateFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return obj.ToString() ?? "";
    }
}

public class StringFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"\"{(string)obj}\"";
    }
}

public class CharFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((char)obj).FormatChar();
    }
}

internal static class CharFormatterLogic
{
    public static string FormatChar(this char value)
    {
        switch (value)
        {
            case '\'': return "'''"; // Single quote
            case '\"': return "'\"'"; // Double quote
            case '\\': return @"'\\'"; // Backslash
            case '\0': return @"'\0'"; // Null
            case '\a': return @"'\a'"; // Alert
            case '\b': return @"'\b'"; // Backspace
            case '\f': return @"'\f'"; // Form feed
            case '\n': return @"'\n'"; // Newline
            case '\r': return @"'\r'"; // Carriage return
            case '\t': return @"'\t'"; // Tab
            case '\v': return @"'\v'"; // Vertical tab
            case '\u00a0': return "'nbsp'"; // Non-breaking space
            case '\u00ad': return "'shy'"; // Soft Hyphen
        }

        if (Char.IsControl(c: value))
        {
            return $"'\\u{(int)value:X4}'";
        }

        return $"'{value}'";
    }
}

public class BoolFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return (bool)obj
            ? "true"
            : "false";
    }
}

public class RuneFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{(Rune)obj} @ \\U{((Rune)obj).Value:X8}";
    }
}

public class IntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((IntPtr)obj).ToInt32():X8}"
            : $"0x{((IntPtr)obj).ToInt64():X16}";
    }
}

public class UIntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((UIntPtr)obj).ToUInt32():X8}"
            : $"0x{((UIntPtr)obj).ToUInt64():X16}";
    }
}

public class DateTimeFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateTime)obj).ToString(format: "yyyy-MM-dd HH:mm:ss");
    }
}

public class DateTimeOffsetFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateTimeOffset)obj).ToString(format: "yyyy-MM-dd HH:mm:ss");
    }
}

public class TimeSpanFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((TimeSpan)obj).TotalSeconds:0.000}s";
    }
}

#if NET6_0_OR_GREATER
public class DateOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateOnly)obj).ToString(format: "yyyy-MM-dd");
    }
}

public class TimeOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((TimeOnly)obj).ToString(format: "HH:mm:ss");
    }
}

#endif

public class EnumFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{obj.GetReprTypeName()}.{obj}";
    }
}

// The DefaultObjectFormatter handles any type not specifically registered.
public class DefaultObjectFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        var parts = new List<string>();

        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj: obj);
            parts.Add(item: $"{field.Name}: {value.Repr(config: config, visited: visited)}");
        }

        // Get public properties with getters
        var properties = type
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj: obj);
                parts.Add(
                    item: $"{prop.Name}: {value.Repr(config: config, visited: visited)}");
            }
            catch
            {
                parts.Add(item: $"{prop.Name}: <error>");
            }
        }

        var content = parts.Count > 0
            ? String.Join(separator: ", ", values: parts)
            : "";
        return $"{content}";
    }
}

#endregion