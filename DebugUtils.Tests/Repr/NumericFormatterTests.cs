using System.Globalization;
using System.Numerics;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class NumericFormatterTests
{
    // Integer Types
    [Theory]
    [InlineData("B", "byte(0b101010)")]
    [InlineData("D", "byte(42)")]
    [InlineData("X", "byte(0x2A)")]
    public void TestByteRepr(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: ((byte)42).Repr(config: config));
    }

    [Theory]
    [InlineData("B", "int(-0b101010)")]
    [InlineData("D", "int(-42)")]
    [InlineData("X", "int(-0x2A)")]
    public void TestIntRepr(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: (-42).Repr(config: config));
    }

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "BigInteger(-42)",
            actual: new BigInteger(value: -42).Repr(config: config));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatFormatString: "EX");
        Assert.Equal(expected: "float(3.1415927410125732421875E+000)", actual: Single
           .Parse(s: "3.1415926535")
           .Repr(config: config));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatFormatString: "F5");
        Assert.Equal(expected: "double(3.14159)", actual: Double.Parse(s: "3.1415926535")
                                                                .Repr(config: config));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatFormatString: "E5");
        Assert.Equal(expected: "Half(3.14062E+000)", actual: Half.Parse(s: "3.14159")
           .Repr(config: config));
    }

    [Fact]
    public void TestDecimalRepr_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        Assert.Equal(expected: "decimal(0x6582A5360B14388541B65F29P-028)",
            actual: 3.1415926535897932384626433832795m.Repr(
                config: config));
    }

    [Fact]
    public void TestHalfRepr_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        Assert.Equal(expected: "Half(0x1.920P+001)", actual: Half.Parse(s: "3.14159")
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
    [InlineData("B",
        "Int128(-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000)")]
    [InlineData("D", "Int128(-170141183460469231731687303715884105728)")]
    [InlineData("X", "Int128(-0x80000000000000000000000000000000)")]
    public void TestInt128Repr(string format, string expected)
    {
        var i128 = Int128.MinValue;
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected,
            actual: i128.Repr(config: config));
    }

    [Theory]
    [InlineData("B",
        "Int128(0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111)")]
    [InlineData("D", "Int128(170141183460469231731687303715884105727)")]
    [InlineData("X", "Int128(0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF)")]
    public void TestInt128Repr2(string format, string expected)
    {
        var i128 = Int128.MaxValue;
        var config = new ReprConfig(IntFormatString: format);
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
    [InlineData("B", "int(0b101010)")]
    [InlineData("b", "int(0b101010)")]
    public void TestIntFormatString_PositiveValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: 42.Repr(config: config));
    }

    [Theory]
    [InlineData("D", "int(-42)")]
    [InlineData("X", "int(-0x2A)")]
    [InlineData("x", "int(-0x2a)")]
    [InlineData("B", "int(-0b101010)")]
    [InlineData("b", "int(-0b101010)")]
    public void TestIntFormatString_NegativeValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: (-42).Repr(config: config));
    }

    [Theory]
    [InlineData("D", "byte(255)")]
    [InlineData("X", "byte(0xFF)")]
    [InlineData("B", "byte(0b11111111)")]
    public void TestIntFormatString_ByteValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: ((byte)255).Repr(config: config));
    }

    [Theory]
    [InlineData("D", "long(9223372036854775807)")]
    [InlineData("X", "long(0x7FFFFFFFFFFFFFFF)")]
    public void TestIntFormatString_LongValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: Int64.MaxValue.Repr(config: config));
    }

    [Fact]
    public void TestIntFormatString_DefaultBehavior()
    {
        // Default format string should use decimal
        var config = new ReprConfig();
        Assert.Equal(expected: "int(42)", actual: 42.Repr(config: config));
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
        var actual = 3.14159f.Repr(config: config);
        Assert.Equal(expected: expected, actual: actual);
    }

    [Fact]
    public void TestFloatFormatString_ExactMode()
    {
        var config = new ReprConfig(FloatFormatString: "EX");
        var result = 3.14159f.Repr(config: config);
        Assert.Equal(expected: "float(3.141590118408203125E+000)", actual: result);
    }

    [Fact]
    public void TestFloatFormatString_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        var result = 3.14159f.Repr(config: config);
        Assert.Equal(expected: "float(0x1.921FA0P+001)", actual: result);
    }

    // Removed BitField test as BF format was deprecated

    [Theory]
    [InlineData("F2", "double(3.14)")]
    [InlineData("E5", "double(3.14159E+000)")]
    [InlineData("EX",
        "double(3.14158999999999988261834005243144929409027099609375E+000)")] // EX should produce exact representation
    public void TestFloatFormatString_DoubleValues(string format, object exact)
    {
        var config = new ReprConfig(FloatFormatString: format);
        var result = 3.14159.Repr(config: config);
        Assert.Equal(expected: exact, actual: result);
    }

    [Fact]
    public void TestFloatFormatString_SpecialValues()
    {
        var config = new ReprConfig(FloatFormatString: "F2");
        Assert.Equal(expected: "float(Quiet NaN)", actual: Single.NaN.Repr(config: config));
        Assert.Equal(expected: "float(Infinity)",
            actual: Single.PositiveInfinity.Repr(config: config));
        Assert.Equal(expected: "float(-Infinity)",
            actual: Single.NegativeInfinity.Repr(config: config));
    }

    [Fact]
    public void TestFloatFormatString_FallbackToString()
    {
        // Empty format string should fall back to toString behavior
        var config = new ReprConfig(FloatFormatString: "");
        var result = 3.14159.Repr(config: config);
        Assert.Equal(expected: "double(3.14159)", actual: result);
    }

    #if NET5_0_OR_GREATER
    [Theory]
    [InlineData("F2", "Half(3.14)")]
    [InlineData("HP", "Half(0x1.920P+001)")] // HP should produce hex power representation
    public void TestFloatFormatString_HalfValues(string format, string expectedOrSpecial)
    {
        var config = new ReprConfig(FloatFormatString: format);
        var result = ((Half)3.14159f).Repr(config: config);
        Assert.Equal(expected: expectedOrSpecial, actual: result);
    }
    #endif

    #endregion

    #region New Custom Format Tests

    [Theory]
    [InlineData("O", "int(0o52)")]      // Octal with 0o prefix
    [InlineData("Q", "int(0q222)")]     // Quaternary with 0q prefix
    public void TestIntFormatString_NewCustomFormats(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: 42.Repr(config: config));
    }

    [Theory]
    [InlineData("O", "byte(0o377)")]    // Octal 255
    [InlineData("Q", "byte(0q3333)")]   // Quaternary 255
    public void TestIntFormatString_NewCustomFormats_Byte(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: ((byte)255).Repr(config: config));
    }

    [Fact]
    public void TestFloatFormatString_HexPower_Detailed()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        
        // Test positive number - should use IEEE 754 hex notation
        var result = 3.14159f.Repr(config: config);
        Assert.Contains("0x1.", result);  // Should start with 0x1. for normalized numbers
        Assert.Contains("P+", result);    // Should have power notation
        
        // Test negative number - should have minus sign
        result = (-3.14159f).Repr(config: config);
        Assert.Contains("-0x1.", result);
        Assert.Contains("P+", result);
    }

    [Fact]
    public void TestFloatFormatString_HexPower_SpecialValues()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        
        // Special values should still work with HP format
        Assert.Equal(expected: "float(Quiet NaN)", actual: Single.NaN.Repr(config: config));
        Assert.Equal(expected: "float(Infinity)", actual: Single.PositiveInfinity.Repr(config: config));
        Assert.Equal(expected: "float(-Infinity)", actual: Single.NegativeInfinity.Repr(config: config));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TestIntFormatString_WithPrefixes()
    {
        var config = new ReprConfig(IntFormatString: "X4");
        Assert.Equal(expected: "int(0x002A)", actual: 42.Repr(config: config));
    }

    [Fact]
    public void TestIntFormatString_BinaryStartsWith()
    {
        var config = new ReprConfig(IntFormatString: "B8");
        var result = 42.Repr(config: config);
        Assert.Equal(expected: "int(0b00101010)",
            actual: result); // Should use C# binary formatter with prefix.
    }

    [Theory]
    [InlineData("X2", "int(0x2A)")]
    [InlineData("x4", "int(0x002a)")]
    [InlineData("B4", "int(0b101010)")]
    [InlineData("b8", "int(0b00101010)")]
    public void TestIntFormatString_StartsWithHandling(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        var result = 42.Repr(config: config);
        Assert.Equal(expected: expected,
            actual: result); // Should use C# binary formatter with prefix.
    }

    #endregion

    #region CultureInfo Tests

    [Fact]
    public void TestCultureInfo_InvariantCulture()
    {
        var config = new ReprConfig(
            FloatFormatString: "F2",
            Culture: CultureInfo.InvariantCulture
        );
        
        // Should use period as decimal separator regardless of system culture
        Assert.Equal(expected: "float(3.14)", actual: 3.14159f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_GermanCulture()
    {
        var config = new ReprConfig(
            FloatFormatString: "F2",
            Culture: new CultureInfo("de-DE")
        );
        
        // German culture uses comma as decimal separator
        Assert.Equal(expected: "float(3,14)", actual: 3.14159f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_NumberFormatting()
    {
        var config = new ReprConfig(
            FloatFormatString: "N2",
            Culture: CultureInfo.InvariantCulture
        );
        
        // Number format with thousand separators
        Assert.Equal(expected: "float(1,234.57)", actual: 1234.5678f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_DefaultBehavior()
    {
        var config = new ReprConfig(
            FloatFormatString: "F2",
            Culture: null  // Should use current culture
        );
        
        // Should not throw and produce some valid output
        var result = 3.14159f.Repr(config: config);
        Assert.Contains("3", result);
        Assert.Contains("14", result);
    }

    [Fact]
    public void TestCultureInfo_CustomFormatsIgnoreCulture()
    {
        var config = new ReprConfig(
            FloatFormatString: "EX",
            Culture: new CultureInfo("de-DE")
        );
        
        // EX format should ignore culture and always use invariant formatting
        var result = 3.14159f.Repr(config: config);
        Assert.Contains(".", result);  // Should use period, not comma
        Assert.Contains("E+", result);
    }

    [Fact]
    public void TestCultureInfo_IntegerFormattingWithCulture()
    {
        var config = new ReprConfig(
            IntFormatString: "N0",
            Culture: CultureInfo.InvariantCulture
        );
        
        // Integer with thousand separators
        Assert.Equal(expected: "int(1,234)", actual: 1234.Repr(config: config));
    }

    #endregion
}