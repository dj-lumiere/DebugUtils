using DebugUtils.Repr;
using DebugUtils.Tests.TestModels;

namespace DebugUtils.Tests;

public class ConfigurationTests
{
    [Fact]
    public void TestReprConfig_MaxDepth()
    {
        var nestedList = new List<object> { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = new ReprConfig(MaxDepth: 1);
        Assert.Equal(expected: "[1_i32, <Max Depth Reached>]",
            actual: nestedList.Repr(config: config));

        config = new ReprConfig(MaxDepth: 0);
        Assert.Equal(expected: "<Max Depth Reached>", actual: nestedList.Repr(config: config));
    }

    [Fact]
    public void TestReprConfig_MaxElementsPerCollection()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var config = new ReprConfig(MaxItemsPerContainer: 3);
        Assert.Equal(expected: "[1_i32, 2_i32, 3_i32, ... (2 more items)]",
            actual: list.Repr(config: config));

        config = new ReprConfig(MaxItemsPerContainer: 0);
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
        var classified =
            new ClassifiedData(writer: "writer", data: "secret", password: "REDACTED");
        var config = new ReprConfig(ViewMode: MemberReprMode.PublicFieldAutoProperty);
        Assert.Equal(
            expected:
            "ClassifiedData(Age: 10_i32, Id: 5_i64, Name: \"Lumi\", Writer: \"writer\")",
            actual: classified.Repr(config: config));

        config = new ReprConfig(ViewMode: MemberReprMode.AllFieldAutoProperty);
        Assert.Equal(
            expected:
            "ClassifiedData(Age: 10_i32, Id: 5_i64, Name: \"Lumi\", Writer: \"writer\", private_Date: DateTime(1970.01.01 00:00:00.0000000 UTC), private_Password: \"REDACTED\", private_Data: \"secret\", private_Key: Guid(9a374b45-3771-4e91-b5e9-64bfa545efe9))",
            actual: classified.Repr(config: config));
    }

    [Fact]
    public void TestReadmeTestRepr()
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
            "Anonymous(Age: 30_i32, Name: \"Alice\", Scores: 1DArray([95_i32, 87_i32, 92_i32]))",
            actual: data.Repr());

        Assert.Equal(expected: "1DArray([1_i32, 2_i32, 3_i32])",
            actual: new[] { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "2DArray([[1_i32, 2_i32], [3_i32, 4_i32]])",
            actual: new[,] { { 1, 2 }, { 3, 4 } }.Repr());
        Assert.Equal(expected: "JaggedArray([[1_i32, 2_i32], [3_i32, 4_i32, 5_i32]])",
            actual: new int[][] { new[] { 1, 2 }, new[] { 3, 4, 5 } }.Repr());

        Assert.Equal(expected: "[1_i32, 2_i32, 3_i32]",
            actual: new List<int> { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "{\"a\", \"b\"}",
            actual: new HashSet<string> { "a", "b" }.Repr());
        Assert.Equal(expected: "{\"x\": 1_i32}",
            actual: new Dictionary<string, int> { { "x", 1 } }.Repr());

        Assert.Equal(expected: "42_i32", actual: 42.Repr());
        Assert.Equal(expected: "0x2A_i32",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "X")));
        Assert.Equal(expected: "0b101010_i32",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "B")));

        Assert.Equal(
            expected: "3.000000000000000444089209850062616169452667236328125E-001_f64",
            actual: (0.1 + 0.2)
           .Repr());
        Assert.Equal(
            expected: "2.99999999999999988897769753748434595763683319091796875E-001_f64",
            actual: 0.3.Repr());
        Assert.Equal(expected: "0.30000000000000004_f64",
            actual: (0.1 + 0.2)
           .Repr(config: new ReprConfig(FloatFormatString: "G")));

        var hideTypes = new ReprConfig(
            TypeMode: TypeReprMode.AlwaysHide,
            UseSimpleFormatsInContainers: false
        );
        Assert.Equal(expected: "[1_i32, 2_i32, 3_i32]", actual: new[] { 1, 2, 3 }.Repr(config: hideTypes));

        var showTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysShow);
        Assert.Equal(expected: "1DArray([1_i32, 2_i32, 3_i32])",
            actual: new[] { 1, 2, 3 }.Repr(config: showTypes));
    }

    [Fact]
    public void TestExampleTestRepr()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Equal(expected: "[1_i32, 2_i32, 3_i32]", actual: list.Repr());

        var config = new ReprConfig(FloatFormatString: "EX");
        var f = 3.14f;
        Assert.Equal(expected: "3.1400001049041748046875E+000_f32",
            actual: f.Repr(config: config));

        int? nullable = 123;
        Assert.Equal(expected: "123_i32?", actual: nullable.Repr());

        var parent = new Children { Name = "Parent" };
        var child = new Children { Name = "Child", Parent = parent };
        parent.Parent = child;
        Assert.StartsWith(
            expectedStartString:
            "Children(Name: \"Parent\", Parent: Children(Name: \"Child\", Parent: <Circular Reference to Children @",
            actualString: parent.Repr());
        Assert.EndsWith(expectedEndString: ">))", actualString: parent.Repr());
    }
}