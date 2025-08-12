using System.Numerics;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class NumericFormatterTests
{
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
        Assert.Equal(expected: "float(3.1415927410125732421875E+000)", actual: Single
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
        Assert.Equal(expected: "Half(3.14062E+000)", actual: Half.Parse(s: "3.14159")
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
}