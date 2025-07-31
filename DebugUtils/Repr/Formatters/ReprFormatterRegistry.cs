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
        if (config.FormattingMode == FormattingMode.ReflectionJson)
        {
            return ObjectJsonFormatter.Value;
        }

        if (config.FormattingMode == FormattingMode.Reflection)
        {
            return ObjectFormatter.Value;
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
            return RecordFormatter.Value;
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
            return SetFormatter;
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
            return ToStringFormatter;
        }

        return ObjectFormatter.Value;
    }
}