using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(DateTime))]
[ReprOptions(needsPrefix: true)]
public class DateTimeFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var datetime = (DateTime)obj;
        var kindSuffix = datetime.Kind switch
        {
            DateTimeKind.Utc => " UTC",
            DateTimeKind.Local => " Local",
            _ => " Unspecified"
        };
        return datetime.ToString(format: "yyyy-MM-dd HH:mm:ss") + kindSuffix;
    }
}

[ReprFormatter(typeof(DateTimeOffset))]
[ReprOptions(needsPrefix: true)]
public class DateTimeOffsetFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var dto = (DateTimeOffset)obj;
        if (dto.Offset == TimeSpan.Zero)
        {
            return dto.ToString("yyyy-MM-dd HH:mm:ss") + "Z";
        }

        var offset = dto.Offset.ToString(format: "c");
        if (!offset.StartsWith(value: "+"))
        {
            offset = "+" + offset;
        }

        return dto.ToString("yyyy-MM-dd HH:mm:ss") + offset;
    }
}

[ReprFormatter(typeof(TimeSpan))]
[ReprOptions(needsPrefix: true)]
public class TimeSpanFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((TimeSpan)obj).TotalSeconds:0.000}s";
    }
}

#if NET6_0_OR_GREATER
[ReprFormatter(typeof(DateOnly))]
[ReprOptions(needsPrefix: true)]
public class DateOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateOnly)obj).ToString(format: "yyyy-MM-dd");
    }
}

[ReprFormatter(typeof(TimeOnly))]
[ReprOptions(needsPrefix: true)]
public class TimeOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((TimeOnly)obj).ToString(format: "HH:mm:ss");
    }
}

#endif