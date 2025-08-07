using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using DebugUtils.Repr;
using DebugUtils.Repr.Records;

namespace DebugUtils.Tests;

public class Student
{
    public required string Name { get; set; }
    public int Age { get; set; }
    public required List<string> Hobbies { get; set; }
}

public class ReprTreeTest
{
    [Fact]
    public void TestReadme()
    {
        var student = new Student
        {
            Name = "Alice",
            Age = 30,
            Hobbies = new List<string> { "reading", "coding" }
        };
        var actualJson = JsonNode.Parse(json: student.ReprTree()) ?? new JsonObject();

        Assert.Equal(expected: "Student", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!.AsObject();
        Assert.Equal(expected: "int", actual: ageNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "30", actual: ageNode[propertyName: "value"]
          ?.ToString());

        var hobbiesNode = actualJson[propertyName: "Hobbies"]!.AsObject();
        Assert.Equal(expected: "List", actual: hobbiesNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: hobbiesNode[propertyName: "count"]!.GetValue<int>());

        var hobbiesValue = hobbiesNode[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: "reading",
            actual: hobbiesValue[index: 0]![propertyName: "value"]!.GetValue<string>());
        Assert.Equal(expected: "coding",
            actual: hobbiesValue[index: 1]![propertyName: "value"]!.GetValue<string>());
    }

    [Fact]
    public void TestExample()
    {
        var person = new Person(name: "John", age: 30);
        var actualJson = JsonNode.Parse(json: person.ReprTree()) ?? new JsonObject();

        Assert.Equal(expected: "Person", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 4, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "John", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!.AsObject();
        Assert.Equal(expected: "int", actual: ageNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "30", actual: ageNode[propertyName: "value"]
          ?.ToString());
    }

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
            [propertyName: "ticks"] = "638712864000000000",
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
            [propertyName: "ticks"] = "638712864000000000",
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
            [propertyName: "ticks"] = "638712864000000000",
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
            [propertyName: "ticks"] = "18000000000",
            [propertyName: "isNegative"] = "false"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    // Integer Types
    [Theory]
    [InlineData(IntReprMode.Binary, "0b101010")]
    [InlineData(IntReprMode.Decimal, "42")]
    [InlineData(IntReprMode.Hex, "0x2A")]
    [InlineData(IntReprMode.HexBytes, "0x2A")]
    public void TestByteRepr(IntReprMode mode, string expectedValue)
    {
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(json: ((byte)42).ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "byte",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = expectedValue
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Theory]
    [InlineData(IntReprMode.Binary, "-0b101010")]
    [InlineData(IntReprMode.Decimal, "-42")]
    [InlineData(IntReprMode.Hex, "-0x2A")]
    [InlineData(IntReprMode.HexBytes, "0xFFFFFFD6")]
    public void TestIntRepr(IntReprMode mode, string expectedValue)
    {
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(json: (-42).ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "int",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = expectedValue
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var actualJson = JsonNode.Parse(json: new BigInteger(value: -42).ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "BigInteger",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "-42"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
        var value = Single.Parse(s: "3.1415926535");
        var actualJson = JsonNode.Parse(json: value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "float",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "3.1415927410125732421875E0"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 5);
        var value = Double.Parse(s: "3.1415926535");
        var actualJson = JsonNode.Parse(json: value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "double",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "3.14159"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Scientific, FloatPrecision: 5);
        var value = Half.Parse(s: "3.14159");
        var actualJson = JsonNode.Parse(json: value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Half",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "3.1406E+000"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDecimalRepr_RawHex()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.HexBytes);
        var value = 3.1415926535897932384626433832795m;
        var actualJson = JsonNode.Parse(json: value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "decimal",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "0x001C00006582A5360B14388541B65F29"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestHalfRepr_BitField()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.BitField);
        var value = Half.Parse(s: "3.14159");
        var actualJson = JsonNode.Parse(json: value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Half",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "0|10000|1001001000"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestFloatRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(json: Single.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "float",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Quiet NaN"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Single.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "float",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Single.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "float",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "-Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestDoubleRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(json: Double.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "double",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Quiet NaN"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Double.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "double",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Double.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "double",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "-Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    #if NET5_0_OR_GREATER
    [Fact]
    public void TestHalfRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(json: Half.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Half",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Quiet NaN"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Half.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Half",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));

        actualJson = JsonNode.Parse(json: Half.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Half",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = "-Infinity"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }
    #endif

    // Collections
    [Fact]
    public void TestListRepr()
    {
        // Test with an empty list
        var actualJson = JsonNode.Parse(json: new List<int>().ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 0, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Empty(collection: actualJson[propertyName: "value"]!.AsArray());

        // Test with a list of integers
        actualJson = JsonNode.Parse(json: new List<int> { 1, 2, 3 }.ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: valueArray[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: valueArray[index: 2]));

        // Test with a list of nullable strings
        actualJson = JsonNode.Parse(json: new List<string?> { "a", null, "c" }.ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);

        // Check first element: "a"
        var item1 = valueArray[index: 0]!.AsObject();
        Assert.Equal(expected: "string", actual: item1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item1[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: item1[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "a", actual: item1[propertyName: "value"]
          ?.ToString());

        // Check second element: null
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "object", [propertyName: "kind"] = "class",
                [propertyName: "value"] = null
            },
            node2: valueArray[index: 1]));

        // Check third element: "c"
        var item3 = valueArray[index: 2]!.AsObject();
        Assert.Equal(expected: "string", actual: item3[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item3[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item3[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: item3[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "c", actual: item3[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        var actualJson = JsonNode.Parse(json: Enumerable.Range(start: 1, count: 3)
                                                        .ReprTree())!;
        Assert.Equal(expected: "RangeIterator", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: valueArray[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: valueArray[index: 2]));
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        var actualJson = JsonNode.Parse(json: nestedList.ReprTree())!;

        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: outerArray.Count);

        // Check first nested list: { 1, 2 }
        var nested1 = outerArray[index: 0]!.AsObject();
        Assert.Equal(expected: "List", actual: nested1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested1[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: nested1[propertyName: "count"]!.GetValue<int>());
        var nested1Value = nested1[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: nested1Value.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: nested1Value[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: nested1Value[index: 1]));

        // Check second nested list: { 3, 4, 5 }
        var nested2 = outerArray[index: 1]!.AsObject();
        Assert.Equal(expected: "List", actual: nested2[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested2[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested2[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: nested2[propertyName: "count"]!.GetValue<int>());
        var nested2Value = nested2[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: nested2Value.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: nested2Value[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "4"
            },
            node2: nested2Value[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "5"
            },
            node2: nested2Value[index: 2]));

        // Check third nested list: { }
        var nested3 = outerArray[index: 2]!.AsObject();
        Assert.Equal(expected: "List", actual: nested3[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested3[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested3[propertyName: "hashCode"]);
        Assert.Equal(expected: 0, actual: nested3[propertyName: "count"]!.GetValue<int>());
        Assert.Empty(collection: nested3[propertyName: "value"]!.AsArray());
    }

    [Fact]
    public void TestArrayRepr()
    {
        var actualJson = JsonNode.Parse(json: Array.Empty<int>()
                                                   .ReprTree())!;
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(0),
            node2: actualJson[propertyName: "dimensions"]!));
        Assert.Empty(collection: actualJson[propertyName: "value"]!.AsArray());

        actualJson = JsonNode.Parse(json: new[] { 1, 2, 3 }.ReprTree())!;
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(3),
            node2: actualJson[propertyName: "dimensions"]!));
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: valueArray[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: valueArray[index: 2]));
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new[] { new[] { 1, 2 }, new[] { 3 } };
        var actualJson = JsonNode.Parse(json: jagged2D.ReprTree())!;

        // Check outer jagged array properties
        Assert.Equal(expected: "JaggedArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: actualJson[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: actualJson[propertyName: "dimensions"]!));
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "elementType"]
          ?.ToString());

        // Check the nested arrays structure
        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: outerArray.Count);

        // First inner array: int[] { 1, 2 }
        var innerArray1Json = outerArray[index: 0]!;
        Assert.Equal(expected: "1DArray", actual: innerArray1Json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: innerArray1Json[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: innerArray1Json[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: innerArray1Json[propertyName: "dimensions"]!));
        Assert.Equal(expected: "int", actual: innerArray1Json[propertyName: "elementType"]
          ?.ToString());

        var innerArray1Values = innerArray1Json[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray1Values.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int",
                [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1" // String value due to repr config
            },
            node2: innerArray1Values[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int",
                [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2" // String value due to repr config
            },
            node2: innerArray1Values[index: 1]));

        // Second inner array: int[] { 3 }
        var innerArray2Json = outerArray[index: 1]!;
        Assert.Equal(expected: "1DArray", actual: innerArray2Json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: innerArray2Json[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: innerArray2Json[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: innerArray2Json[propertyName: "dimensions"]!));
        Assert.Equal(expected: "int", actual: innerArray2Json[propertyName: "elementType"]
          ?.ToString());

        var innerArray2Values = innerArray2Json[propertyName: "value"]!.AsArray();
        Assert.Single(innerArray2Values);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int",
                [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3" // String value due to repr config
            },
            node2: innerArray2Values[index: 0]));
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new[,] { { 1, 2 }, { 3, 4 } };
        var actualJson = JsonNode.Parse(json: array2D.ReprTree())!;

        Assert.Equal(expected: "2DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2, 2),
            node2: actualJson[propertyName: "dimensions"]!));

        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: outerArray.Count);

        var innerArray1 = outerArray[index: 0]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray1.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: innerArray1[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: innerArray1[index: 1]));

        var innerArray2 = outerArray[index: 1]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray2.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: innerArray2[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "4"
            },
            node2: innerArray2[index: 1]));
    }

    [Fact]
    public void TestHashSetRepr()
    {
        var set = new HashSet<int> { 1, 2 };
        var actualJson = JsonNode.Parse(json: set.ReprTree())!;

        Assert.Equal(expected: "HashSet", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        var one = JsonNode.Parse(json: 1.ReprTree())!;
        var two = JsonNode.Parse(json: 2.ReprTree())!;
        Assert.Contains(collection: valueArray,
            filter: item => JsonNode.DeepEquals(node1: item, node2: one));
        Assert.Contains(collection: valueArray,
            filter: item => JsonNode.DeepEquals(node1: item, node2: two));
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var actualJson = JsonNode.Parse(json: set.ReprTree())!;

        Assert.Equal(expected: "SortedSet", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: valueArray[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3"
            },
            node2: valueArray[index: 2]));
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue(item: "first");
        queue.Enqueue(item: "second");
        var actualJson = JsonNode.Parse(json: queue.ReprTree())!;

        Assert.Equal(expected: "Queue", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);

        var item1 = valueArray[index: 0]!.AsObject();
        Assert.Equal(expected: "string", actual: item1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item1[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: item1[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "first", actual: item1[propertyName: "value"]
          ?.ToString());

        var item2 = valueArray[index: 1]!.AsObject();
        Assert.Equal(expected: "string", actual: item2[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item2[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item2[propertyName: "hashCode"]);
        Assert.Equal(expected: 6, actual: item2[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "second", actual: item2[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(item: 1);
        stack.Push(item: 2);
        var actualJson = JsonNode.Parse(json: stack.ReprTree())!;

        Assert.Equal(expected: "Stack", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2"
            },
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 1]));
    }

    [Fact]
    public void TestPriorityQueueRepr()
    {
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue(element: "second", priority: 2);
        pq.Enqueue(element: "first", priority: 1);
        pq.Enqueue(element: "third", priority: 3);

        var actualJson = JsonNode.Parse(json: pq.ReprTree())!;

        Assert.Equal(expected: "PriorityQueue", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "elementType"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: actualJson[propertyName: "priorityType"]
          ?.ToString());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "first" &&
            item[propertyName: "priority"]![propertyName: "value"]!.GetValue<string>() == "1");

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "second" &&
            item[propertyName: "priority"]![propertyName: "value"]!.GetValue<string>() == "2");

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "third" &&
            item[propertyName: "priority"]![propertyName: "value"]!.GetValue<string>() == "3");
    }

    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        var actualJson = JsonNode.Parse(json: point.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Point",
            [propertyName: "kind"] = "struct",
            [propertyName: "X"] = new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "10"
            },
            [propertyName: "Y"] = new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "20"
            }
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        var actualJson = JsonNode.Parse(json: custom.ReprTree())!;

        Assert.Equal(expected: "CustomStruct", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 4, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "test", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var valueNode = actualJson[propertyName: "Value"]!.AsObject();
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "42"
            }, node2: valueNode));
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        var actualJson = JsonNode.Parse(json: person.ReprTree())!;

        Assert.Equal(expected: "Person", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!.AsObject();
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "30"
            }, node2: ageNode));
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        var actualJson = JsonNode.Parse(json: noToString.ReprTree())!;

        Assert.Equal(expected: "NoToStringClass", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var dataNode = actualJson[propertyName: "Data"]!.AsObject();
        Assert.Equal(expected: "string", actual: dataNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: dataNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: dataNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 4, actual: dataNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "data", actual: dataNode[propertyName: "value"]
          ?.ToString());

        var numberNode = actualJson[propertyName: "Number"]!.AsObject();
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "123"
            },
            node2: numberNode));
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings(EquipmentName: "Printer",
            EquipmentSettings: new Dictionary<string, double>
                { [key: "Temp (C)"] = 200.0, [key: "PrintSpeed (mm/s)"] = 30.0 });
        var actualJson = JsonNode.Parse(json: settings.ReprTree())!;

        Assert.Equal(expected: "TestSettings", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "record class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var equipmentName = actualJson[propertyName: "EquipmentName"]!.AsObject();
        Assert.Equal(expected: "string", actual: equipmentName[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: equipmentName[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: equipmentName[propertyName: "hashCode"]);
        Assert.Equal(expected: 7, actual: equipmentName[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Printer", actual: equipmentName[propertyName: "value"]
          ?.ToString());

        var equipmentSettings = actualJson[propertyName: "EquipmentSettings"]!.AsObject();
        Assert.Equal(expected: "Dictionary", actual: equipmentSettings[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: equipmentSettings[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: equipmentSettings[propertyName: "hashCode"]);
        Assert.Equal(expected: 2,
            actual: equipmentSettings[propertyName: "count"]!.GetValue<int>());

        var settingsArray = equipmentSettings[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: settingsArray.Count);

        // Since dictionary order isn't guaranteed, we check for presence of keys
        var tempSetting =
            settingsArray.FirstOrDefault(predicate: s =>
                s![propertyName: "key"]![propertyName: "value"]!.ToString() == "Temp (C)");
        Assert.NotNull(@object: tempSetting);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "double", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "2.0E2"
            },
            node2: tempSetting[propertyName: "value"]));

        var speedSetting =
            settingsArray.FirstOrDefault(predicate: s =>
                s![propertyName: "key"]![propertyName: "value"]!.ToString() ==
                "PrintSpeed (mm/s)");
        Assert.NotNull(@object: speedSetting);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "double", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "3.0E1"
            },
            node2: speedSetting[propertyName: "value"]));
    }

    [Fact]
    public void TestEnumRepr()
    {
        var actualJson = JsonNode.Parse(json: Colors.GREEN.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Colors",
            [propertyName: "kind"] = "enum",
            [propertyName: "name"] = "GREEN",
            [propertyName: "value"] = new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            }
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestTupleRepr()
    {
        var actualJson = JsonNode.Parse(json: (1, "hello").ReprTree())!;

        Assert.Equal(expected: "ValueTuple", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "length"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);

        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: valueArray[index: 0]));

        var stringElement = valueArray[index: 1]!.AsObject();
        Assert.Equal(expected: "string", actual: stringElement[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: stringElement[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: stringElement[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: stringElement[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "hello", actual: stringElement[propertyName: "value"]
          ?.ToString());
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
        var actualJson = JsonNode.Parse(json: ((List<int>?)null).ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "List",
            [propertyName: "kind"] = "class",
            [propertyName: "value"] = null
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestListWithNullElements()
    {
        var listWithNull = new List<List<int>?> { new() { 1 }, null };
        var actualJson = JsonNode.Parse(json: listWithNull.ReprTree())!;

        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);

        var listNode = valueArray[index: 0]!.AsObject();
        Assert.Equal(expected: "List", actual: listNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: listNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: listNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: listNode[propertyName: "count"]!.GetValue<int>());
        var innerValue = listNode[propertyName: "value"]!.AsArray();
        Assert.Single(innerValue);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "1"
            },
            node2: innerValue[index: 0]));

        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "object", [propertyName: "kind"] = "class",
                [propertyName: "value"] = null
            },
            node2: valueArray[index: 1]));
    }

    #if NET7_0_OR_GREATER
    [Theory]
    [InlineData(IntReprMode.Binary,
        "-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData(IntReprMode.Decimal,
        "-170141183460469231731687303715884105728")]
    [InlineData(IntReprMode.Hex,
        "-0x80000000000000000000000000000000")]
    [InlineData(IntReprMode.HexBytes, "0x80000000000000000000000000000000")]
    public void TestInt128Repr(IntReprMode mode, string expectedValue)
    {
        var i128 = Int128.MinValue;
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(json: i128.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Int128",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = expectedValue
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Theory]
    [InlineData(IntReprMode.Binary,
        "0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111")]
    [InlineData(IntReprMode.Decimal, "170141183460469231731687303715884105727")]
    [InlineData(IntReprMode.Hex, "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    [InlineData(IntReprMode.HexBytes, "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    public void TestInt128Repr2(IntReprMode mode, string expectedValue)
    {
        var i128 = Int128.MaxValue;
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(json: i128.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Int128",
            [propertyName: "kind"] = "struct",
            [propertyName: "value"] = expectedValue
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }
    #endif

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
            [propertyName: "ticks"] = "18000000000",
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
            [propertyName: "ticks"] = "0",
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
            [propertyName: "ticks"] = "18000000000",
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
            [propertyName: "ticks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "0",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "ticks"] = "0",
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
            [propertyName: "ticks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "1",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "ticks"] = "36000000000",
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
            [propertyName: "ticks"] = "638712864000000000",
            [propertyName: "offset"] = new JsonObject
            {
                [propertyName: "type"] = "TimeSpan",
                [propertyName: "kind"] = "struct",
                [propertyName: "day"] = "0",
                [propertyName: "hour"] = "1",
                [propertyName: "minute"] = "0",
                [propertyName: "second"] = "0",
                [propertyName: "millisecond"] = "0",
                [propertyName: "ticks"] = "36000000000",
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
            [propertyName: "ticks"] = "0"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    public static int Add(int a, int b) => a + b;

    internal static long Add2(int a) => a;

    private T Add3<T>(T a) => a;

    private static void Add4(in int a, out int b) => b = a + 1;

    private async Task<int> Lambda(int a)
    {
        // Added delay for truly async function.
        // However, this would not do much because it is only used for testing purposes
        // and not being called, only investigated the metadata of it.
        await Task.Delay(millisecondsDelay: 1);
        return a;
    }

    [Fact]
    public void TestFunctionHierarchical()
    {
        var add5 = (int a) => a + 11;
        var a = Add;
        var b = Add2;
        var c = Add3<short>;
        var d = Add4;
        var e = Lambda;
        var nullJsonObject = new JsonObject
        {
            [propertyName: "type"] = "object", [propertyName: "kind"] = "class",
            [propertyName: "value"] = null
        };

        var actualJson = JsonNode.Parse(json: add5.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        var props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Lambda", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("internal"),
            node2: props[propertyName: "modifiers"]));
        var parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: parameters[propertyName: "dimensions"]!));
        Assert.NotNull(@object: parameters[propertyName: "value"]);
        var parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Single(parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: a.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("public", "static"),
            node2: props[propertyName: "modifiers"]));
        parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: parameters[propertyName: "dimensions"]!));
        Assert.NotNull(@object: parameters[propertyName: "value"]);
        parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: parameterArray.Count);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
        Assert.Equal(expected: "int", actual: parameterArray[index: 1]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "b", actual: parameterArray[index: 1]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 1]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 1]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: b.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add2", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "long", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("internal", "static"),
            node2: props[propertyName: "modifiers"]));
        parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: parameters[propertyName: "dimensions"]!));
        Assert.NotNull(@object: parameters[propertyName: "value"]);
        parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Single(parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: c.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add3", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "short", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "generic"),
            node2: props[propertyName: "modifiers"]));
        parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: parameters[propertyName: "dimensions"]!));
        parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Single(parameterArray);
        Assert.Equal(expected: "short", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: d.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add4", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "void", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "static"),
            node2: props[propertyName: "modifiers"]));
        parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: parameters[propertyName: "dimensions"]!));
        parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: parameterArray.Count);
        Assert.Equal(expected: "ref int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "in", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
        Assert.Equal(expected: "ref int", actual: parameterArray[index: 1]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "b", actual: parameterArray[index: 1]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "out", actual: parameterArray[index: 1]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 1]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: e.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Lambda", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "Task<int>", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "async"),
            node2: props[propertyName: "modifiers"]));
        parameters = props[propertyName: "parameters"]!.AsObject();
        Assert.Equal(expected: "1DArray", actual: parameters[propertyName: "type"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: parameters[propertyName: "dimensions"]!));
        parameterArray = parameters[propertyName: "value"]!.AsArray();
        Assert.Single(parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
    }

    [Fact]
    public void TestObjectReprTree()
    {
        var data = new { Name = "Alice", Age = 30 };
        var actualJsonNode = JsonNode.Parse(json: data.ReprTree())!;

        Assert.Equal(expected: "Anonymous", actual: actualJsonNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJsonNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJsonNode[propertyName: "hashCode"]);

        var nameNode = actualJsonNode[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJsonNode[propertyName: "Age"]!.AsObject();
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "int", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "30"
            }, node2: ageNode));
    }

    [Fact]
    public void TestCircularReprTree()
    {
        List<object> a = new();
        a.Add(item: a);
        var actualJsonString = a.ReprTree();

        // Parse the JSON to verify structure
        var json = JsonNode.Parse(json: actualJsonString)!;

        // Verify top-level structure
        Assert.Equal(expected: "List", actual: json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: json[propertyName: "count"]!.GetValue<int>());

        // Verify circular reference structure
        var firstElement = json[propertyName: "value"]![index: 0]!;
        Assert.Equal(expected: "CircularReference", actual: firstElement[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "List",
            actual: firstElement[propertyName: "target"]![propertyName: "type"]
              ?.ToString());
        Assert.StartsWith(expectedStartString: "0x",
            actualString: firstElement[propertyName: "target"]![propertyName: "hashCode"]
              ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxDepth_ReprTree()
    {
        var nestedList = new List<object> { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = new ReprConfig(MaxDepth: 1);
        var actualJson = JsonNode.Parse(json: nestedList.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "struct",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "kind"]
              ?.ToString());
        Assert.Equal(expected: "1",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "value"]
              ?.ToString());
        Assert.Equal(expected: "List",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "class",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "kind"]
              ?.ToString());
        Assert.Equal(expected: "true",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "maxDepthReached"]
              ?.ToString());
        Assert.Equal(expected: 1,
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "depth"]!
               .GetValue<int>());

        config = new ReprConfig(MaxDepth: 0);
        actualJson = JsonNode.Parse(json: nestedList.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: "true", actual: actualJson[propertyName: "maxDepthReached"]
          ?.ToString());
        Assert.Equal(expected: 0, actual: actualJson[propertyName: "depth"]!.GetValue<int>());
    }

    [Fact]
    public void TestReprConfig_MaxCollectionItems_ReprTree()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var config = new ReprConfig(MaxElementsPerCollection: 3);
        var actualJson = JsonNode.Parse(json: list.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: 4, actual: actualJson[propertyName: "value"]!.AsArray()
           .Count);
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 2]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "... (2 more items)",
            actual: actualJson[propertyName: "value"]![index: 3]
              ?.ToString());

        config = new ReprConfig(MaxElementsPerCollection: 0);
        actualJson = JsonNode.Parse(json: list.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "... (5 more items)",
            actual: actualJson[propertyName: "value"]![index: 0]
              ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxStringLength_ReprTree()
    {
        var longString = "This is a very long string that should be truncated.";
        var config = new ReprConfig(MaxStringLength: 10);
        var actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "This is a ... (42 more letters)",
            actual: actualJson[propertyName: "value"]
              ?.ToString());

        config = new ReprConfig(MaxStringLength: 0);
        actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "... (52 more letters)", actual: actualJson[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_ShowNonPublicProperties_ReprTree()
    {
        var classified = new ClassifiedData("writer", "secret");
        var config = new ReprConfig(ShowNonPublicProperties: false);
        var actualJson = JsonNode.Parse(json: classified.ReprTree(config: config));
        Assert.NotNull(actualJson);
        Assert.Equal(expected: "ClassifiedData", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var writerNode = actualJson[propertyName: "Writer"]!.AsObject();
        Assert.Equal(expected: "string", actual: writerNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: writerNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "writer", actual: writerNode[propertyName: "value"]
          ?.ToString());


        config = new ReprConfig(ShowNonPublicProperties: true);
        actualJson = JsonNode.Parse(json: classified.ReprTree(config: config));
        Assert.NotNull(actualJson);
        Assert.Equal(expected: "ClassifiedData", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);


        writerNode = actualJson[propertyName: "Writer"]!.AsObject();
        Assert.Equal(expected: "string", actual: writerNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: writerNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "writer", actual: writerNode[propertyName: "value"]
          ?.ToString());

        var secretNode = actualJson[propertyName: "private_Data"];
        Assert.NotNull(secretNode);
        Assert.Equal(expected: "string", actual: secretNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: secretNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "secret", actual: secretNode[propertyName: "value"]
          ?.ToString());
    }
}