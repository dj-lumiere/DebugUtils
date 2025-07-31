using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Primitive;

[ReprFormatter(typeof(DateTime))]
public class DateTimeFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateTime)obj).ToString(format: "yyyy-MM-dd HH:mm:ss");
    }
}

[ReprFormatter(typeof(DateTimeOffset))]
public class DateTimeOffsetFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateTimeOffset)obj).ToString(format: "yyyy-MM-dd HH:mm:ss");
    }
}

[ReprFormatter(typeof(TimeSpan))]
public class TimeSpanFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((TimeSpan)obj).TotalSeconds:0.000}s";
    }
}

#if NET6_0_OR_GREATER
[ReprFormatter(typeof(DateOnly))]
public class DateOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateOnly)obj).ToString(format: "yyyy-MM-dd");
    }
}

[ReprFormatter(typeof(TimeOnly))]
public class TimeOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((TimeOnly)obj).ToString(format: "HH:mm:ss");
    }
}

#endif