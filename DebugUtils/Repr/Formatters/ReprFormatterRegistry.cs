using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using DebugUtils.Repr.Formatters.Collections;
using DebugUtils.Repr.Formatters.Functions;
using DebugUtils.Repr.Formatters.Primitive;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr.Formatters;

public static class ReprFormatterRegistry
{
    private static readonly Dictionary<Type, IReprFormatter> Formatters = new();

    private static readonly Lazy<EnumFormatter> enumFormatter =
        new(valueFactory: () => new EnumFormatter());

    private static readonly Lazy<RecordFormatter> recordFormatter =
        new(valueFactory: () => new RecordFormatter());

    private static readonly Lazy<ReflectionFormatter> reflectionFormatter =
        new(valueFactory: () => new ReflectionFormatter());

    private static readonly Lazy<ReflectionJsonFormatter> reflectionJsonFormatter =
        new(valueFactory: () => new ReflectionJsonFormatter());

    private static readonly IReprFormatter toStringFormatter = new ToStringFormatter();
    private static readonly IReprFormatter setFormatter = new SetFormatter();

    static ReprFormatterRegistry()
    {
        DiscoverAttributedFormatters();
        RegisterFallbackFormatters();
    }

    private static void DiscoverAttributedFormatters()
    {
        // Only register exact type matches from attributes
        var formatterTypes = typeof(ReprFormatterRegistry).Assembly
                                                          .GetTypes()
                                                          .Where(predicate: t =>
                                                               t.GetCustomAttribute<
                                                                   ReprFormatterAttribute>() !=
                                                               null);

        foreach (var type in formatterTypes)
        {
            var attr = type.GetCustomAttribute<ReprFormatterAttribute>();
            var formatter = Activator.CreateInstance(type: type) as IReprFormatter;

            foreach (var targetType in attr.TargetTypes)
            {
                // Only register concrete types, not interfaces/base classes
                if (!targetType.IsInterface && !targetType.IsAbstract)
                {
                    Formatters[key: targetType] = formatter;
                }
            }
        }
    }
    private static void RegisterFallbackFormatters()
    {
        // Only register the interface/base class entries that can't use attributes
        Formatters[key: typeof(Array)] = new ArrayFormatter();
        Formatters[key: typeof(IDictionary)] = new DictionaryFormatter();
        Formatters[key: typeof(IEnumerable)] = new EnumerableFormatter();
        Formatters[key: typeof(ITuple)] = new TupleFormatter();
        Formatters[key: typeof(Enum)] = enumFormatter.Value;
        Formatters[key: typeof(Delegate)] = new FunctionFormatter();
    }
    public static IReprFormatter GetFormatter(Type type, ReprConfig config)
    {
        if (config.FormattingMode == FormattingMode.ReflectionJson)
        {
            return reflectionJsonFormatter.Value;
        }

        if (config.FormattingMode == FormattingMode.Reflection)
        {
            return reflectionFormatter.Value;
        }

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
            return recordFormatter.Value;
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

        if (type.IsAssignableTo(targetType: typeof(Delegate)))
        {
            return Formatters[key: typeof(Delegate)];
        }

        if (type.IsAssignableTo(targetType: typeof(IEnumerable)))
        {
            return Formatters[key: typeof(IEnumerable)];
        }

        if (type.OverridesToStringType())
        {
            return toStringFormatter;
        }

        return reflectionFormatter.Value;
    }
}