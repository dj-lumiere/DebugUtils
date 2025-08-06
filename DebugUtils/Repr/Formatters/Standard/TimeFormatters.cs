using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(DateTime))]
[ReprOptions(needsPrefix: true)]
internal class DateTimeFormatter : IReprFormatter
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

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var datetime = (DateTime)obj;
        var json = new JsonObject();
        json.Add(propertyName: "type", value: "DateTime");
        json.Add(propertyName: "kind", value: "struct");
        json.Add(propertyName: "year", value: datetime.Year);
        json.Add(propertyName: "month", value: datetime.Month);
        json.Add(propertyName: "day", value: datetime.Day);
        json.Add(propertyName: "hour", value: datetime.Hour);
        json.Add(propertyName: "minute", value: datetime.Minute);
        json.Add(propertyName: "second", value: datetime.Second);
        json.Add(propertyName: "millisecond", value: datetime.Millisecond);
        json.Add(propertyName: "ticks", value: datetime.Ticks);
        json.Add(propertyName: "timezone", value: datetime.Kind.ToString());
        return json;
    }
}

[ReprFormatter(typeof(DateTimeOffset))]
[ReprOptions(needsPrefix: true)]
internal class DateTimeOffsetFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var dto = (DateTimeOffset)obj;
        if (dto.Offset == TimeSpan.Zero)
        {
            return dto.ToString(format: "yyyy-MM-dd HH:mm:ss") + "Z";
        }

        var offset = dto.Offset.ToString(format: "c");
        if (!offset.StartsWith(value: "+"))
        {
            offset = "+" + offset;
        }

        return dto.ToString(format: "yyyy-MM-dd HH:mm:ss") + offset;
    }


    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dto = (DateTimeOffset)obj;
        var json = new JsonObject();
        json.Add(propertyName: "type", value: "DateTimeOffset");
        json.Add(propertyName: "kind", value: "struct");
        json.Add(propertyName: "year", value: dto.Year);
        json.Add(propertyName: "month", value: dto.Month);
        json.Add(propertyName: "day", value: dto.Day);
        json.Add(propertyName: "hour", value: dto.Hour);
        json.Add(propertyName: "minute", value: dto.Minute);
        json.Add(propertyName: "second", value: dto.Second);
        json.Add(propertyName: "millisecond", value: dto.Millisecond);
        json.Add(propertyName: "ticks", value: dto.Ticks);
        json.Add(propertyName: "offset",
            value: dto.Offset.Repr(context: context.WithIncrementedDepth()));
        return json;
    }
}

[ReprFormatter(typeof(TimeSpan))]
[ReprOptions(needsPrefix: true)]
internal class TimeSpanFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((TimeSpan)obj).TotalSeconds:0.000}s";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var ts = (TimeSpan)obj;
        var isNegative = ts.Ticks < 0;
        if (isNegative)
        {
            ts = ts.Negate();
        }

        var json = new JsonObject();
        json.Add(propertyName: "type", value: "TimeSpan");
        json.Add(propertyName: "kind", value: "struct");
        json.Add(propertyName: "day", value: ts.Days);
        json.Add(propertyName: "hour", value: ts.Hours);
        json.Add(propertyName: "minute", value: ts.Minutes);
        json.Add(propertyName: "second", value: ts.Seconds);
        json.Add(propertyName: "millisecond", value: ts.Milliseconds);
        json.Add(propertyName: "ticks", value: ts.Ticks);
        json.Add(propertyName: "isNegative", value: isNegative);
        return json;
    }
}

#if NET6_0_OR_GREATER
[ReprFormatter(typeof(DateOnly))]
[ReprOptions(needsPrefix: true)]
internal class DateOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((DateOnly)obj).ToString(format: "yyyy-MM-dd");
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dateOnly = (DateOnly)obj;
        var json = new JsonObject();
        json.Add(propertyName: "type", value: "DateTimeOffset");
        json.Add(propertyName: "kind", value: "struct");
        json.Add(propertyName: "year", value: dateOnly.Year);
        json.Add(propertyName: "month", value: dateOnly.Month);
        json.Add(propertyName: "day", value: dateOnly.Day);
        return json;
    }
}

[ReprFormatter(typeof(TimeOnly))]
[ReprOptions(needsPrefix: true)]
internal class TimeOnlyFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return ((TimeOnly)obj).ToString(format: "HH:mm:ss");
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var to = (TimeOnly)obj;
        var json = new JsonObject();
        json.Add(propertyName: "type", value: "DateTimeOffset");
        json.Add(propertyName: "kind", value: "struct");
        json.Add(propertyName: "hour", value: to.Hour);
        json.Add(propertyName: "minute", value: to.Minute);
        json.Add(propertyName: "second", value: to.Second);
        json.Add(propertyName: "millisecond", value: to.Millisecond);
        json.Add(propertyName: "ticks", value: to.Ticks);
        return json;
    }
}

#endif