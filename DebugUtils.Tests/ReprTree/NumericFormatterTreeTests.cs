using System.Numerics;
using System.Text.Json.Nodes;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class NumericFormatterTreeTests
{
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
            [propertyName: "value"] = "3.1415927410125732421875E+000"
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
            [propertyName: "value"] = "3.14062E+000"
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

}