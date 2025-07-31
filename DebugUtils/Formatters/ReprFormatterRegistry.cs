using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
            [typeof(string)] = new StringFormatter(),
            [typeof(char)] = new CharFormatter(),
            [typeof(bool)] = new BoolFormatter(),
            [typeof(decimal)] = new DecimalFormatter(),
            [typeof(Array)] = new ArrayFormatter(),
            [typeof(IDictionary)] = new DictionaryFormatter(),
            [typeof(IEnumerable)] = new EnumerableFormatter(),
            [typeof(ITuple)] = new TupleFormatter(),
            [typeof(Rune)] = new RuneFormatter(),
            [typeof(IntPtr)] = new IntPtrFormatter(),
            [typeof(UIntPtr)] = new UIntPtrFormatter(),
            [typeof(DateTime)] = new DateTimeFormatter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetFormatter(),
            [typeof(TimeSpan)] = new TimeSpanFormatter(),
            [typeof(Enum)] = new EnumFormatter(),
            [typeof(BigInteger)] = intFormatter,
            [typeof(int)] = intFormatter, [typeof(uint)] = intFormatter,
            [typeof(long)] = intFormatter, [typeof(ulong)] = intFormatter,
            [typeof(short)] = intFormatter, [typeof(ushort)] = intFormatter,
            [typeof(sbyte)] = intFormatter, [typeof(byte)] = intFormatter,
            [typeof(double)] = floatFormatter, [typeof(float)] = floatFormatter,
#if NET5_0_OR_GREATER
            [typeof(Half)] = floatFormatter,
#endif
#if NET6_0_OR_GREATER
            [typeof(DateOnly)] = new DateOnlyFormatter(),
            [typeof(TimeOnly)] = new TimeOnlyFormatter(),
#endif
#if NET7_0_OR_GREATER
            [typeof(Int128)] = intFormatter, [typeof(UInt128)] = intFormatter,
#endif
        };
    }

    public static IReprFormatter GetFormatter(Type type)
    {
        if (Formatters.TryGetValue(type, out var formatter)) return formatter;
        if (type.IsEnum) return Formatters[typeof(Enum)];
        if (type.IsRecordType()) return new RecordFormatter();
        if (type.IsDictionaryType()) return Formatters[typeof(IDictionary)];
        if (type.IsTupleType()) return Formatters[typeof(ITuple)];
        if (type.IsArray) return Formatters[typeof(Array)];
        if (typeof(IEnumerable).IsAssignableFrom(type)) return Formatters[typeof(IEnumerable)];
        if (type.OverridesToStringType()) return degenerateFormatter;
        return new DefaultObjectFormatter();
    }
}

// --- All Formatters now implement the same, single interface ---

public class DegenerateFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => o.ToString() ?? "";
}

public class StringFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => $"\"{(string)o}\"";
}

public class CharFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        CharFormatterLogic.FormatChar((char)o);
}

public class BoolFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => (bool)o
        ? "true"
        : "false";
}

public class RuneFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        $"{(Rune)o} @ \\U{((Rune)o).Value:X8}";
}

public class IntPtrFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => IntPtr.Size == 4
        ? $"0x{((IntPtr)o).ToInt32():X8}"
        : $"0x{((IntPtr)o).ToInt64():X16}";
}

public class UIntPtrFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => IntPtr.Size == 4
        ? $"0x{((UIntPtr)o).ToUInt32():X8}"
        : $"0x{((UIntPtr)o).ToUInt64():X16}";
}

public class DateTimeFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss");
}

public class DateTimeOffsetFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        ((DateTimeOffset)o).ToString("yyyy-MM-dd HH:mm:ss");
}

public class TimeSpanFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        $"{((TimeSpan)o).TotalSeconds:0.000}s";
}

#if NET6_0_OR_GREATER
public class DateOnlyFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        ((DateOnly)o).ToString("yyyy-MM-dd");
}

public class TimeOnlyFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) =>
        ((TimeOnly)o).ToString("HH:mm:ss");
}

#endif

public class EnumFormatter : IReprFormatter
{
    public string ToRepr(object o, ReprConfig c, HashSet<int>? v) => $"{o.GetReprTypeName()}.{o}";
}

// The DefaultObjectFormatter handles any type not specifically registered.
public class DefaultObjectFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        var parts = new List<string>();

        // Get public fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            parts.Add($"{field.Name}: {value.Repr(config, visited)}");
        }

        // Get public properties with getters
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj);
                parts.Add($"{prop.Name}: {value.Repr(config, visited)}");
            }
            catch
            {
                parts.Add($"{prop.Name}: <error>");
            }
        }

        var content = parts.Count > 0
            ? string.Join(", ", parts)
            : "";
        return $"{content}";
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

        if (char.IsControl(value))
        {
            return $"'\\u{(int)value:X4}'";
        }

        return $"'{value}'";
    }
}