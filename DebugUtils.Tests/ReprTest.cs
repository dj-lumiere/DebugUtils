using System.Numerics;
using System.Text;
using DebugUtils.Repr;

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
    public override string ToString()
    {
        return $"{Name}({Age})";
    }
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

public class Children
{
    public required string Name { get; set; }
    public Children? Parent { get; set; }
}

public class ClassifiedData(string writer, string data)
{
    public string Writer { get; set; } = writer;
    private string Data { get; set; } = data;
}

public class ReprTest
{
    [Fact]
    public void ReadmeTestRepr()
    {
        var arr = new int[] { 1, 2, 3, 4 };
        Assert.Equal(expected: "System.Int32[]", actual: arr.ToString());

        var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        Assert.Equal(
            expected: "System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
            actual: dict.ToString());

        var data = new { Name = "Alice", Age = 30, Scores = new[] { 95, 87, 92 } };
        Assert.Equal(
            expected:
            "Anonymous(Name: \"Alice\", Age: int(30), Scores: 1DArray([int(95), int(87), int(92)]))",
            actual: data.Repr());

        Assert.Equal(expected: "1DArray([int(1), int(2), int(3)])",
            actual: new[] { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "2DArray([[int(1), int(2)], [int(3), int(4)]])",
            actual: new[,] { { 1, 2 }, { 3, 4 } }.Repr());
        Assert.Equal(expected: "JaggedArray([[int(1), int(2)], [int(3), int(4), int(5)]])",
            actual: new int[][] { new[] { 1, 2 }, new[] { 3, 4, 5 } }.Repr());

        Assert.Equal(expected: "[int(1), int(2), int(3)]",
            actual: new List<int> { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "{\"a\", \"b\"}",
            actual: new HashSet<string> { "a", "b" }.Repr());
        Assert.Equal(expected: "{\"x\": int(1)}",
            actual: new Dictionary<string, int> { { "x", 1 } }.Repr());

        Assert.Equal(expected: "int(42)", actual: 42.Repr());
        Assert.Equal(expected: "int(0x2A)",
            actual: 42.Repr(config: new ReprConfig(IntMode: IntReprMode.Hex)));
        Assert.Equal(expected: "int(0b101010)",
            actual: 42.Repr(config: new ReprConfig(IntMode: IntReprMode.Binary)));

        Assert.Equal(expected: "double(3.000000000000000444089209850062616169452667236328125E-1)",
            actual: (0.1 + 0.2)
           .Repr());
        Assert.Equal(
            expected: "double(2.99999999999999988897769753748434595763683319091796875E-1)",
            actual: 0.3.Repr());
        Assert.Equal(expected: "double(0.30000000000000004)",
            actual: (0.1 + 0.2)
           .Repr(config: new ReprConfig(FloatMode: FloatReprMode.General)));

        var hideTypes = new ReprConfig(
            TypeMode: TypeReprMode.AlwaysHide,
            ContainerReprMode: ContainerReprMode.UseParentConfig
        );
        Assert.Equal(expected: "[1, 2, 3]", actual: new[] { 1, 2, 3 }.Repr(config: hideTypes));

        var showTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysShow);
        Assert.Equal(expected: "1DArray([int(1), int(2), int(3)])",
            actual: new[] { 1, 2, 3 }.Repr(config: showTypes));
    }

    [Fact]
    public void ExampleTestRepr()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Equal(expected: "[int(1), int(2), int(3)]", actual: list.Repr());

        var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
        var f = 3.14f;
        Assert.Equal(expected: "float(3.1400001049041748046875E0)",
            actual: f.Repr(config: config));

        int? nullable = 123;
        Assert.Equal(expected: "int?(123)", actual: nullable.Repr());

        var parent = new Children { Name = "Parent" };
        var child = new Children { Name = "Child", Parent = parent };
        parent.Parent = child;
        Assert.StartsWith(
            expectedStartString:
            "Children(Name: \"Parent\", Parent: Children(Name: \"Child\", Parent: <Circular Reference to Children @",
            actualString: parent.Repr());
        Assert.EndsWith(expectedEndString: ">))", actualString: parent.Repr());
    }

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
        Assert.Equal(expected: "DateTime(2025-01-01 00:00:00 Unspecified)", actual:
            dateTime.Repr());
        Assert.Equal(expected: "DateTime(2025-01-01 00:00:00 Local)", actual:
            localDateTime.Repr());
        Assert.Equal(expected: "DateTime(2025-01-01 00:00:00 UTC)", actual:
            utcDateTime.Repr());
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
            actual: 3.1415926535897932384626433832795m.Repr(
                config: config));
    }

    [Fact]
    public void TestHalfRepr_BitField()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.BitField);
        Assert.Equal(expected: "Half(0|10000|1001001000)", actual: Half.Parse(s: "3.14159")
           .Repr(config: config));
    }

    [Fact]
    public void TestFloatRepr_SpecialValues()
    {
        Assert.Equal(expected: "float(Quiet NaN)", actual: Single.NaN.Repr());
        Assert.Equal(expected: "float(Infinity)", actual: Single.PositiveInfinity.Repr());
        Assert.Equal(expected: "float(-Infinity)", actual: Single.NegativeInfinity.Repr());
    }

    [Fact]
    public void TestDoubleRepr_SpecialValues()
    {
        Assert.Equal(expected: "double(Quiet NaN)", actual: Double.NaN.Repr());
        Assert.Equal(expected: "double(Infinity)", actual: Double.PositiveInfinity.Repr());
        Assert.Equal(expected: "double(-Infinity)", actual: Double.NegativeInfinity.Repr());
    }

    #if NET5_0_OR_GREATER
    [Fact]
    public void TestHalfRepr_SpecialValues()
    {
        Assert.Equal(expected: "Half(Quiet NaN)", actual: Half.NaN.Repr());
        Assert.Equal(expected: "Half(Infinity)", actual: Half.PositiveInfinity.Repr());
        Assert.Equal(expected: "Half(-Infinity)", actual: Half.NegativeInfinity.Repr());
    }
    #endif

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

    [Fact]
    public void TestPriorityQueueRepr()
    {
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue(element: "second", priority: 2);
        pq.Enqueue(element: "first", priority: 1);
        pq.Enqueue(element: "third", priority: 3);

        var repr = pq.Repr();
        Assert.Contains(expectedSubstring: "\"first\" (priority: int(1))", actualString: repr);
        Assert.Contains(expectedSubstring: "\"second\" (priority: int(2))", actualString: repr);
        Assert.Contains(expectedSubstring: "\"third\" (priority: int(3))", actualString: repr);
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
        Assert.Equal(expected: "Colors.GREEN (int(1))", actual: Colors.GREEN.Repr());
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
        Assert.Equal(expected: "[[int(1)], null]", actual: listWithNull.Repr());
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
        Assert.Equal(expected: expected,
            actual: i128.Repr(config: config));
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
        Assert.Equal(expected: expected,
            actual: i128.Repr(config: config));
    }
    #endif

    [Fact]
    public void TestGuidRepr()
    {
        var guid = Guid.NewGuid();
        Assert.Equal(expected: $"Guid({guid})", actual: guid.Repr());
    }

    [Fact]
    public void TestTimeSpanRepr_Negative()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        Assert.Equal(expected: "TimeSpan(-1800.000s)", actual: TimeSpan.FromMinutes(minutes: -30)
           .Repr(config: config));
    }

    [Fact]
    public void TestTimeSpanRepr_Zero()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        Assert.Equal(expected: "TimeSpan(0.000s)",
            actual: TimeSpan.Zero.Repr(config: config));
    }

    [Fact]
    public void TestTimeSpanRepr_Positive()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Decimal);
        Assert.Equal(expected: "TimeSpan(1800.000s)", actual: TimeSpan.FromMinutes(minutes: 30)
           .Repr(config: config));
    }

    [Fact]
    public void TestDateTimeOffsetRepr()
    {
        Assert.Equal(expected: "DateTimeOffset(2025-01-01 00:00:00Z)",
            actual: new DateTimeOffset(dateTime: new DateTime(
                date: new DateOnly(year: 2025, month: 1, day: 1),
                time: new TimeOnly(hour: 0, minute: 0, second: 0),
                kind: DateTimeKind.Utc)).Repr());
    }

    [Fact]
    public void TestDateTimeOffsetRepr_WithOffset()
    {
        Assert.Equal(expected: "DateTimeOffset(2025-01-01 00:00:00+01:00:00)",
            actual: new DateTimeOffset(dateTime: new DateTime(year: 2025, month: 1, day: 1),
                offset: TimeSpan.FromHours(hours: 1)).Repr());
    }

    [Fact]
    public void TestDateOnly()
    {
        Assert.Equal(expected: "DateOnly(2025-01-01)",
            actual: new DateOnly(year: 2025, month: 1, day: 1).Repr());
    }

    [Fact]
    public void TestTimeOnly()
    {
        Assert.Equal(expected: "TimeOnly(00:00:00)",
            actual: new TimeOnly(hour: 0, minute: 0, second: 0).Repr());
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
    public void TestFunction()
    {
        var Add5 = (int a) => a + 1;
        var a = Add;
        var b = Add2;
        var c = Add3<short>;
        var d = Add4;
        var e = Lambda;

        Assert.Equal(expected: "internal int Lambda(int a)", actual: Add5.Repr());
        Assert.Equal(expected: "public static int Add(int a, int b)", actual: a.Repr());
        Assert.Equal(expected: "internal static long Add2(int a)", actual: b.Repr());
        Assert.Equal(expected: "private generic short Add3(short a)", actual: c.Repr());
        Assert.Equal(expected: "private static void Add4(in ref int a, out ref int b)",
            actual: d.Repr());
        Assert.Equal(expected: "private async Task<int> Lambda(int a)", actual: e.Repr());
    }

    [Fact]
    public void TestCircularReference()
    {
        var a = new List<object>();
        a.Add(item: a);
        var repr = a.Repr();
        // object hash code can be different.
        Assert.StartsWith(expectedStartString: "[<Circular Reference to List @",
            actualString: repr);
        Assert.EndsWith(expectedEndString: ">]", actualString: repr);
    }

    [Fact]
    public void TestReprConfig_MaxDepth()
    {
        var nestedList = new List<object> { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = new ReprConfig(MaxDepth: 1);
        Assert.Equal(expected: "[int(1), <Max Depth Reached>]",
            actual: nestedList.Repr(config: config));

        config = new ReprConfig(MaxDepth: 0);
        Assert.Equal(expected: "<Max Depth Reached>", actual: nestedList.Repr(config: config));
    }

    [Fact]
    public void TestReprConfig_MaxElementsPerCollection()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var config = new ReprConfig(MaxElementsPerCollection: 3);
        Assert.Equal(expected: "[int(1), int(2), int(3), ... (2 more items)]",
            actual: list.Repr(config: config));

        config = new ReprConfig(MaxElementsPerCollection: 0);
        Assert.Equal(expected: "[... (5 more items)]", actual: list.Repr(config: config));
    }

    [Fact]
    public void TestReprConfig_MaxStringLength()
    {
        var longString = "This is a very long string that should be truncated.";
        var config = new ReprConfig(MaxStringLength: 10);
        Assert.Equal(expected: "\"This is a ... (42 more letters)\"",
            actual: longString.Repr(config: config));

        config = new ReprConfig(MaxStringLength: 0);
        Assert.Equal(expected: "\"... (52 more letters)\"",
            actual: longString.Repr(config: config));
    }

    [Fact]
    public void TestReprConfig_ShowNonPublicProperties()
    {
        var classified = new ClassifiedData(writer: "writer", data: "secret");
        var config = new ReprConfig(ShowNonPublicProperties: false);
        Assert.Equal(expected: "ClassifiedData(Writer: \"writer\")",
            actual: classified.Repr(config: config));

        config = new ReprConfig(ShowNonPublicProperties: true);
        Assert.Equal(expected: "ClassifiedData(Writer: \"writer\", private_Data: \"secret\")",
            actual: classified.Repr(config: config));
    }
}