using System.Collections;
using System.Reflection;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr;

internal static class ReprFormatterRegistry
{
    private static readonly Dictionary<Type, IReprFormatter> ReprFormatters = new();

    private static readonly List<(Func<Type, bool>, IReprFormatter)> ConditionalReprFormatters =
        new();

    private static readonly Dictionary<Type, IReprTreeFormatter> ReprTreeFormatters = new();

    private static readonly List<(Func<Type, bool>, IReprTreeFormatter)>
        ConditionalReprTreeFormatters = new();

    static ReprFormatterRegistry()
    {
        DiscoverAttributedFormatters();
        RegisterConditionalFormatters();
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
            if (Activator.CreateInstance(type: type) is IReprFormatter formatter)
            {
                var targetTypes = attr?.TargetTypes ?? Array.Empty<Type>();
                foreach (var targetType in targetTypes)
                {
                    // Only register concrete types, not interfaces/base classes
                    if (targetType is { IsInterface: false, IsAbstract: false })
                    {
                        ReprFormatters[key: targetType] = formatter;
                    }
                }
            }

            if (Activator.CreateInstance(type: type) is IReprTreeFormatter treeFormatter)
            {
                var targetTypes = attr?.TargetTypes ?? Array.Empty<Type>();
                foreach (var targetType in targetTypes)
                {
                    // Only register concrete types, not interfaces/base classes
                    if (targetType is { IsInterface: false, IsAbstract: false })
                    {
                        ReprTreeFormatters[key: targetType] = treeFormatter;
                    }
                }
            }

        }
    }
    private static void RegisterConditionalFormatters()
    {
        // Only register the interface/base class entries that can't use attributes
        ConditionalReprFormatters.AddRange(collection: new List<(Func<Type, bool>, IReprFormatter)>
        {
            (t => t.IsEnum, new EnumFormatter()),
            (t => t.IsRecordType(), new RecordFormatter()),
            (t => t.IsDictionaryType(), new DictionaryFormatter()),
            (t => t.IsTupleType(), new TupleFormatter()),
            (t => t.IsArray, new ArrayFormatter()),
            (t => t.IsSetType(), new SetFormatter()),
            (t => t.IsPriorityQueueType(), new PriorityQueueFormatter()),
            (t => t.IsAssignableTo(targetType: typeof(Delegate)), new FunctionFormatter()),
            (t => t.IsAssignableTo(targetType: typeof(IEnumerable)), new EnumerableFormatter()),
            (t => t.IsAnonymousType(), new ObjectFormatter()),
            (t => typeof(Type).IsAssignableFrom(c: t), new TypeFormatter()),
            (t => t.IsMemoryType(), new MemoryFormatter()),
            (t => t.IsReadOnlyMemoryType(), new ReadOnlyMemoryFormatter())
        });
        ConditionalReprTreeFormatters.AddRange(
            collection: new List<(Func<Type, bool>, IReprTreeFormatter)>
            {
                (t => t.IsEnum, new EnumFormatter()),
                (t => t.IsDictionaryType(), new DictionaryFormatter()),
                (t => t.IsTupleType(), new TupleFormatter()),
                (t => t.IsArray, new ArrayFormatter()),
                (t => t.IsPriorityQueueType(), new PriorityQueueFormatter()),
                (t => t.IsAssignableTo(targetType: typeof(Delegate)), new FunctionFormatter()),
                (t => t.IsAssignableTo(targetType: typeof(IEnumerable)),
                    new EnumerableFormatter()),
                (t => typeof(Type).IsAssignableFrom(c: t), new TypeFormatter()),
                (t => t.IsMemoryType(), new MemoryFormatter()),
                (t => t.IsReadOnlyMemoryType(), new ReadOnlyMemoryFormatter()),
                (t => t.IsAnonymousType(), new ObjectFormatter())
            });
    }
    public static IReprFormatter GetStandardFormatter(Type type, ReprContext context)
    {
        if (ReprFormatters.TryGetValue(key: type, value: out var directFormatter))
        {
            return directFormatter;
        }

        foreach (var (condition, formatter) in ConditionalReprFormatters)
        {
            if (condition(arg: type))
            {
                return formatter;
            }
        }

        return new ObjectFormatter();
    }
    public static IReprTreeFormatter GetTreeFormatter(Type type, ReprContext context)
    {
        if (ReprTreeFormatters.TryGetValue(key: type, value: out var directFormatter))
        {
            return directFormatter;
        }

        foreach (var (condition, formatter) in ConditionalReprTreeFormatters)
        {
            if (condition(arg: type))
            {
                return formatter;
            }
        }

        return new ObjectFormatter();
    }
}