using System.Text;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class StandardFormatterTests
{
    // Basic Types
    [Fact]
    public void TestNullRepr()
    {
        Assert.Equal(expected: "null", actual: ((string?)null).Repr());
    }

    [Fact]
    public void TestStringRepr()
    {
        Assert.Equal(expected: "\"hello\"", actual: "hello".Repr());
        Assert.Equal(expected: "\"\"", actual: "".Repr());
    }

    [Fact]
    public void TestCharRepr()
    {
        Assert.Equal(expected: "'A'", actual: 'A'.Repr());
        Assert.Equal(expected: "'\\n'", actual: '\n'.Repr());
        Assert.Equal(expected: "'\\u007F'", actual: '\u007F'.Repr());
        Assert.Equal(expected: "'아'", actual: '아'.Repr());
    }

    [Fact]
    public void TestRuneRepr()
    {
        Assert.Equal(expected: "Rune(💜 @ \\U0001F49C)", actual: new Rune(value: 0x1f49c).Repr());
    }

    [Fact]
    public void TestBoolRepr()
    {
        Assert.Equal(expected: "true", actual: true.Repr());
    }

    [Fact]
    public void TestDateTimeRepr()
    {
        var dateTime = new DateTime(year: 2025, month: 1, day: 1, hour: 0, minute: 0, second: 0);
        var localDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Local);
        var utcDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Utc);
        Assert.Equal(expected: "DateTime(2025.01.01 00:00:00.0000000 Unspecified)", actual:
            dateTime.Repr());
        Assert.Equal(expected: "DateTime(2025.01.01 00:00:00.0000000 Local)", actual:
            localDateTime.Repr());
        Assert.Equal(expected: "DateTime(2025.01.01 00:00:00.0000000 UTC)", actual:
            utcDateTime.Repr());
    }

    [Fact]
    public void TestTimeSpanRepr()
    {
        Assert.Equal(expected: "TimeSpan(1D 00:00:00.0000000)", actual: TimeSpan.FromDays(days: 1)
           .Repr());
    }

    [Fact]
    public void TestTimeSpanRepr_Negative()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "TimeSpan(-00:30:00.0000000)", actual: TimeSpan
           .FromMinutes(minutes: -30)
           .Repr(config: config));
    }

    [Fact]
    public void TestTimeSpanRepr_Negative_WithDays()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "TimeSpan(-1D-01:00:00.0000000)",
            actual: new TimeSpan(days: -1, hours: -1, minutes: 0, seconds: 0)
               .Repr(config: config));
    }

    [Fact]
    public void TestTimeSpanRepr_Zero()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "TimeSpan(00:00:00.0000000)",
            actual: TimeSpan.Zero.Repr(config: config));
    }

    [Fact]
    public void TestTimeSpanRepr_Positive()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "TimeSpan(00:30:00.0000000)", actual: TimeSpan
           .FromMinutes(minutes: 30)
           .Repr(config: config));
    }

    [Fact]
    public void TestDateTimeOffsetRepr()
    {
        Assert.Equal(expected: "DateTimeOffset(2025.01.01 12:34:56.7899870Z)",
            actual: new DateTimeOffset(dateTime: new DateTime(
                date: new DateOnly(year: 2025, month: 1, day: 1),
                time: new TimeOnly(hour: 12, minute: 34, second: 56, millisecond: 789,
                    microsecond: 987),
                kind: DateTimeKind.Utc)).Repr());
    }

    [Fact]
    public void TestDateTimeOffsetRepr_WithOffset()
    {
        Assert.Equal(expected: "DateTimeOffset(2025.01.01 00:00:00.0000000+01:00:00.0000000)",
            actual: new DateTimeOffset(dateTime: new DateTime(year: 2025, month: 1, day: 1),
                offset: TimeSpan.FromHours(hours: 1)).Repr());
    }

    [Fact]
    public void TestDateOnly()
    {
        Assert.Equal(expected: "DateOnly(2025.01.01)",
            actual: new DateOnly(year: 2025, month: 1, day: 1).Repr());
    }

    [Fact]
    public void TestTimeOnly()
    {
        Assert.Equal(expected: "TimeOnly(01:02:03.0000000)",
            actual: new TimeOnly(hour: 1, minute: 2, second: 3).Repr());
    }

    [Fact]
    public void TestGuidRepr()
    {
        var guid = Guid.NewGuid();
        Assert.Equal(expected: $"Guid({guid})", actual: guid.Repr());
    }

    // Memory and Span Types
    [Fact]
    public void TestMemoryRepr()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        var memory = new Memory<int>(array: array, start: 1, length: 3);
        Assert.Equal(expected: "Memory([2_i32, 3_i32, 4_i32])", actual: memory.Repr());

        var emptyMemory = Memory<int>.Empty;
        Assert.Equal(expected: "Memory([])", actual: emptyMemory.Repr());
    }

    [Fact]
    public void TestReadOnlyMemoryRepr()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        var readOnlyMemory = new ReadOnlyMemory<int>(array: array, start: 1, length: 3);
        Assert.Equal(expected: "ReadOnlyMemory([2_i32, 3_i32, 4_i32])",
            actual: readOnlyMemory.Repr());

        var emptyReadOnlyMemory = ReadOnlyMemory<int>.Empty;
        Assert.Equal(expected: "ReadOnlyMemory([])", actual: emptyReadOnlyMemory.Repr());
    }

    [Fact]
    public void TestSpanRepr()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        var span = new Span<int>(array: array, start: 1, length: 3);
        Assert.Equal(expected: "Span([2_i32, 3_i32, 4_i32])", actual: span.Repr());

        var emptySpan = Span<int>.Empty;
        Assert.Equal(expected: "Span([])", actual: emptySpan.Repr());
    }

    [Fact]
    public void TestReadOnlySpanRepr()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        var readOnlySpan = new ReadOnlySpan<int>(array: array, start: 1, length: 3);
        Assert.Equal(expected: "ReadOnlySpan([2_i32, 3_i32, 4_i32])",
            actual: readOnlySpan.Repr());

        var emptyReadOnlySpan = ReadOnlySpan<int>.Empty;
        Assert.Equal(expected: "ReadOnlySpan([])", actual: emptyReadOnlySpan.Repr());
    }

    [Fact]
    public void TestIndexRepr()
    {
        var fromStart = new Index(value: 5);
        Assert.Equal(expected: "Index(5)", actual: fromStart.Repr());

        var fromEnd = new Index(value: 3, fromEnd: true);
        Assert.Equal(expected: "Index(^3)", actual: fromEnd.Repr());

        var startIndex = Index.Start;
        Assert.Equal(expected: "Index(0)", actual: startIndex.Repr());

        var endIndex = Index.End;
        Assert.Equal(expected: "Index(^0)", actual: endIndex.Repr());
    }

    [Fact]
    public void TestRangeRepr()
    {
        var range1 = new Range(start: 1, end: 5);
        Assert.Equal(expected: "Range(1..5)", actual: range1.Repr());

        var range2 = new Range(start: Index.Start, end: new Index(value: 3, fromEnd: true));
        Assert.Equal(expected: "Range(0..^3)", actual: range2.Repr());

        var allRange = Range.All;
        Assert.Equal(expected: "Range(0..^0)", actual: allRange.Repr());

        var rangeFromIndex = 2..^1;
        Assert.Equal(expected: "Range(2..^1)", actual: rangeFromIndex.Repr());
    }

    // Nullable Types
    [Fact]
    public void TestNullableStructRepr()
    {
        Assert.Equal(expected: "123_i32?", actual: ((int?)123).Repr());
        Assert.Equal(expected: "null_i32?", actual: ((int?)null).Repr());
    }

    [Fact]
    public void TestNullableClassRepr()
    {
        Assert.Equal(expected: "null", actual: ((List<int>?)null).Repr());
    }

    [Fact]
    public void TestListWithNullElements()
    {
        var listWithNull = new List<List<int>?> { new() { 1 }, null };
        Assert.Equal(expected: "[[1_i32], null]", actual: listWithNull.Repr());
    }
}