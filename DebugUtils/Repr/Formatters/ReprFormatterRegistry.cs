using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.Formatters.Collections;
using DebugUtils.Repr.Formatters.Numeric;
using DebugUtils.Repr.Formatters.Primitive;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr.Formatters;

public static class ReprFormatterRegistry
{
    private static readonly Dictionary<Type, IReprFormatter> Formatters;
    private static readonly IReprFormatter intFormatter = new IntegerFormatter();
    private static readonly IReprFormatter floatFormatter = new FloatFormatter();
    private static readonly IReprFormatter toStringFormatter = new ToStringFormatter();
    private static readonly IReprFormatter setFormatter = new SetFormatter();

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

        if (type.IsSetType())
        {
            return setFormatter;
        }

        if (typeof(IEnumerable).IsAssignableFrom(c: type))
        {
            return Formatters[key: typeof(IEnumerable)];
        }

        if (type.OverridesToStringType())
        {
            return toStringFormatter;
        }

        return new ReflectionFormatter();
    }

    public static void RegisterFormatter<T>(IReprFormatter formatter)
    {
        Formatters[key: typeof(T)] = formatter;
    }
}