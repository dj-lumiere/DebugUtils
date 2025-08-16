using System.Globalization;
using System.Numerics;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class NumericFormatterTests
{
    // Integer Types
    [Theory]
    [InlineData("B", "0b101010u8")]
    [InlineData("D", "42u8")]
    [InlineData("X", "0x2Au8")]
    public void TestByteRepr(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: ((byte)42).Repr(config: config));
    }

    [Theory]
    [InlineData("B", "-0b101010i32")]
    [InlineData("D", "-42i32")]
    [InlineData("X", "-0x2Ai32")]
    public void TestIntRepr(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: (-42).Repr(config: config));
    }

    [Fact]
    public void TestBigIntRepr()
    {
        var config = new ReprConfig();
        Assert.Equal(expected: "-42n",
            actual: new BigInteger(value: -42).Repr(config: config));
    }

    // Floating Point Types
    [Fact]
    public void TestFloatRepr_Exact()
    {
        var config = new ReprConfig(FloatFormatString: "EX");
        Assert.Equal(expected: "3.1415927410125732421875E+000f32", actual: Single
           .Parse(s: "3.1415926535")
           .Repr(config: config));
    }

    [Fact]
    public void TestDoubleRepr_Round()
    {
        var config = new ReprConfig(FloatFormatString: "F5");
        Assert.Equal(expected: "3.14159f64", actual: Double.Parse(s: "3.1415926535")
                                                                .Repr(config: config));
    }

    [Fact]
    public void TestHalfRepr_Scientific()
    {
        var config = new ReprConfig(FloatFormatString: "E5");
        Assert.Equal(expected: "3.14062E+000f16", actual: Half.Parse(s: "3.14159")
           .Repr(config: config));
    }

    [Fact]
    public void TestDecimalRepr_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        Assert.Equal(expected: "0x6582A536_0B143885_41B65F29p10-028m",
            actual: 3.1415926535897932384626433832795m.Repr(
                config: config));
    }

    [Fact]
    public void TestHalfRepr_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        Assert.Equal(expected: "0x1.920p+001f16", actual: Half.Parse(s: "3.14159")
           .Repr(config: config));
    }

    [Fact]
    public void TestFloatRepr_SpecialValues()
    {
        Assert.Equal(expected: "QuietNaN(0x400000)f32", actual: Single.NaN.Repr());
        Assert.Equal(expected: "Infinityf32", actual: Single.PositiveInfinity.Repr());
        Assert.Equal(expected: "-Infinityf32", actual: Single.NegativeInfinity.Repr());
    }

    [Fact]
    public void TestDoubleRepr_SpecialValues()
    {
        Assert.Equal(expected: "QuietNaN(0x8000000000000)f64", actual: Double.NaN.Repr());
        Assert.Equal(expected: "Infinityf64", actual: Double.PositiveInfinity.Repr());
        Assert.Equal(expected: "-Infinityf64", actual: Double.NegativeInfinity.Repr());
    }

    #if NET5_0_OR_GREATER
    [Fact]
    public void TestHalfRepr_SpecialValues()
    {
        Assert.Equal(expected: "QuietNaN(0x200)f16", actual: Half.NaN.Repr());
        Assert.Equal(expected: "Infinityf16", actual: Half.PositiveInfinity.Repr());
        Assert.Equal(expected: "-Infinityf16", actual: Half.NegativeInfinity.Repr());
    }
    #endif

    #if NET7_0_OR_GREATER
    [Theory]
    [InlineData("B",
        "-0b10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000i128")]
    [InlineData("D", "-170141183460469231731687303715884105728i128")]
    [InlineData("X", "-0x80000000000000000000000000000000i128")]
    public void TestInt128Repr(string format, string expected)
    {
        var i128 = Int128.MinValue;
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected,
            actual: i128.Repr(config: config));
    }

    [Theory]
    [InlineData("B",
        "0b1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111i128")]
    [InlineData("D", "170141183460469231731687303715884105727i128")]
    [InlineData("X", "0x7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFi128")]
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
    [InlineData("D", "42")]
    [InlineData("X", "0x2A")]
    [InlineData("x", "0x2a")]
    [InlineData("N0", "42")]
    [InlineData("B", "0b101010")]
    [InlineData("b", "0b101010")]
    public void TestIntFormatString_PositiveValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected + "i32", actual: 42.Repr(config: config));
    }

    [Theory]
    [InlineData("D", "-42")]
    [InlineData("X", "-0x2A")]
    [InlineData("x", "-0x2a")]
    [InlineData("B", "-0b101010")]
    [InlineData("b", "-0b101010")]
    public void TestIntFormatString_NegativeValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected + "i32", actual: (-42).Repr(config: config));
    }

    [Theory]
    [InlineData("D", "255u8")]
    [InlineData("X", "0xFFu8")]
    [InlineData("B", "0b11111111u8")]
    public void TestIntFormatString_ByteValues(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: ((byte)255).Repr(config: config));
    }

    [Theory]
    [InlineData("D", "9223372036854775807i64")]
    [InlineData("X", "0x7FFFFFFFFFFFFFFFi64")]
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
        Assert.Equal(expected: "42i32", actual: 42.Repr(config: config));
    }

    #endregion

    #region FloatFormatString Tests

    [Theory]
    [InlineData("F2", "3.14f32")]
    [InlineData("E2", "3.14E+000f32")]
    [InlineData("G", "3.14159f32")]
    [InlineData("N2", "3.14f32")]
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
        Assert.Equal(expected: "3.141590118408203125E+000f32", actual: result);
    }

    [Fact]
    public void TestFloatFormatString_HexPower()
    {
        var config = new ReprConfig(FloatFormatString: "HP");
        var result = 3.14159f.Repr(config: config);
        Assert.Equal(expected: "0x1.921FA0p+001f32", actual: result);
    }

    // Removed BitField test as BF format was deprecated

    [Theory]
    [InlineData("F2", "3.14f64")]
    [InlineData("E5", "3.14159E+000f64")]
    [InlineData("EX",
        "3.14158999999999988261834005243144929409027099609375E+000f64")] // EX should produce exact representation
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
        Assert.Equal(expected: "QuietNaN(0x400000)f32", actual: Single.NaN.Repr(config: config));
        Assert.Equal(expected: "Infinityf32",
            actual: Single.PositiveInfinity.Repr(config: config));
        Assert.Equal(expected: "-Infinityf32",
            actual: Single.NegativeInfinity.Repr(config: config));
    }

    [Fact]
    public void TestFloatFormatString_FallbackToString()
    {
        // Empty format string should fall back to toString behavior
        var config = new ReprConfig(FloatFormatString: "");
        var result = 3.14159.Repr(config: config);
        Assert.Equal(expected: "3.14159f64", actual: result);
    }

    #if NET5_0_OR_GREATER
    [Theory]
    [InlineData("F2", "3.14f16")]
    [InlineData("HP", "0x1.920p+001f16")] // HP should produce hex power representation
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
    [InlineData("O", "0o52i32")] // Octal with 0o prefix
    [InlineData("Q", "0q222i32")] // Quaternary with 0q prefix
    public void TestIntFormatString_NewCustomFormats(string format, string expected)
    {
        var config = new ReprConfig(IntFormatString: format);
        Assert.Equal(expected: expected, actual: 42.Repr(config: config));
    }

    [Theory]
    [InlineData("O", "0o377u8")] // Octal 255
    [InlineData("Q", "0q3333u8")] // Quaternary 255
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
        Assert.Equal("0x1.921FA0p+001f32", result); // Should have power notation

        // Test negative number - should have minus sign
        result = (-3.14159f).Repr(config: config);
        Assert.Equal("-0x1.921FA0p+001f32", result);
    }

    [Fact]
    public void TestFloatFormatString_HexPower_SpecialValues()
    {
        var config = new ReprConfig(FloatFormatString: "HP");

        // Special values should still work with HP format
        Assert.Equal(expected: "QuietNaN(0x400000)f32", actual: Single.NaN.Repr(config: config));
        Assert.Equal(expected: "Infinityf32",
            actual: Single.PositiveInfinity.Repr(config: config));
        Assert.Equal(expected: "-Infinityf32",
            actual: Single.NegativeInfinity.Repr(config: config));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TestIntFormatString_WithPrefixes()
    {
        var config = new ReprConfig(IntFormatString: "X4");
        Assert.Equal(expected: "0x002Ai32", actual: 42.Repr(config: config));
    }

    [Fact]
    public void TestIntFormatString_BinaryStartsWith()
    {
        var config = new ReprConfig(IntFormatString: "B8");
        var result = 42.Repr(config: config);
        Assert.Equal(expected: "0b00101010i32",
            actual: result); // Should use C# binary formatter with prefix.
    }

    [Theory]
    [InlineData("X2", "0x2Ai32")]
    [InlineData("x4", "0x002ai32")]
    [InlineData("B4", "0b101010i32")]
    [InlineData("b8", "0b00101010i32")]
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
        Assert.Equal(expected: "3.14f32", actual: 3.14159f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_GermanCulture()
    {
        var config = new ReprConfig(
            FloatFormatString: "F2",
            Culture: new CultureInfo(name: "de-DE")
        );

        // German culture uses comma as decimal separator
        Assert.Equal(expected: "3,14f32", actual: 3.14159f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_NumberFormatting()
    {
        var config = new ReprConfig(
            FloatFormatString: "N2",
            Culture: CultureInfo.InvariantCulture
        );

        // Number format with thousand separators
        Assert.Equal(expected: "1,234.57f32", actual: 1234.5678f.Repr(config: config));
    }

    [Fact]
    public void TestCultureInfo_DefaultBehavior()
    {
        var config = new ReprConfig(
            FloatFormatString: "F2",
            Culture: null // Should use current culture
        );

        // Should not throw and produce some valid output
        var result = 3.14159f.Repr(config: config);
        Assert.Contains(expectedSubstring: "3", actualString: result);
        Assert.Contains(expectedSubstring: "14", actualString: result);
    }

    [Fact]
    public void TestCultureInfo_CustomFormatsIgnoreCulture()
    {
        var config = new ReprConfig(
            FloatFormatString: "EX",
            Culture: new CultureInfo(name: "de-DE")
        );

        // EX format should ignore culture and always use invariant formatting
        var result = 3.14159f.Repr(config: config);
        Assert.Contains(expectedSubstring: ".",
            actualString: result); // Should use period, not comma
        Assert.Contains(expectedSubstring: "E+", actualString: result);
    }

    [Fact]
    public void TestCultureInfo_IntegerFormattingWithCulture()
    {
        var config = new ReprConfig(
            IntFormatString: "N0",
            Culture: CultureInfo.InvariantCulture
        );

        // Integer with thousand separators
        Assert.Equal(expected: "1,234i32", actual: 1234.Repr(config: config));
    }

    #endregion
}