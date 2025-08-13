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

    // NEW: Format String Tests for v1.5

    #region IntFormatString Tests

    [Theory]
    [InlineData("D", "int(42)")]
    [InlineData("X", "int(0x2A)")]
    [InlineData("x", "int(0x2a)")]
    [InlineData("N0", "int(42)")]
    [InlineData("HB", "int(0x0000002A)")]
    [InlineData("B", "int(0b101010)")]
    [InlineData("b", "int(0b101010)")]
    public void TestIntFormatString_PositiveValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected, 42.Repr(config));
    }

    [Theory]
    [InlineData("D", "int(-42)")]
    [InlineData("X", "int(-0x2A)")]
    [InlineData("x", "int(-0x2a)")]
    [InlineData("HB", "int(0xFFFFFFD6)")]
    [InlineData("B", "int(-0b101010)")]
    [InlineData("b", "int(-0b101010)")]
    public void TestIntFormatString_NegativeValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected, (-42).Repr(config));
    }

    [Theory]
    [InlineData("D", "byte(255)")]
    [InlineData("X", "byte(0xFF)")]
    [InlineData("HB", "byte(0xFF)")]
    [InlineData("B", "byte(0b11111111)")]
    public void TestIntFormatString_ByteValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected, ((byte)255).Repr(config));
    }

    [Theory]
    [InlineData("D", "long(9223372036854775807)")]
    [InlineData("X", "long(0x7FFFFFFFFFFFFFFF)")]
    [InlineData("HB", "long(0x7FFFFFFFFFFFFFFF)")]
    public void TestIntFormatString_LongValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected, long.MaxValue.Repr(config));
    }

    [Fact]
    public void TestIntFormatString_FallbackToEnum()
    {
        // Empty format string should fall back to enum behavior
        var config = new ReprConfig(IntFormatString: "", IntMode: IntReprMode.Hex);
        Assert.Equal("int(0x2A)", 42.Repr(config));
    }

    #endregion

    #region FloatFormatString Tests

    [Theory]
    [InlineData("F2", "float(3.14)")]
    [InlineData("E2", "float(3.14E+000)")]
    [InlineData("G", "float(3.14159)")]
    [InlineData("N2", "float(3.14)")]
    public void TestFloatFormatString_StandardFormats(string format, string expected)
    {
        var config = new ReprConfig(FloatFormatString: format);
        var actual = 3.14159f.Repr(config);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestFloatFormatString_ExactMode()
    {
        var config = new ReprConfig(FloatFormatString: "EX");
        var result = 3.1415926535897931f.Repr(config);
        Assert.StartsWith("float(3.1415927410125732421875E+000)", result);
    }

    [Fact]
    public void TestFloatFormatString_HexBytes()
    {
        var config = new ReprConfig(FloatFormatString: "HB");
        var result = 3.14159f.Repr(config);
        Assert.Equal("float(0x40490FD0)", result);
    }

    [Fact]
    public void TestFloatFormatString_BitField()
    {
        var config = new ReprConfig(FloatFormatString: "BF");
        var result = 3.14159f.Repr(config);
        Assert.Contains("|", result); // Should contain bit field separators
        Assert.StartsWith("float(0|", result); // Positive number starts with 0
    }

    [Theory]
    [InlineData("F2", "double(3.14)")]
    [InlineData("E5", "double(3.14159E+000)")]
    [InlineData("EX", true)] // EX should produce exact representation
    public void TestFloatFormatString_DoubleValues(string format, object expectedOrExact)
    {
        var config = new ReprConfig(FloatFormatString: format);
        var result = 3.1415926535897931.Repr(config);

        if (expectedOrExact is bool)
        {
            Assert.StartsWith("double(3.1415926535897931", result);
        }
        else
        {
            Assert.Equal((string)expectedOrExact, result);
        }
    }

    [Fact]
    public void TestFloatFormatString_SpecialValues()
    {
        var config = new ReprConfig(FloatFormatString: "F2");
        Assert.Equal("float(Quiet NaN)", float.NaN.Repr(config));
        Assert.Equal("float(Infinity)", float.PositiveInfinity.Repr(config));
        Assert.Equal("float(-Infinity)", float.NegativeInfinity.Repr(config));
    }

    [Fact]
    public void TestFloatFormatString_FallbackToEnum()
    {
        // Empty format string should fall back to enum behavior
        var config = new ReprConfig(FloatFormatString: "", FloatMode: FloatReprMode.Round,
            FloatPrecision: 2);
        var result = 3.14159.Repr(config);
        Assert.Equal("double(3.14)", result);
    }

    #if NET5_0_OR_GREATER
    [Theory]
    [InlineData("F2", "Half(3.14)")]
    [InlineData("HB", true)] // HB should produce hex representation
    [InlineData("BF", true)] // BF should produce bit field
    public void TestFloatFormatString_HalfValues(string format, object expectedOrSpecial)
    {
        var config = new ReprConfig(FloatFormatString: format);
        var result = ((Half)3.14159f).Repr(config);

        if (expectedOrSpecial is bool)
        {
            Assert.StartsWith("Half(", result);
            if (format == "HB") Assert.Contains("0x", result);
            if (format == "BF") Assert.Contains("|", result);
        }
        else
        {
            Assert.Equal((string)expectedOrSpecial, result);
        }
    }
    #endif

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public void TestBackwardCompatibility_IntModeStillWorks()
    {
        var config = new ReprConfig(IntMode: IntReprMode.Hex);
        Assert.Equal("int(0x2A)", 42.Repr(config));
    }

    [Fact]
    public void TestBackwardCompatibility_FloatModeStillWorks()
    {
        var config = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 2);
        Assert.Equal("float(3.14)", 3.14159f.Repr(config));
    }

    [Fact]
    public void TestFormatStringTakesPrecedenceOverEnum()
    {
        // Format string should take precedence over enum when both are specified
        var config = new ReprConfig(
            IntFormatString: "X",
            IntMode: IntReprMode.Binary // This should be ignored
        );
        Assert.Equal("int(0x2A)", 42.Repr(config));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TestIntFormatString_WithPrefixes()
    {
        var config = new ReprConfig(IntFormatString: "X4");
        Assert.Equal("int(0x002A)", 42.Repr(config));
    }

    [Fact]
    public void TestIntFormatString_BinaryStartsWith()
    {
        var config = new ReprConfig(IntFormatString: "B8");
        var result = 42.Repr(config);
        Assert.Equal("int(0b00101010)", result); // Should still use custom binary formatter
    }

    [Theory]
    [InlineData("X2")]
    [InlineData("x4")]
    [InlineData("B4")]
    [InlineData("b8")]
    public void TestIntFormatString_StartsWithHandling(string format)
    {
        var config = new ReprConfig(IntFormatString: format);
        var result = 42.Repr(config);
        Assert.NotNull(result);
        Assert.StartsWith("int(", result);
    }

    #endregion
}