using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

public enum FloatReprMode
{
    Exact,
    Scientific,
    Round,
    General,
    RawBytesHex,
    BitField
}

public struct FloatReprConfig
{
    public FloatReprMode mode { get; } = FloatReprMode.Exact;
    public int precision { get; } = 10;
    public bool forceFloatReprModeInContainer { get; } = true;

    public FloatReprConfig(FloatReprMode mode, int precision = 10,
        bool forceFloatReprModeInContainer = true)
    {
        this.mode = mode;
        this.precision = precision;
        this.forceFloatReprModeInContainer = forceFloatReprModeInContainer;
    }
}

public enum IntReprMode
{
    Hex,
    Binary,
    Decimal,
    rawByteHex
}

public struct IntReprConfig
{
    public IntReprMode mode { get; } = IntReprMode.Decimal;
    public bool forceIntReprModeInContainer { get; } = true;
    public IntReprConfig(IntReprMode mode, bool forceIntReprModeInContainer = true)
    {
        this.mode = mode;
        this.forceIntReprModeInContainer = forceIntReprModeInContainer;
    }
}

public static partial class DebugUtils
{
    public static readonly FloatReprConfig DefaultContainderFloatConfig =
        new(FloatReprMode.General, 2, true);

    public static readonly IntReprConfig
        DefaultContainerIntConfig = new(IntReprMode.Decimal, true);

    public static readonly FloatReprConfig DefaultFloatConfig =
        new(FloatReprMode.Exact, -1, false);

    public static readonly IntReprConfig DefaultIntConfig = new(IntReprMode.Decimal, false);

    public static string Repr<T>(this T t, FloatReprConfig? floatReprConfig = null,
        IntReprConfig? intReprConfig = null)
    {
        var notnullFloatReprConfig = floatReprConfig ?? DefaultFloatConfig;
        var notnullIntReprConfig = intReprConfig ?? DefaultIntConfig;
        return t switch
        {
            null => "null",
            string s => $"\"{s}\"",
            char c => $"{c.ReprChar()}",
            bool b => b
                ? "true"
                : "false",
            byte bt => $"Byte({bt.ReprByte(notnullIntReprConfig)})",
            sbyte sb => $"SByte({sb.ReprSByte(notnullIntReprConfig)})",
            short s => $"Short({s.ReprShort(notnullIntReprConfig)})",
            ushort us => $"UShort({us.ReprUShort(notnullIntReprConfig)})",
            int i => $"Int({i.ReprInt(notnullIntReprConfig)})",
            uint ui => $"UInt({ui.ReprUInt(notnullIntReprConfig)})",
            long l => $"Long({l.ReprLong(notnullIntReprConfig)})",
            ulong ul => $"ULong({ul.ReprULong(notnullIntReprConfig)})",
            BigInteger bi => $"BigInteger({bi.ReprBigInteger(notnullIntReprConfig)})",
            IntPtr ip => IntPtr.Size == 4
                ? $"IntPtr(0x{ip.ToInt64():X8})"
                : $"IntPtr(0x{ip.ToInt64():X16})",
            UIntPtr uip => IntPtr.Size == 4
                ? $"UIntPtr(0x{uip.ToUInt64():X8})"
                : $"UIntPtr(0x{uip.ToUInt64():X16})",
            double d => d.ReprDouble(notnullFloatReprConfig),
            float f => f.ReprFloat(notnullFloatReprConfig),
            Half h => h.ReprHalf(notnullFloatReprConfig),
            decimal dec => dec.ReprDecimal(notnullFloatReprConfig),
            DateTime dt => $"DateTime({dt:yyyy-MM-dd HH:mm:ss})",
            TimeSpan ts => $"TimeSpan({ts})",
            Array arr => arr.ReprArray(notnullFloatReprConfig, notnullIntReprConfig),
            IList list => list.ReprIList(notnullFloatReprConfig, notnullIntReprConfig),
            IDictionary dict => dict.ReprIDictionary(notnullFloatReprConfig, notnullIntReprConfig),
            _ when IsSetType(t) => ((IEnumerable)t).ReprISet(notnullFloatReprConfig,
                notnullIntReprConfig),
            _ when IsQueueType(t) => ((IEnumerable)t).ReprQueue(notnullFloatReprConfig,
                notnullIntReprConfig),
            _ when IsStackType(t) => ((IEnumerable)t).ReprStack(notnullFloatReprConfig,
                notnullIntReprConfig),
            IEnumerable list when t is not string =>
                $"{t.GetType().Name}({list.ReprIEnumerable(notnullFloatReprConfig, notnullIntReprConfig)})",
            ITuple tuple => tuple.ReprTuple(notnullFloatReprConfig, notnullIntReprConfig),
            _ when IsEnum(t.GetType()) => $"{t.GetType().Name}.{t.ToString()}",
            // Handle value types
            _ when t.GetType()
                .IsValueType && OverridedToString(t) => t.ToString()!,
            _ when t.GetType()
                .IsValueType => ReprWithFields(t, t.GetType(), notnullFloatReprConfig,
                notnullIntReprConfig),
            // Handle reference types  
            _ when IsRecord(t.GetType()) => ReprWithFields(t, t.GetType(), notnullFloatReprConfig,
                notnullIntReprConfig),
            _ when OverridedToString(t) => t.ToString()!,
            _ => ReprWithFields(t, t.GetType(), notnullFloatReprConfig, notnullIntReprConfig)
        };
    }

    private static string ReprChar(this char c)
    {
        switch (c)
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

        if (char.IsControl(c))
        {
            return $"'\\u{(int)c:X4}'";
        }

        return $"'{c}'";
    }
}