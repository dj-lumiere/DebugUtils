using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using DebugUtils.Repr;
using DebugUtils.Repr.Records;

namespace DebugUtils.Tests;

public class ReprTreeTest
{
    [Fact]
    public void TestNullRepr()
    {
        var actualJson = JsonNode.Parse(((string?)null).ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "string",
            ["kind"] = "class",
            ["value"] = null
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestStringRepr()
    {
        var actualJson = JsonNode.Parse("hello".ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "string",
            ["kind"] = "class",
            ["length"] = "5",
            ["value"] = "hello"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse("".ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "string",
            ["kind"] = "class",
            ["length"] = "0",
            ["value"] = ""
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestCharRepr()
    {
        var actualJson = JsonNode.Parse('A'.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "char",
            ["kind"] = "struct",
            ["value"] = "A",
            ["unicodeValue"] = "0x0041"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse('\n'.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "char",
            ["kind"] = "struct",
            ["value"] = "\n",
            ["unicodeValue"] = "0x000A"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse('\u007F'.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "char",
            ["kind"] = "struct",
            ["value"] = "\u007F",
            ["unicodeValue"] = "0x007F"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse('아'.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "char",
            ["kind"] = "struct",
            ["value"] = "아",
            ["unicodeValue"] = "0xC544"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestRuneRepr()
    {
        var rune = new Rune(value: 0x1f49c);
        var actualJson = JsonNode.Parse(rune.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Rune",
            ["kind"] = "struct",
            ["value"] = rune.ToString(),
            ["unicodeValue"] = "0x0001F49C"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestBoolRepr()
    {
        var actualJson = JsonNode.Parse(true.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "bool",
            ["kind"] = "struct",
            ["value"] = "true"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDateTimeRepr()
    {
        var dateTime = new DateTime(year: 2025, month: 1, day: 1, hour: 0, minute: 0, second: 0);
        var localDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Local);
        var utcDateTime = DateTime.SpecifyKind(value: dateTime, kind: DateTimeKind.Utc);

        var actualJson = JsonNode.Parse(dateTime.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "DateTime",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["timezone"] = "Unspecified"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(localDateTime.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "DateTime",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["timezone"] = "Local"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(utcDateTime.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "DateTime",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["timezone"] = "Utc"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr()
    {
        var timeSpan = TimeSpan.FromMinutes(minutes: 30);
        var actualJson = JsonNode.Parse(timeSpan.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "TimeSpan",
            ["kind"] = "struct",
            ["day"] = "0",
            ["hour"] = "0",
            ["minute"] = "30",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "18000000000",
            ["isNegative"] = "false"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
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
        var actualJson = JsonNode.Parse(((byte)42).ReprTree(config));
        var expectedJson = new JsonObject
        {
            ["type"] = "byte",
            ["kind"] = "struct",
            ["value"] = expectedValue
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Theory]
    [InlineData(IntReprMode.Binary, "-0b101010")]
    [InlineData(IntReprMode.Decimal, "-42")]
    [InlineData(IntReprMode.Hex, "-0x2A")]
    [InlineData(IntReprMode.HexBytes, "0xFFFFFFD6")]
    public void TestIntRepr(IntReprMode mode, string expectedValue)
    {
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse((-42).ReprTree(config));
        var expectedJson = new JsonObject
        {
            ["type"] = "int",
            ["kind"] = "struct",
            ["value"] = expectedValue
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var actualJson = JsonNode.Parse(new BigInteger(value: -42).ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "BigInteger",
            ["kind"] = "struct",
            ["value"] = "-42"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
        var value = Single.Parse(s: "3.1415926535");
        var actualJson = JsonNode.Parse(value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "float",
            ["kind"] = "struct",
            ["value"] = "3.1415927410125732421875E0"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 5);
        var value = Double.Parse(s: "3.1415926535");
        var actualJson = JsonNode.Parse(value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "double",
            ["kind"] = "struct",
            ["value"] = "3.14159"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Scientific, FloatPrecision: 5);
        var value = Half.Parse(s: "3.14159");
        var actualJson = JsonNode.Parse(value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "Half",
            ["kind"] = "struct",
            ["value"] = "3.1406E+000"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDecimalRepr_RawHex()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.HexBytes);
        var value = 3.1415926535897932384626433832795m;
        var actualJson = JsonNode.Parse(value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "decimal",
            ["kind"] = "struct",
            ["value"] = "0x001C00006582A5360B14388541B65F29"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestHalfRepr_BitField()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.BitField);
        var value = Half.Parse(s: "3.14159");
        var actualJson = JsonNode.Parse(value.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "Half",
            ["kind"] = "struct",
            ["value"] = "0|10000|1001001000"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestFloatRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(float.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "float",
            ["kind"] = "struct",
            ["value"] = "Quiet NaN"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(float.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "float",
            ["kind"] = "struct",
            ["value"] = "Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(float.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "float",
            ["kind"] = "struct",
            ["value"] = "-Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDoubleRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(double.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "double",
            ["kind"] = "struct",
            ["value"] = "Quiet NaN"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(double.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "double",
            ["kind"] = "struct",
            ["value"] = "Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(double.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "double",
            ["kind"] = "struct",
            ["value"] = "-Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    #if NET5_0_OR_GREATER
    [Fact]
    public void TestHalfRepr_SpecialValues()
    {
        var actualJson = JsonNode.Parse(Half.NaN.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Half",
            ["kind"] = "struct",
            ["value"] = "Quiet NaN"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(Half.PositiveInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Half",
            ["kind"] = "struct",
            ["value"] = "Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(Half.NegativeInfinity.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Half",
            ["kind"] = "struct",
            ["value"] = "-Infinity"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }
    #endif

    // Collections
    [Fact]
    public void TestListRepr()
    {
        var actualJson = JsonNode.Parse(new List<int>().ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["count"] = "0",
            ["value"] = new JsonArray()
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(new List<int> { 1, 2, 3 }.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["count"] = "3",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "2" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "3" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(new List<string?> { "a", null, "c" }.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["count"] = "3",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "string", ["kind"] = "class", ["length"] = "1", ["value"] = "a" },
                new JsonObject { ["type"] = "object", ["kind"] = "class", ["value"] = null },
                new JsonObject
                    { ["type"] = "string", ["kind"] = "class", ["length"] = "1", ["value"] = "c" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        var actualJson = JsonNode.Parse(Enumerable.Range(start: 1, count: 3)
                                                  .ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "RangeIterator",
            ["kind"] = "class",
            ["count"] = "3",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "2" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "3" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        var actualJson = JsonNode.Parse(nestedList.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["count"] = "3",
            ["value"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "List",
                    ["kind"] = "class",
                    ["count"] = "2",
                    ["value"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "1"
                        },
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "2"
                        }
                    )
                },
                new JsonObject
                {
                    ["type"] = "List",
                    ["kind"] = "class",
                    ["count"] = "3",
                    ["value"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "3"
                        },
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "4"
                        },
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "5"
                        }
                    )
                },
                new JsonObject
                {
                    ["type"] = "List",
                    ["kind"] = "class",
                    ["count"] = "0",
                    ["value"] = new JsonArray()
                }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestArrayRepr()
    {
        var actualJson = JsonNode.Parse(Array.Empty<int>()
                                             .ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "1DArray",
            ["kind"] = "class",
            ["length"] = "0",
            ["value"] = new JsonArray()
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(new[] { 1, 2, 3 }.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "1DArray",
            ["kind"] = "class",
            ["length"] = "3",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "2" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "3" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new[]
            { new[] { 1, 2 }, new[] { 3 } };
        var actualJson = JsonNode.Parse(jagged2D.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "JaggedArray",
            ["kind"] = "class",
            ["length"] = "2",
            ["value"] = new JsonArray(
                new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "1"
                    },
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "2"
                    }
                },
                new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "3"
                    }
                }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new[,] { { 1, 2 }, { 3, 4 } };
        var actualJson = JsonNode.Parse(array2D.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "2DArray",
            ["kind"] = "class",
            ["length"] = "4",
            ["value"] = new JsonArray
            {
                new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "1"
                    },
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "2"
                    }
                },
                new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "3"
                    },
                    new JsonObject
                    {
                        ["type"] = "int", ["kind"] = "struct", ["value"] = "4"
                    }

                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestHashSetRepr()
    {
        var set = new HashSet<int> { 1, 2 };
        var actualJson = JsonNode.Parse(set.ReprTree());

        Assert.Equal("HashSet", actualJson["type"]
          ?.ToString());
        Assert.Equal("class", actualJson["kind"]
          ?.ToString());
        Assert.Equal("2", actualJson["count"]
          ?.ToString());

        var valueArray = actualJson["value"]
          ?.AsArray();
        Assert.Contains(valueArray, item => item?["value"]
          ?.ToString() == "1");
        Assert.Contains(valueArray, item => item?["value"]
          ?.ToString() == "2");
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var actualJson = JsonNode.Parse(set.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "SortedSet",
            ["kind"] = "class",
            ["count"] = "3",
            ["value"] = new JsonArray(
                new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" },
                new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "2" },
                new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "3" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue(item: "first");
        queue.Enqueue(item: "second");
        var actualJson = JsonNode.Parse(queue.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Queue",
            ["kind"] = "class",
            ["count"] = "2",
            ["value"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "string", ["kind"] = "class", ["length"] = "5", ["value"] = "first"
                },
                new JsonObject
                {
                    ["type"] = "string", ["kind"] = "class", ["length"] = "6", ["value"] = "second"
                }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(item: 1);
        stack.Push(item: 2);
        var actualJson = JsonNode.Parse(stack.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Stack",
            ["kind"] = "class",
            ["count"] = "2",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "2" },
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        var actualJson = JsonNode.Parse(point.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Point",
            ["kind"] = "struct",
            ["X"] = new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "10" },
            ["Y"] = new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "20" }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        var actualJson = JsonNode.Parse(custom.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "CustomStruct",
            ["kind"] = "struct",
            ["Name"] = new JsonObject
                { ["type"] = "string", ["kind"] = "class", ["length"] = "4", ["value"] = "test" },
            ["Value"] = new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "42" }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        var actualJson = JsonNode.Parse(person.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Person",
            ["kind"] = "class",
            ["Name"] = new JsonObject
                { ["type"] = "string", ["kind"] = "class", ["length"] = "5", ["value"] = "Alice" },
            ["Age"] = new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "30" }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        var actualJson = JsonNode.Parse(noToString.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "NoToStringClass",
            ["kind"] = "class",
            ["Data"] = new JsonObject
                { ["type"] = "string", ["kind"] = "class", ["length"] = "4", ["value"] = "data" },
            ["Number"] = new JsonObject
                { ["type"] = "int", ["kind"] = "struct", ["value"] = "123" }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings(EquipmentName: "Printer",
            EquipmentSettings: new Dictionary<string, double>
                { ["Temp (C)"] = 200.0, ["PrintSpeed (mm/s)"] = 30.0 });
        var actualJson = JsonNode.Parse(settings.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "TestSettings",
            ["kind"] = "record class",
            ["EquipmentName"] = new JsonObject
            {
                ["type"] = "string", ["kind"] = "class", ["length"] = "7", ["value"] = "Printer"
            },
            ["EquipmentSettings"] = new JsonObject
            {
                ["type"] = "Dictionary", ["kind"] = "class", ["count"] = "2", ["value"] =
                    new JsonArray
                    {
                        new JsonObject
                        {
                            ["key"] = new JsonObject
                            {
                                ["type"] = "string", ["kind"] = "class", ["length"] = "8",
                                ["value"] = "Temp (C)"
                            },
                            ["value"] = new JsonObject
                                { ["type"] = "double", ["kind"] = "struct", ["value"] = "2.0E2" }
                        },
                        new JsonObject
                        {
                            ["key"] = new JsonObject
                            {
                                ["type"] = "string", ["kind"] = "class", ["length"] = "17",
                                ["value"] = "PrintSpeed (mm/s)"
                            },
                            ["value"] = new JsonObject
                                { ["type"] = "double", ["kind"] = "struct", ["value"] = "3.0E1" }
                        }
                    }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestEnumRepr()
    {
        var actualJson = JsonNode.Parse(Colors.GREEN.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Colors",
            ["kind"] = "enum",
            ["name"] = "GREEN",
            ["value"] = new JsonObject { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTupleRepr()
    {
        var actualJson = JsonNode.Parse((1, "hello").ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "ValueTuple",
            ["kind"] = "struct",
            ["length"] = "2",
            ["value"] = new JsonArray(
                new JsonObject
                    { ["type"] = "int", ["kind"] = "struct", ["value"] = "1" },
                new JsonObject
                {
                    ["type"] = "string", ["kind"] = "class", ["length"] = "5", ["value"] = "hello"
                }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestNullableStructRepr()
    {
        var actualJson = JsonNode.Parse(((int?)123).ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "int?",
            ["kind"] = "struct",
            ["value"] = "123"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(((int?)null).ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "int?",
            ["kind"] = "struct",
            ["value"] = null
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestNullableClassRepr()
    {
        var actualJson = JsonNode.Parse(((List<int>?)null).ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["value"] = null
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestListWithNullElements()
    {
        var listWithNull = new List<List<int>?> { new() { 1 }, null };
        var actualJson = JsonNode.Parse(listWithNull.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "List",
            ["kind"] = "class",
            ["count"] = "2",
            ["value"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "List",
                    ["kind"] = "class",
                    ["count"] = "1",
                    ["value"] = new JsonArray(
                        new JsonObject
                        {
                            ["type"] = "int", ["kind"] = "struct", ["value"] = "1"
                        }
                    )
                },
                new JsonObject
                {
                    ["type"] = "object",
                    ["kind"] = "class",
                    ["value"] = null
                }
            )
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    #if NET7_0_OR_GREATER
    [Theory]
    [InlineData(IntReprMode.Binary,
        "-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData(IntReprMode.Decimal,
        "-170141183460469231731687303715884105728")]
    [InlineData(IntReprMode.Hex,
        "-0x80000000000000000000000000000000")]
    [InlineData(IntReprMode.HexBytes,
        "0x80000000000000000000000000000000")]
    public void TestInt128Repr(IntReprMode mode, string expectedValue)
    {
        var i128 = Int128.MinValue;
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(i128.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "Int128",
            ["kind"] = "struct",
            ["value"] = expectedValue
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Theory]
    [InlineData(IntReprMode.Binary,
        "0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111")]
    [InlineData(IntReprMode.Decimal,
        "170141183460469231731687303715884105727")]
    [InlineData(IntReprMode.Hex,
        "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    [InlineData(IntReprMode.HexBytes,
        "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    public void TestInt128Repr2(IntReprMode mode, string expectedValue)
    {
        var i128 = Int128.MaxValue;
        var config = new ReprConfig(IntMode: mode);
        var actualJson = JsonNode.Parse(i128.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "Int128",
            ["kind"] = "struct",
            ["value"] = expectedValue
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }
    #endif

    [Fact]
    public void TestGuidRepr()
    {
        var guid = Guid.NewGuid();
        var actualJson = JsonNode.Parse(guid.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Guid",
            ["kind"] = "struct",
            ["value"] = guid.ToString()
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Negative()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.FromMinutes(minutes: -30);
        var actualJson = JsonNode.Parse(timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "TimeSpan",
            ["kind"] = "struct",
            ["day"] = "0",
            ["hour"] = "0",
            ["minute"] = "30",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "18000000000",
            ["isNegative"] = "true"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Zero()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.Zero;
        var actualJson = JsonNode.Parse(timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "TimeSpan",
            ["kind"] = "struct",
            ["day"] = "0",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "0",
            ["isNegative"] = "false"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTimeSpanRepr_Positive()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        var timeSpan = TimeSpan.FromMinutes(minutes: 30);
        var actualJson = JsonNode.Parse(timeSpan.ReprTree(config: config));
        var expectedJson = new JsonObject
        {
            ["type"] = "TimeSpan",
            ["kind"] = "struct",
            ["day"] = "0",
            ["hour"] = "0",
            ["minute"] = "30",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "18000000000",
            ["isNegative"] = "false"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDateTimeOffsetRepr()
    {
        var dateTimeOffset = new DateTimeOffset(dateTime: new DateTime(
            date: new DateOnly(year: 2025, month: 1, day: 1),
            time: new TimeOnly(hour: 0, minute: 0, second: 0),
            kind: DateTimeKind.Utc));
        var actualJson = JsonNode.Parse(dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "DateTimeOffset",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["offset"] = new JsonObject
            {
                ["type"] = "TimeSpan",
                ["kind"] = "struct",
                ["day"] = "0",
                ["hour"] = "0",
                ["minute"] = "0",
                ["second"] = "0",
                ["millisecond"] = "0",
                ["ticks"] = "0",
                ["isNegative"] = "false"
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDateTimeOffsetRepr_WithOffset()
    {
        var dateTimeOffset = new DateTimeOffset(
            dateTime: new DateTime(year: 2025, month: 1, day: 1),
            offset: TimeSpan.FromHours(hours: 1));
        var actualJson = JsonNode.Parse(dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "DateTimeOffset",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["offset"] = new JsonObject
            {
                ["type"] = "TimeSpan",
                ["kind"] = "struct",
                ["day"] = "0",
                ["hour"] = "1",
                ["minute"] = "0",
                ["second"] = "0",
                ["millisecond"] = "0",
                ["ticks"] = "36000000000",
                ["isNegative"] = "false"
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }
    [Fact]
    public void TestDateTimeOffsetRepr_WithNegativeOffset()
    {
        var dateTimeOffset = new DateTimeOffset(
            dateTime: new DateTime(year: 2025, month: 1, day: 1),
            offset: TimeSpan.FromHours(hours: -1));
        var actualJson = JsonNode.Parse(dateTimeOffset.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "DateTimeOffset",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "638712864000000000",
            ["offset"] = new JsonObject
            {
                ["type"] = "TimeSpan",
                ["kind"] = "struct",
                ["day"] = "0",
                ["hour"] = "1",
                ["minute"] = "0",
                ["second"] = "0",
                ["millisecond"] = "0",
                ["ticks"] = "36000000000",
                ["isNegative"] = "true"
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestDateOnly()
    {
        var dateOnly = new DateOnly(year: 2025, month: 1, day: 1);
        var actualJson = JsonNode.Parse(dateOnly.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "DateOnly",
            ["kind"] = "struct",
            ["year"] = "2025",
            ["month"] = "1",
            ["day"] = "1"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestTimeOnly()
    {
        var timeOnly = new TimeOnly(hour: 0, minute: 0, second: 0);
        var actualJson = JsonNode.Parse(timeOnly.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "TimeOnly",
            ["kind"] = "struct",
            ["hour"] = "0",
            ["minute"] = "0",
            ["second"] = "0",
            ["millisecond"] = "0",
            ["ticks"] = "0"
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    public static int Add(int a, int b)
    {
        return a + b;
    }

    internal static long Add2(int a)
    {
        return a;
    }

    private T Add3<T>(T a)
    {
        return a;
    }

    private static void Add4(in int a, out int b)
    {
        b = a + 1;
    }

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
        var Add5 = (int a) => a + 11;
        var a = Add;
        var b = Add2;
        var c = Add3<short>;
        var d = Add4;
        var e = Lambda;

        var actualJson = JsonNode.Parse(Add5.ReprTree());
        var expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Lambda",
                ["returnType"] = "int",
                ["modifiers"] = new JsonArray("internal"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "1", ["value"] =
                        new JsonArray(
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "int", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        )
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(a.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Add",
                ["returnType"] = "int",
                ["modifiers"] = new JsonArray("public", "static"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "2", ["value"] =
                        new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "int", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            },
                            new JsonObject
                            {
                                ["name"] = "b", ["type"] = "int", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        }
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(b.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Add2",
                ["returnType"] = "long",
                ["modifiers"] = new JsonArray("internal", "static"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "1", ["value"] =
                        new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "int", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        }
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(c.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Add3",
                ["returnType"] = "short",
                ["modifiers"] = new JsonArray("private", "generic"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "1", ["value"] =
                        new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "short", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        }
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));

        actualJson = JsonNode.Parse(d.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Add4",
                ["returnType"] = "void",
                ["modifiers"] = new JsonArray("private", "static"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "2", ["value"] =
                        new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "ref int", ["modifier"] = "in",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            },
                            new JsonObject
                            {
                                ["name"] = "b", ["type"] = "ref int", ["modifier"] = "out",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        }
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
        actualJson = JsonNode.Parse(e.ReprTree());
        expectedJson = new JsonObject
        {
            ["type"] = "Function",
            ["properties"] = new JsonObject
            {
                ["name"] = "Lambda",
                ["returnType"] = "Task<int>",
                ["modifiers"] = new JsonArray("private", "async"),
                ["parameters"] = new JsonObject
                {
                    ["type"] = "1DArray", ["kind"] = "class", ["length"] = "1", ["value"] =
                        new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "a", ["type"] = "int", ["modifier"] = "",
                                ["defaultValue"] = new JsonObject
                                    { ["type"] = "object", ["kind"] = "class", ["value"] = null }
                            }
                        }
                }
            }
        };
        Assert.True(JsonNode.DeepEquals(actualJson, expectedJson));
    }

    [Fact]
    public void TestObjectReprTree()
    {
        var data = new { Name = "Alice", Age = 30 };
        var actualJsonString = data.ReprTree();
        var actualJsonNode = JsonNode.Parse(json: actualJsonString);
        var expectedJsonNode = new JsonObject
        {
            [propertyName: "type"] = "Anonymous",
            [propertyName: "kind"] = "class",
            [propertyName: "Name"] = new JsonObject
            {
                [propertyName: "type"] = "string",
                [propertyName: "kind"] = "class",
                ["length"] = "5",
                [propertyName: "value"] = "Alice"
            },
            [propertyName: "Age"] = new JsonObject
            {
                [propertyName: "type"] = "int",
                [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "30"
            }
        };
        Assert.True(
            condition: JsonNode.DeepEquals(node1: actualJsonNode, node2: expectedJsonNode));
    }

    [Fact]
    public void TestCircularReprTree()
    {
        List<object> a = new();
        a.Add(item: a);
        var actualJsonString = a.ReprTree();

        // Parse the JSON to verify structure
        var json = JsonNode.Parse(json: actualJsonString);

        // Verify top-level structure
        Assert.Equal(expected: "List", actual: json?["type"]
          ?.ToString());
        Assert.Equal(expected: "1", actual: json?["count"]
          ?.ToString());

        // Verify circular reference structure
        var firstElement = json?["value"]?[0] ?? null;
        Assert.Equal(expected: "CircularReference", actual: firstElement?["type"]
          ?.ToString());
        Assert.Equal(expected: "List",
            actual: firstElement?["target"]?["type"]
              ?.ToString());
        Assert.StartsWith(expectedStartString: "0x",
            actualString: firstElement?["target"]?["hashCode"]
              ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxDepth_ReprTree()
    {
        var nestedList = new List<object>
            { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = new ReprConfig(MaxDepth: 1);
        var actualJson = JsonNode.Parse(nestedList.ReprTree(config: config));
        Assert.Equal("List", actualJson["type"]
          ?.ToString());
        Assert.Equal("2", actualJson["count"]
          ?.ToString());
        Assert.Equal("int", actualJson["value"]?[0]?["type"]
          ?.ToString());
        Assert.Equal("struct", actualJson["value"]?[0]?["kind"]
          ?.ToString());
        Assert.Equal("1", actualJson["value"]?[0]?["value"]
          ?.ToString());
        Assert.Equal("List", actualJson["value"]?[1]?["type"]
          ?.ToString());
        Assert.Equal("class", actualJson["value"]?[1]?["kind"]
          ?.ToString());
        Assert.Equal("true", actualJson["value"]?[1]?["maxDepthReached"]
          ?.ToString());
        Assert.Equal("1", actualJson["value"]?[1]?["depth"]
          ?.ToString());

        config = new ReprConfig(MaxDepth: 0);
        actualJson = JsonNode.Parse(nestedList.ReprTree(config: config));
        Assert.Equal("List", actualJson["type"]
          ?.ToString());
        Assert.Equal("class", actualJson["kind"]
          ?.ToString());
        Assert.Equal("true", actualJson["maxDepthReached"]
          ?.ToString());
        Assert.Equal("0", actualJson["depth"]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxCollectionItems_ReprTree()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var config = new ReprConfig(MaxElementsPerCollection: 3);
        var actualJson = JsonNode.Parse(list.ReprTree(config: config));
        Assert.Equal("List", actualJson["type"]
          ?.ToString());
        Assert.Equal("5", actualJson["count"]
          ?.ToString());
        Assert.Equal(4, actualJson["value"]
                      ?.AsArray()
                       .Count);
        Assert.Equal("int", actualJson["value"]?[0]?["type"]
          ?.ToString());
        Assert.Equal("int", actualJson["value"]?[1]?["type"]
          ?.ToString());
        Assert.Equal("int", actualJson["value"]?[2]?["type"]
          ?.ToString());
        Assert.Equal("... (2 more items)", actualJson["value"]?[3]
          ?.ToString());

        config = new ReprConfig(MaxElementsPerCollection: 0);
        actualJson = JsonNode.Parse(list.ReprTree(config: config));
        Assert.Equal("List", actualJson["type"]
          ?.ToString());
        Assert.Equal("... (5 more items)", actualJson["value"]?[0]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxStringLength_ReprTree()
    {
        var longString = "This is a very long string that should be truncated.";
        var config = new ReprConfig(MaxStringLength: 10);
        var actualJson = JsonNode.Parse(longString.ReprTree(config: config));
        Assert.Equal("string", actualJson["type"]
          ?.ToString());
        Assert.Equal("This is a ... (42 more letters)", actualJson["value"]
          ?.ToString());

        config = new ReprConfig(MaxStringLength: 0);
        actualJson = JsonNode.Parse(longString.ReprTree(config: config));
        Assert.Equal("string", actualJson["type"]
          ?.ToString());
        Assert.Equal("... (52 more letters)", actualJson["value"]
          ?.ToString());
    }
}