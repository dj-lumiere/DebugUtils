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
        var dt = (DateTime)obj;
        var kindSuffix = dt.Kind switch
        {
            DateTimeKind.Utc => "UTC",
            DateTimeKind.Local => "Local",
            _ => "Unspecified"
        };
        return
            $"{dt.Year:D4}.{dt.Month:D2}.{dt.Day:D2} {dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}.{dt.Millisecond:D3}{dt.Ticks % 10000:D4} {kindSuffix}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var datetime = (DateTime)obj;
        var result = new JsonObject
        {
            { "type", "DateTime" },
            { "kind", "struct" },
            { "year", datetime.Year.ToString() },
            { "month", datetime.Month.ToString() },
            { "day", datetime.Day.ToString() },
            { "hour", datetime.Hour.ToString() },
            { "minute", datetime.Minute.ToString() },
            { "second", datetime.Second.ToString() },
            { "millisecond", datetime.Millisecond.ToString() },
            { "subTicks", (datetime.Ticks % 10000).ToString() },
            { "totalTicks", datetime.Ticks.ToString() },
            { "timezone", datetime.Kind.ToString() }
        };
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
        var dtoTime =
            $"{dto.Year:D4}.{dto.Month:D2}.{dto.Day:D2} {dto.Hour:D2}:{dto.Minute:D2}:{dto.Second:D2}.{dto.Millisecond:D3}{dto.Ticks % 10000:D4}";
        if (dto.Offset == TimeSpan.Zero)
        {
            return dtoTime + "Z";
        }

        var ts = dto.Offset;
        var isNegative = ts.Ticks < 0;
        if (isNegative)
        {
            ts = ts.Negate();
        }

        var subDayPart =
            $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}{ts.Ticks % 10000:D4}";
        var tsString = (isNegative, ts.Days) switch
        {
            (true, 0) => $"-{subDayPart}",
            (true, _) => $"-{ts.Days}D-{subDayPart}",
            (false, 0) => $"+{subDayPart}",
            (false, _) => $"+{ts.Days}D+{subDayPart}"
        };
        return
            $"{dtoTime}{tsString}";
    }


    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dto = (DateTimeOffset)obj;
        var result = new JsonObject
        {
            { "type", "DateTimeOffset" },
            { "kind", "struct" },
            { "year", dto.Year.ToString() },
            { "month", dto.Month.ToString() },
            { "day", dto.Day.ToString() },
            { "hour", dto.Hour.ToString() },
            { "minute", dto.Minute.ToString() },
            { "second", dto.Second.ToString() },
            { "millisecond", dto.Millisecond.ToString() },
            { "subTicks", (dto.Ticks % 10000).ToString() },
            { "totalTicks", dto.Ticks.ToString() },
            { "offset", dto.Offset.FormatAsJsonNode(context: context.WithIncrementedDepth()) }
        };
        return result;
    }
}

[ReprFormatter(typeof(TimeSpan))]
[ReprOptions(needsPrefix: true)]
internal class TimeSpanFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var ts = (TimeSpan)obj;
        var isNegative = ts.Ticks < 0;
        if (isNegative)
        {
            ts = ts.Negate();
        }

        var subDayPart =
            $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}{ts.Ticks % 10000:D4}";
        return (isNegative, ts.Days) switch
        {
            (true, 0) => $"-{subDayPart}",
            (true, _) => $"-{ts.Days}D-{subDayPart}",
            (false, 0) => subDayPart,
            (false, _) => $"{ts.Days}D {subDayPart}"
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var ts = (TimeSpan)obj;
        var isNegative = ts.Ticks < 0;
        if (isNegative)
        {
            ts = ts.Negate();
        }

        var result = new JsonObject
        {
            { "type", "TimeSpan" },
            { "kind", "struct" },
            { "day", ts.Days.ToString() },
            { "hour", ts.Hours.ToString() },
            { "minute", ts.Minutes.ToString() },
            { "second", ts.Seconds.ToString() },
            { "millisecond", ts.Milliseconds.ToString() },
            { "subTicks", (ts.Ticks % 10000).ToString() },
            { "totalTicks", ts.Ticks.ToString() },
            {
                "isNegative", isNegative.ToString()
                                        .ToLowerInvariant()
            }
        };
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
        var dateOnly = (DateOnly)obj;
        return $"{dateOnly.Year:D4}.{dateOnly.Month:D2}.{dateOnly.Day:D2}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dateOnly = (DateOnly)obj;
        var result = new JsonObject
        {
            { "type", "DateOnly" },
            { "kind", "struct" },
            { "year", dateOnly.Year.ToString() },
            { "month", dateOnly.Month.ToString() },
            { "day", dateOnly.Day.ToString() }
        };
        return result;
    }
}

[ReprFormatter(typeof(TimeOnly))]
[ReprOptions(needsPrefix: true)]
internal class TimeOnlyFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var to = (TimeOnly)obj;
        return
            $"{to.Hour:D2}:{to.Minute:D2}:{to.Second:D2}.{to.Millisecond:D3}{to.Ticks % 10000:D4}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var to = (TimeOnly)obj;
        var result = new JsonObject
        {
            { "type", "TimeOnly" },
            { "kind", "struct" },
            { "hour", to.Hour.ToString() },
            { "minute", to.Minute.ToString() },
            { "second", to.Second.ToString() },
            { "millisecond", to.Millisecond.ToString() },
            { "subTicks", (to.Ticks % 10000).ToString() },
            { "totalTicks", to.Ticks.ToString() }
        };
        return result;
    }
}

#endif