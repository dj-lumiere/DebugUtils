using System.Numerics;
using System.Text;
using Xunit;
using DebugUtils;
using System.Collections.Generic;
using System.Linq;
using DebugUtils.Records;

namespace DebugUtils.Tests;

// Test data structures from DebugUtilsTest.cs
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct CustomStruct
{
    public string Name;
    public int Value;
    public override string ToString() => $"Custom({Name}, {Value})";
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
    public override string ToString() => $"{Name}({Age})";
}

public class NoToStringClass
{
    public string Data { get; set; }
    public int Number { get; set; }
    public NoToStringClass(string data, int number)
    {
        Data = data;
        Number = number;
    }
}

public record TestSettings(string EquipmentName, Dictionary<String, double> EquipmentSettings);

public enum Colors
{
    RED,
    GREEN,
    BLUE
}

public class ReprTest
{
    // Basic Types
    [Fact]
    public void TestNullRepr()
    {
        Assert.Equal("null", ((string?)null).Repr());
    }

    [Fact]
    public void TestStringRepr()
    {
        Assert.Equal("\"hello\"", "hello".Repr());
        Assert.Equal("\"\"", "".Repr());
    }

    [Fact]
    public void TestCharRepr()
    {
        Assert.Equal("'A'", 'A'.Repr());
        Assert.Equal("'\\n'", '\n'.Repr());
        Assert.Equal("'\\u007F'", '\u007F'.Repr());
        Assert.Equal("'아'", '아'.Repr());
    }

    [Fact]
    public void TestRuneRepr()
    {
        Assert.Equal("Rune(💜 @ \\U0001F49C)", (new Rune(0x1f49c)).Repr());
    }

    [Fact]
    public void TestBoolRepr()
    {
        Assert.Equal("true", true.Repr());
    }

    [Fact]
    public void TestDateTimeRepr()
    {
        Assert.Equal("DateTime(2025-01-01 00:00:00)", DateTime.Parse("2025-01-01")
            .Repr());
    }

    [Fact]
    public void TestTimeSpanRepr()
    {
        Assert.Equal("TimeSpan(1800.000s)", TimeSpan.FromMinutes(30)
            .Repr());
    }

    // Integer Types
    [Theory]
    [InlineData(IntReprMode.Binary, "byte(0b101010)")]
    [InlineData(IntReprMode.Decimal, "byte(42)")]
    [InlineData(IntReprMode.Hex, "byte(0x2A)")]
    [InlineData(IntReprMode.HexBytes, "byte(0x2A)")]
    public void TestByteRepr(IntReprMode mode, string expected)
    {
        var config = new ReprConfig(IntMode: mode);
        Assert.Equal(expected, ((byte)42).Repr(config: config));
    }

    [Theory]
    [InlineData(IntReprMode.Binary, "int(-0b101010)")]
    [InlineData(IntReprMode.Decimal, "int(-42)")]
    [InlineData(IntReprMode.Hex, "int(-0x2A)")]
    [InlineData(IntReprMode.HexBytes, "int(0xFFFFFFD6)")]
    public void TestIntRepr(IntReprMode mode, string expected)
    {
        var config = new ReprConfig(IntMode: mode);
        Assert.Equal(expected, (-42).Repr(config: config));
    }

#if NET7_0_OR_GREATER
    [Theory]
    [InlineData(IntReprMode.Binary,
        "Int128(-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000)")]
    [InlineData(IntReprMode.Decimal, "Int128(-170141183460469231731687303715884105728)")]
    [InlineData(IntReprMode.Hex, "Int128(-0x80000000000000000000000000000000)")]
    [InlineData(IntReprMode.HexBytes, "Int128(0x80000000000000000000000000000000)")]
    public void TestInt128Repr(IntReprMode mode, string expected)
    {
        var i128 = Int128.MinValue;
        var config = new ReprConfig(IntMode: mode);
        Assert.Equal(expected, i128.Repr(config: config));
    }

    [Theory]
    [InlineData(IntReprMode.Binary,
        "Int128(0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111)")]
    [InlineData(IntReprMode.Decimal, "Int128(170141183460469231731687303715884105727)")]
    [InlineData(IntReprMode.Hex, "Int128(0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF)")]
    [InlineData(IntReprMode.HexBytes, "Int128(0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF)")]
    public void TestInt128Repr2(IntReprMode mode, string expected)
    {
        var i128 = Int128.MaxValue;
        var config = new ReprConfig(IntMode: mode);
        Assert.Equal(expected, i128.Repr(config: config));
    }
#endif

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        Assert.Equal("BigInteger(-42)", new BigInteger(-42).Repr(config: config));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
        Assert.Equal("float(3.1415927410125732421875E0)", Single.Parse("3.1415926535")
            .Repr(config));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 5);
        Assert.Equal("double(3.14159)", double.Parse("3.1415926535")
            .Repr(config));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Scientific, FloatPrecision: 5);
        Assert.Equal("Half(3.1406E+000)", Half.Parse("3.14159")
            .Repr(config));
    }

    [Fact]
    public void TestDecimalRepr_RawHex()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.HexBytes);
        Assert.Equal("decimal(0x001C00006582A5360B14388541B65F29)",
            3.1415926535897932384626433832795m.Repr(config));
    }

    [Fact]
    public void TestHalfRepr_BitField()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.BitField);
        Assert.Equal("Half(0|10000|1001001000)", Half.Parse("3.14159")
            .Repr(config));
    }

    // Collections
    [Fact]
    public void TestListRepr()
    {
        Assert.Equal("[]", new List<int>().Repr());
        Assert.Equal("[int(1), int(2), int(3)]", new List<int> { 1, 2, 3 }.Repr());
        Assert.Equal("[\"a\", null, \"c\"]", new List<string?> { "a", null, "c" }.Repr());
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        Assert.Equal("RangeIterator([int(1), int(2), int(3)])", Enumerable.Range(1, 3)
            .Repr());
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        Assert.Equal("[[int(1), int(2)], [int(3), int(4), int(5)], []]", nestedList.Repr());
    }

    // Arrays
    [Fact]
    public void TestArrayRepr()
    {
        Assert.Equal("1DArray([])", Array.Empty<int>()
            .Repr());
        Assert.Equal("1DArray([int(1), int(2), int(3)])", new[] { 1, 2, 3 }.Repr());
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new int[][] { new[] { 1, 2 }, new[] { 3 } };
        Assert.Equal("JaggedArray([[int(1), int(2)], [int(3)]])", jagged2D.Repr());
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new int[,] { { 1, 2 }, { 3, 4 } };
        Assert.Equal("2DArray([[int(1), int(2)], [int(3), int(4)]])", array2D.Repr());
    }

    // Dictionaries, Sets, Queues
    [Fact]
    public void TestDictionaryRepr()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        // Note: Dictionary order is not guaranteed, so we check for both possibilities
        var possibleOutputs = new[]
        {
            "{\"a\": int(1), \"b\": int(2)}",
            "{\"b\": int(2), \"a\": int(1)}"
        };
        Assert.Contains(dict.Repr(), possibleOutputs);
    }

    [Fact]
    public void TestHashSetRepr()
    {
        var set = new HashSet<int> { 1, 2 };
        // Note: HashSet order is not guaranteed, so we sort the string representation for a stable test
        var repr = set.Repr(); // e.g., "{int(1), int(2), int(3)}"
        var possibleOutputs = new[]
        {
            "{int(1), int(2)}",
            "{int(2), int(1)}"
        };
        Assert.Contains(repr, possibleOutputs);
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var repr = set.Repr();
        Assert.Equal("SortedSet({int(1), int(2), int(3)})", repr);
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue("first");
        queue.Enqueue("second");
        Assert.Equal("Queue([\"first\", \"second\"])", queue.Repr());
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(1);
        stack.Push(2);
        Assert.Equal("Stack([int(2), int(1)])", stack.Repr());
    }

    // Custom Types
    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        Assert.Equal("Point(X: int(10), Y: int(20))", point.Repr());
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        Assert.Equal("Custom(test, 42)", custom.Repr());
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person("Alice", 30);
        Assert.Equal("Alice(30)", person.Repr());
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass("data", 123);
        Assert.Equal("NoToStringClass(Data: \"data\", Number: int(123))", noToString.Repr());
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings("Printer",
            new Dictionary<string, double> { ["Temp (C)"] = 200.0, ["PrintSpeed (mm/s)"] = 30.0 });
        // Note: Dictionary order is not guaranteed, so we check for both possibilities
        var possibleOutputs = new[]
        {
            "TestSettings({ EquipmentName: \"Printer\", EquipmentSettings: {\"PrintSpeed (mm/s)\": double(3.0E1), \"Temp (C)\": double(2.0E2)} })",
            "TestSettings({ EquipmentName: \"Printer\", EquipmentSettings: {\"Temp (C)\": double(2.0E2), \"PrintSpeed (mm/s)\": double(3.0E1)} })"
        };
        Assert.Contains(settings.Repr(), possibleOutputs);
    }

    [Fact]
    public void TestEnumRepr()
    {
        Assert.Equal("Colors.GREEN", Colors.GREEN.Repr());
    }

    [Fact]
    public void TestTupleRepr()
    {
        Assert.Equal("(int(1), \"hello\")", (1, "hello").Repr());
    }

    // Nullable Types
    [Fact]
    public void TestNullableStructRepr()
    {
        Assert.Equal("int?(123)", ((int?)123).Repr());
        Assert.Equal("int?(null)", ((int?)null).Repr());
    }

    [Fact]
    public void TestNullableClassRepr()
    {
        Assert.Equal("null", ((List<int>?)null).Repr());
    }

    [Fact]
    public void TestListWithNullElements()
    {
        var listWithNull = new List<List<int>?> { new() { 1 }, null };
        Assert.Equal("[[int(1)], null]", listWithNull.Repr());
    }
}