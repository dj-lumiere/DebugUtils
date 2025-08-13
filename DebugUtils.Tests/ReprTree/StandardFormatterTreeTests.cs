using System.Text;
using System.Text.Json.Nodes;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class StandardFormatterTreeTests
{
    [Fact]
    public void TestNullRepr()
    {
        var actualJson = JsonNode.Parse(json: ((string?)null).ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "length"]);
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Null(@object: actualJson[propertyName: "value"]);
    }

    [Fact]
    public void TestStringRepr()
    {
        var actualJson = JsonNode.Parse(json: "hello".ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: actualJson[propertyName: "length"]!.GetValue<int>());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "hello", actual: actualJson[propertyName: "value"]
          ?.ToString());

        actualJson = JsonNode.Parse(json: "".ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 0, actual: actualJson[propertyName: "length"]!.GetValue<int>());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "", actual: actualJson[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestCharRepr()
    {
        var actualJson = JsonNode.Parse(json: 'A'.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "char", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "A", actual: actualJson[propertyName: "value"]
          ?.ToString());
        Assert.Equal(expected: "0x0041", actual: actualJson[propertyName: "unicodeValue"]
          ?.ToString());

        actualJson = JsonNode.Parse(json: '\n'.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "char", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "\\n", actual: actualJson[propertyName: "value"]
          ?.ToString());
        Assert.Equal(expected: "0x000A", actual: actualJson[propertyName: "unicodeValue"]
          ?.ToString());

        actualJson = JsonNode.Parse(json: '\u007F'.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "char", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "\\u007F", actual: actualJson[propertyName: "value"]
          ?.ToString());
        Assert.Equal(expected: "0x007F", actual: actualJson[propertyName: "unicodeValue"]
          ?.ToString());

        actualJson = JsonNode.Parse(json: '아'.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "char", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "아", actual: actualJson[propertyName: "value"]
          ?.ToString());
        Assert.Equal(expected: "0xC544", actual: actualJson[propertyName: "unicodeValue"]
          ?.ToString());
    }

    [Fact]
    public void TestRuneRepr()
    {
        var rune = new Rune(value: 0x1f49c);
        var actualJson = JsonNode.Parse(json: rune.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "Rune", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "💜", actual: actualJson[propertyName: "value"]
          ?.ToString());
        Assert.Equal(expected: "0x0001F49C", actual: actualJson[propertyName: "unicodeValue"]
          ?.ToString());
    }

    [Fact]
    public void TestBoolRepr()
    {
        var actualJson = JsonNode.Parse(json: true.ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "bool", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "true", actual: actualJson[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestDateTimeRepr()
    {
        var dateTime = new DateTime(year: 2025, month: 1, day: 1, hour: 0, minute: 0, second: 0);
        var localDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Local);
        var utcDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Utc);

        var actualJson = JsonNode.Parse(json: dateTime.ReprTree()) ?? new JsonObject();
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTime",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "timezone"] = "Unspecified"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: localDateTime.ReprTree()) ?? new JsonObject();
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTime",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "timezone"] = "Local"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: utcDateTime.ReprTree()) ?? new JsonObject();
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTime",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "timezone"] = "Utc"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr()
    {
        var timeSpan = TimeSpan.FromMinutes(minutes: 30);
        var actualJson = JsonNode.Parse(json: timeSpan.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "TimeSpan",
            [propertyName: "kind"] = "struct",
            [propertyName: "day"] = "0",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "30",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "18000000000",
            [propertyName: "isNegative"] = "false"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestGuidRepr()
    {
        var guid = Guid.NewGuid();
        var actualJson = JsonNode.Parse(json: guid.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Guid",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = guid.ToString()
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Negative()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.FromMinutes(minutes: -30);
        var actualJson = JsonNode.Parse(json: timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "TimeSpan",
            [propertyName: "kind"] = "struct",
            [propertyName: "day"] = "0",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "30",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "18000000000",
            [propertyName: "isNegative"] = "true"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Zero()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.Zero;
        var actualJson = JsonNode.Parse(json: timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "TimeSpan",
            [propertyName: "kind"] = "struct",
            [propertyName: "day"] = "0",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "0",
            [propertyName: "isNegative"] = "false"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Positive()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.FromMinutes(minutes: 30);
        var actualJson = JsonNode.Parse(json: timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "TimeSpan",
            [propertyName: "kind"] = "struct",
            [propertyName: "day"] = "0",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "30",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "18000000000",
            [propertyName: "isNegative"] = "false"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDateTimeOffsetRepr()
    {
        var dateTimeOffset = new DateTimeOffset(dateTime: new DateTime(
            date: new DateOnly(year: 2025, month: 1, day: 1),
            time: new TimeOnly(hour: 0, minute: 0, second: 0), kind: DateTimeKind.Utc));
        var actualJson = JsonNode.Parse(json: dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTimeOffset",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "0",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "subTicks"] = "0",
                [propertyName: "totalTicks"] = "0",
                [propertyName: "isNegative"] = "false"
            }
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDateTimeOffsetRepr_WithOffset()
    {
        var dateTimeOffset =
            new DateTimeOffset(dateTime: new DateTime(year: 2025, month: 1, day: 1),
                offset: TimeSpan.FromHours(hours: 1));
        var actualJson = JsonNode.Parse(json: dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTimeOffset",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "1",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "subTicks"] = "0",
                [propertyName: "totalTicks"] = "36000000000",
                [propertyName: "isNegative"] = "false"
            }
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDateTimeOffsetRepr_WithNegativeOffset()
    {
        var dateTimeOffset =
            new DateTimeOffset(dateTime: new DateTime(year: 2025, month: 1, day: 1),
                offset: TimeSpan.FromHours(hours: -1));
        var actualJson = JsonNode.Parse(json: dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateTimeOffset",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "1",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "subTicks"] = "0",
                [propertyName: "totalTicks"] = "36000000000",
                [propertyName: "isNegative"] = "true"
            }
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDateOnly()
    {
        var dateOnly = new DateOnly(year: 2025, month: 1, day: 1);
        var actualJson = JsonNode.Parse(json: dateOnly.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "DateOnly",
            [propertyName: "kind"] = "struct",
            [propertyName: "year"] = "2025",
            [propertyName: "month"] = "1",
            [propertyName: "day"] = "1"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTimeOnly()
    {
        var timeOnly = new TimeOnly(hour: 0, minute: 0, second: 0);
        var actualJson = JsonNode.Parse(json: timeOnly.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "TimeOnly",
            [propertyName: "kind"] = "struct",
            [propertyName: "hour"] = "0",
            [propertyName: "minute"] = "0",
            [propertyName: "second"] = "0",
            [propertyName: "millisecond"] = "0",
            [propertyName: "subTicks"] = "0",
            [propertyName: "totalTicks"] = "0"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestNullableStructRepr()
    {
        var actualJson = JsonNode.Parse(json: ((int?)123).ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "int?",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "123"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: ((int?)null).ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "int?",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = null
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestNullableClassRepr()
    {
        var actualJson = JsonNode.Parse(json: ((List<int>?)null).ReprTree()) ?? new JsonObject();
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "value"]);
    }
}