using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using DebugUtils.Repr.Formatters.Collections;
using DebugUtils.Repr.Formatters.Fallback;
using DebugUtils.Repr.Formatters.Functions;
using DebugUtils.Repr.Formatters.Standard;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr.Formatters;

public static class ReprFormatterRegistry
{
    private static readonly Dictionary<Type, IReprFormatter> Formatters = new();

    private static readonly Lazy<EnumFormatter> EnumFormatter =
        new(valueFactory: () => new EnumFormatter());

    private static readonly Lazy<RecordFormatter> RecordFormatter =
        new(valueFactory: () => new RecordFormatter());

    private static readonly Lazy<ObjectFormatter> ObjectFormatter =
        new(valueFactory: () => new ObjectFormatter());

    private static readonly Lazy<ObjectJsonFormatter> ObjectJsonFormatter =
        new(valueFactory: () => new ObjectJsonFormatter());

    private static readonly IReprFormatter ToStringFormatter = new ToStringFormatter();
    private static readonly IReprFormatter SetFormatter = new SetFormatter();

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
        Formatters[key: typeof(Enum)] = EnumFormatter.Value;
        Formatters[key: typeof(Delegate)] = new FunctionFormatter();
    }
    public static IReprFormatter GetFormatter(Type type, ReprConfig config)
    {
        var formatter = (config.FormattingMode, type) switch
        {
            (FormattingMode.Json, _) => ObjectJsonFormatter.Value,
            (FormattingMode.Reflection, _) => ObjectFormatter.Value,
            (_, { } t) when Formatters.TryGetValue(key: t, value: out var result) => result,
            (_, { } t) when t.IsEnum => Formatters[key: typeof(Enum)],
            (_, { } t) when t.IsRecordType() => RecordFormatter.Value,
            (_, { } t) when t.IsDictionaryType() => Formatters[key: typeof(IDictionary)],
            (_, { } t) when t.IsTupleType() => Formatters[key: typeof(ITuple)],
            (_, { } t) when t.IsArray => Formatters[key: typeof(Array)],
            (_, { } t) when t.IsSetType() => SetFormatter,
            (_, { } t) when t.IsAssignableTo(targetType: typeof(Delegate)) => Formatters[
                key: typeof(Delegate)],
            (_, { } t) when t.IsAssignableTo(targetType: typeof(IEnumerable)) => Formatters[
                key: typeof(IEnumerable)],
            (_, { } t) when t.IsAnonymousType() => RecordFormatter.Value,
            (_, { } t) when t.OverridesToStringType() => ToStringFormatter,
            (_, _) => ObjectFormatter.Value
        };

        return formatter;
    }
}