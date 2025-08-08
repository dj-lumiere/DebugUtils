using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(DateTime))]
[ReprOptions(needsPrefix: true)]
internal class DateTimeFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
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
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "DateTime");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "year", value: datetime.Year.ToString());
        result.Add(propertyName: "month", value: datetime.Month.ToString());
        result.Add(propertyName: "day", value: datetime.Day.ToString());
        result.Add(propertyName: "hour", value: datetime.Hour.ToString());
        result.Add(propertyName: "minute", value: datetime.Minute.ToString());
        result.Add(propertyName: "second", value: datetime.Second.ToString());
        result.Add(propertyName: "millisecond", value: datetime.Millisecond.ToString());
        result.Add(propertyName: "ticks", value: datetime.Ticks.ToString());
        result.Add(propertyName: "timezone", value: datetime.Kind.ToString());
        return result;
    }
}

[ReprFormatter(typeof(DateTimeOffset))]
[ReprOptions(needsPrefix: true)]
internal class DateTimeOffsetFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
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
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "DateTimeOffset");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "year", value: dto.Year.ToString());
        result.Add(propertyName: "month", value: dto.Month.ToString());
        result.Add(propertyName: "day", value: dto.Day.ToString());
        result.Add(propertyName: "hour", value: dto.Hour.ToString());
        result.Add(propertyName: "minute", value: dto.Minute.ToString());
        result.Add(propertyName: "second", value: dto.Second.ToString());
        result.Add(propertyName: "millisecond", value: dto.Millisecond.ToString());
        result.Add(propertyName: "ticks", value: dto.Ticks.ToString());
        result.Add(propertyName: "offset",
            value: dto.Offset.FormatAsJsonNode(context: context.WithIncrementedDepth()));
        return result;
    }
}

[ReprFormatter(typeof(TimeSpan))]
[ReprOptions(needsPrefix: true)]
internal class TimeSpanFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
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

        var result = new JsonObject();
        result.Add(propertyName: "type", value: "TimeSpan");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "day", value: ts.Days.ToString());
        result.Add(propertyName: "hour", value: ts.Hours.ToString());
        result.Add(propertyName: "minute", value: ts.Minutes.ToString());
        result.Add(propertyName: "second", value: ts.Seconds.ToString());
        result.Add(propertyName: "millisecond", value: ts.Milliseconds.ToString());
        result.Add(propertyName: "ticks", value: ts.Ticks.ToString());
        result.Add(propertyName: "isNegative", value: isNegative.ToString()
                                                                .ToLowerInvariant());
        return result;
    }
}

#if NET6_0_OR_GREATER
[ReprFormatter(typeof(DateOnly))]
[ReprOptions(needsPrefix: true)]
internal class DateOnlyFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return ((DateOnly)obj).ToString(format: "yyyy-MM-dd");
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dateOnly = (DateOnly)obj;
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "DateOnly");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "year", value: dateOnly.Year.ToString());
        result.Add(propertyName: "month", value: dateOnly.Month.ToString());
        result.Add(propertyName: "day", value: dateOnly.Day.ToString());
        return result;
    }
}

[ReprFormatter(typeof(TimeOnly))]
[ReprOptions(needsPrefix: true)]
internal class TimeOnlyFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return ((TimeOnly)obj).ToString(format: "HH:mm:ss");
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var to = (TimeOnly)obj;
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "TimeOnly");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "hour", value: to.Hour.ToString());
        result.Add(propertyName: "minute", value: to.Minute.ToString());
        result.Add(propertyName: "second", value: to.Second.ToString());
        result.Add(propertyName: "millisecond", value: to.Millisecond.ToString());
        result.Add(propertyName: "ticks", value: to.Ticks.ToString());
        return result;
    }
}

#endif