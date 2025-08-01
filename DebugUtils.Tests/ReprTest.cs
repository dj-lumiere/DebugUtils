using System.Numerics;
using System.Text;
using DebugUtils.Repr;
using DebugUtils.Repr.Records;

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
    public override string ToString()
    {
        return $"Custom({Name}, {Value})";
    }
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

public record TestSettings(string EquipmentName, Dictionary<string, double> EquipmentSettings);

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
        Assert.Equal("DateTime(2025-01-01 00:00:00)", DateTime.Parse("2025-01-01")
            .Repr());
    }

    [Fact]
    public void TestTimeSpanRepr()
    {
        Assert.Equal(expected: "TimeSpan(1800.000s)", actual: TimeSpan.FromMinutes(minutes: 30)
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
        Assert.Equal(expected: expected, actual: ((byte)42).Repr(config: config));
    }

    [Theory]
    [InlineData(IntReprMode.Binary, "int(-0b101010)")]
    [InlineData(IntReprMode.Decimal, "int(-42)")]
    [InlineData(IntReprMode.Hex, "int(-0x2A)")]
    [InlineData(IntReprMode.HexBytes, "int(0xFFFFFFD6)")]
    public void TestIntRepr(IntReprMode mode, string expected)
    {
        var config = new ReprConfig(IntMode: mode);
        Assert.Equal(expected: expected, actual: (-42).Repr(config: config));
    }

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        Assert.Equal(expected: "BigInteger(-42)",
            actual: new BigInteger(value: -42).Repr(config: config));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
        Assert.Equal(expected: "float(3.1415927410125732421875E0)", actual: Single
           .Parse(s: "3.1415926535")
           .Repr(config: config));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 5);
        Assert.Equal(expected: "double(3.14159)", actual: Double.Parse(s: "3.1415926535")
                                                                .Repr(config: config));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Scientific, FloatPrecision: 5);
        Assert.Equal(expected: "Half(3.1406E+000)", actual: Half.Parse(s: "3.14159")
                                                                .Repr(config: config));
    }

    [Fact]
    public void TestDecimalRepr_RawHex()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.HexBytes);
        Assert.Equal(expected: "decimal(0x001C00006582A5360B14388541B65F29)",
            actual: 3.1415926535897932384626433832795m.Repr(config: config));
    }

    [Fact]
    public void TestHalfRepr_BitField()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.BitField);
        Assert.Equal(expected: "Half(0|10000|1001001000)", actual: Half.Parse(s: "3.14159")
           .Repr(config: config));
    }

    // Collections
    [Fact]
    public void TestListRepr()
    {
        Assert.Equal(expected: "[]", actual: new List<int>().Repr());
        Assert.Equal(expected: "[int(1), int(2), int(3)]",
            actual: new List<int> { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "[\"a\", null, \"c\"]",
            actual: new List<string?> { "a", null, "c" }.Repr());
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        Assert.Equal(expected: "RangeIterator([int(1), int(2), int(3)])", actual: Enumerable
           .Range(start: 1, count: 3)
           .Repr());
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        Assert.Equal(expected: "[[int(1), int(2)], [int(3), int(4), int(5)], []]",
            actual: nestedList.Repr());
    }

    // Arrays
    [Fact]
    public void TestArrayRepr()
    {
        Assert.Equal(expected: "1DArray([])", actual: Array.Empty<int>()
                                                           .Repr());
        Assert.Equal(expected: "1DArray([int(1), int(2), int(3)])",
            actual: new[] { 1, 2, 3 }.Repr());
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new[]
            { new[] { 1, 2 }, new[] { 3 } };
        Assert.Equal(expected: "JaggedArray([[int(1), int(2)], [int(3)]])",
            actual: jagged2D.Repr());
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new[,] { { 1, 2 }, { 3, 4 } };
        Assert.Equal(expected: "2DArray([[int(1), int(2)], [int(3), int(4)]])",
            actual: array2D.Repr());
    }

    // Dictionaries, Sets, Queues
    [Fact]
    public void TestDictionaryRepr()
    {
        var dict = new Dictionary<string, int> { [key: "a"] = 1, [key: "b"] = 2 };
        // Note: Dictionary order is not guaranteed, so we check for both possibilities
        var possibleOutputs = new[]
        {
            "{\"a\": int(1), \"b\": int(2)}",
            "{\"b\": int(2), \"a\": int(1)}"
        };
        Assert.Contains(expected: dict.Repr(), collection: possibleOutputs);
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
        Assert.Contains(expected: repr, collection: possibleOutputs);
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var repr = set.Repr();
        Assert.Equal(expected: "SortedSet({int(1), int(2), int(3)})", actual: repr);
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue(item: "first");
        queue.Enqueue(item: "second");
        Assert.Equal(expected: "Queue([\"first\", \"second\"])", actual: queue.Repr());
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(item: 1);
        stack.Push(item: 2);
        Assert.Equal(expected: "Stack([int(2), int(1)])", actual: stack.Repr());
    }

    // Custom Types
    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        Assert.Equal(expected: "Point(X: int(10), Y: int(20))", actual: point.Repr());
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        Assert.Equal(expected: "Custom(test, 42)", actual: custom.Repr());
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        Assert.Equal(expected: "Alice(30)", actual: person.Repr());
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        Assert.Equal(expected: "NoToStringClass(Data: \"data\", Number: int(123))",
            actual: noToString.Repr());
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings(EquipmentName: "Printer",
            EquipmentSettings: new Dictionary<string, double>
                { [key: "Temp (C)"] = 200.0, [key: "PrintSpeed (mm/s)"] = 30.0 });
        // Note: Dictionary order is not guaranteed, so we check for both possibilities
        var possibleOutputs = new[]
        {
            "TestSettings({ EquipmentName: \"Printer\", EquipmentSettings: {\"PrintSpeed (mm/s)\": double(3.0E1), \"Temp (C)\": double(2.0E2)} })",
            "TestSettings({ EquipmentName: \"Printer\", EquipmentSettings: {\"Temp (C)\": double(2.0E2), \"PrintSpeed (mm/s)\": double(3.0E1)} })"
        };
        Assert.Contains(expected: settings.Repr(), collection: possibleOutputs);
    }

    [Fact]
    public void TestEnumRepr()
    {
        Assert.Equal(expected: "Colors.GREEN", actual: Colors.GREEN.Repr());
    }

    [Fact]
    public void TestTupleRepr()
    {
        Assert.Equal(expected: "(int(1), \"hello\")", actual: (1, "hello").Repr());
    }

    // Nullable Types
    [Fact]
    public void TestNullableStructRepr()
    {
        Assert.Equal(expected: "int?(123)", actual: ((int?)123).Repr());
        Assert.Equal(expected: "int?(null)", actual: ((int?)null).Repr());
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
        Assert.Equal("[[int(1)], null]", listWithNull.Repr());
        Assert.Equal(expected: "[[int(1)], null]", actual: listWithNull.Repr());
    }
    }
}