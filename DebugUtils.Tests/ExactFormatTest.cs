using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class ExactFormatTest
{
    private static readonly ReprConfig OldExactConfig = new();
    private static readonly ReprConfig NewExactConfig = new();

    [Fact]
    public void TestDecimal_Exact_Normal()
    {
        Assert.Contains(expectedSubstring: "1.0E+000",
            actualString: 1.0m.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "-1.0E+000",
            actualString: (-1.0m).Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "3.1415926535897932384626433833E+000",
            actualString: 3.1415926535897932384626433833m.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "1.23456789E+028",
            actualString: 12345678900000000000000000000m.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "1.0E-028",
            actualString: 0.0000000000000000000000000001m.Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestDecimal_ExactBeta_Normal()
    {
        Assert.Contains(expectedSubstring: "1.0E+000",
            actualString: 1.0m.Repr());
        Assert.Contains(expectedSubstring: "-1.0E+000",
            actualString: (-1.0m).Repr());
        Assert.Contains(expectedSubstring: "3.1415926535897932384626433833E+000",
            actualString: 3.1415926535897932384626433833m.Repr());
        Assert.Contains(expectedSubstring: "1.23456789E+028",
            actualString: 12345678900000000000000000000m.Repr());
        Assert.Contains(expectedSubstring: "1.0E-028",
            actualString: 0.0000000000000000000000000001m.Repr());
    }

    [Fact]
    public void TestDecimal_Exact_Zero()
    {
        Assert.Contains(expectedSubstring: "0.0E+000",
            actualString: 0.0m.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "0.0E+000",
            actualString: (-0.0m).Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestDecimal_ExactBeta_Zero()
    {
        Assert.Contains(expectedSubstring: "0.0E+000",
            actualString: 0.0m.Repr());
        Assert.Contains(expectedSubstring: "0.0E+000",
            actualString: (-0.0m).Repr());
    }

    [Fact]
    public void TestDecimal_Exact_MaxMin()
    {
        Assert.Contains(expectedSubstring: "7.9228162514264337593543950335E+028",
            actualString: Decimal.MaxValue.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "-7.9228162514264337593543950335E+028",
            actualString: Decimal.MinValue.Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestDecimal_ExactBeta_MaxMin()
    {
        Assert.Contains(expectedSubstring: "7.9228162514264337593543950335E+028",
            actualString: Decimal.MaxValue.Repr());
        Assert.Contains(expectedSubstring: "-7.9228162514264337593543950335E+028",
            actualString: Decimal.MinValue.Repr());
    }

    [Fact]
    public void TestFloat_Exact_Normal()
    {
        var result0 = 0.0f.Repr(config: OldExactConfig);
        var resultNeg0 = (-0.0f).Repr(config: OldExactConfig);
        var result1 = 1.0f.Repr(config: OldExactConfig);
        var resultNeg1 = (-1.0f).Repr(config: OldExactConfig);
        var result15 = 1.5f.Repr(config: OldExactConfig);
        var result25 = 2.5f.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNeg0);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
        Assert.NotNull(@object: result15);
        Assert.NotNull(@object: result25);
    }

    [Fact]
    public void TestFloat_ExactBeta_Normal()
    {
        var result0 = 0.0f.Repr();
        var resultNeg0 = (-0.0f).Repr();
        var result1 = 1.0f.Repr();
        var resultNeg1 = (-1.0f).Repr();
        var result15 = 1.5f.Repr();
        var result25 = 2.5f.Repr();

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNeg0);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
        Assert.NotNull(@object: result15);
        Assert.NotNull(@object: result25);
    }

    [Fact]
    public void TestFloat_Exact_Subnormal()
    {
        var minValue = 1.401298E-45f; // Smallest positive subnormal float
        var subnormal = 1e-40f; // A subnormal number

        var result1 = minValue.Repr(config: OldExactConfig);
        var result2 = subnormal.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestFloat_ExactBeta_Subnormal()
    {
        var minValue = 1.401298E-45f; // Smallest positive subnormal float
        var subnormal = 1e-40f; // A subnormal number

        var result1 = minValue.Repr();
        var result2 = subnormal.Repr();

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestFloat_Exact_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Single.NaN.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Single.PositiveInfinity.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Single.NegativeInfinity.Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestFloat_ExactBeta_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Single.NaN.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Single.PositiveInfinity.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Single.NegativeInfinity.Repr());
    }

    [Fact]
    public void TestFloat_Exact_MaxMin()
    {
        var maxResult = Single.MaxValue.Repr(config: OldExactConfig);
        var minResult = Single.MinValue.Repr(config: OldExactConfig);
        var epsilonResult = Single.Epsilon.Repr(config: OldExactConfig);

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Fact]
    public void TestFloat_ExactBeta_MaxMin()
    {
        var maxResult = Single.MaxValue.Repr();
        var minResult = Single.MinValue.Repr();
        var epsilonResult = Single.Epsilon.Repr();

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Fact]
    public void TestDouble_Exact_Normal()
    {
        var result0 = 0.0.Repr(config: OldExactConfig);
        var resultNeg0 = (-0.0).Repr(config: OldExactConfig);
        var result1 = 1.0.Repr(config: OldExactConfig);
        var resultNeg1 = (-1.0).Repr(config: OldExactConfig);
        var result15 = 1.5.Repr(config: OldExactConfig);
        var result25 = 2.5.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNeg0);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
        Assert.NotNull(@object: result15);
        Assert.NotNull(@object: result25);
    }

    [Fact]
    public void TestDouble_ExactBeta_Normal()
    {
        var result0 = 0.0.Repr();
        var resultNeg0 = (-0.0).Repr();
        var result1 = 1.0.Repr();
        var resultNeg1 = (-1.0).Repr();
        var result15 = 1.5.Repr();
        var result25 = 2.5.Repr();

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNeg0);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
        Assert.NotNull(@object: result15);
        Assert.NotNull(@object: result25);
    }

    [Fact]
    public void TestDouble_Exact_Subnormal()
    {
        var minValue = 4.9406564584124654E-324; // Smallest positive subnormal double
        var subnormal = 1e-320; // A subnormal number

        var result1 = minValue.Repr(config: OldExactConfig);
        var result2 = subnormal.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestDouble_ExactBeta_Subnormal()
    {
        var minValue = 4.9406564584124654E-324; // Smallest positive subnormal double
        var subnormal = 1e-320; // A subnormal number

        var result1 = minValue.Repr();
        var result2 = subnormal.Repr();

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestDouble_Exact_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Double.NaN.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Double.PositiveInfinity.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Double.NegativeInfinity.Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestDouble_ExactBeta_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Double.NaN.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Double.PositiveInfinity.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Double.NegativeInfinity.Repr());
    }

    [Fact]
    public void TestDouble_Exact_MaxMin()
    {
        var maxResult = Double.MaxValue.Repr(config: OldExactConfig);
        var minResult = Double.MinValue.Repr(config: OldExactConfig);
        var epsilonResult = Double.Epsilon.Repr(config: OldExactConfig);

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Fact]
    public void TestDouble_ExactBeta_MaxMin()
    {
        var maxResult = Double.MaxValue.Repr();
        var minResult = Double.MinValue.Repr();
        var epsilonResult = Double.Epsilon.Repr();

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Fact]
    public void TestHalf_Exact_Normal()
    {
        var result0 = Half.Zero.Repr(config: OldExactConfig);
        var resultNegZero = Half.NegativeZero.Repr(config: OldExactConfig);
        var result1 = ((Half)1.0f).Repr(config: OldExactConfig);
        var resultNeg1 = ((Half)(-1.0f)).Repr(config: OldExactConfig);

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNegZero);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
    }

    [Fact]
    public void TestHalf_ExactBeta_Normal()
    {
        var result0 = Half.Zero.Repr();
        var resultNegZero = Half.NegativeZero.Repr();
        var result1 = ((Half)1.0f).Repr();
        var resultNeg1 = ((Half)(-1.0f)).Repr();

        Assert.NotNull(@object: result0);
        Assert.NotNull(@object: resultNegZero);
        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: resultNeg1);
    }

    [Fact]
    public void TestHalf_Exact_Subnormal()
    {
        var minValue = (Half)5.9604645E-8f; // Smallest positive subnormal Half
        var result = minValue.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Fact]
    public void TestHalf_ExactBeta_Subnormal()
    {
        var minValue = (Half)5.9604645E-8f; // Smallest positive subnormal Half
        var result = minValue.Repr();

        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Fact]
    public void TestHalf_Exact_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Half.NaN.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Half.PositiveInfinity.Repr(config: OldExactConfig));
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Half.NegativeInfinity.Repr(config: OldExactConfig));
    }

    [Fact]
    public void TestHalf_ExactBeta_SpecialValues()
    {
        Assert.Contains(expectedSubstring: "NaN",
            actualString: Half.NaN.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Half.PositiveInfinity.Repr());
        Assert.Contains(expectedSubstring: "Infinity",
            actualString: Half.NegativeInfinity.Repr());
    }

    [Fact]
    public void TestHalf_Exact_MaxMin()
    {
        var maxResult = Half.MaxValue.Repr(config: OldExactConfig);
        var minResult = Half.MinValue.Repr(config: OldExactConfig);
        var epsilonResult = Half.Epsilon.Repr(config: OldExactConfig);

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Fact]
    public void TestHalf_ExactBeta_MaxMin()
    {
        var maxResult = Half.MaxValue.Repr();
        var minResult = Half.MinValue.Repr();
        var epsilonResult = Half.Epsilon.Repr();

        Assert.NotNull(@object: maxResult);
        Assert.NotNull(@object: minResult);
        Assert.NotNull(@object: epsilonResult);
        Assert.NotEmpty(collection: maxResult);
        Assert.NotEmpty(collection: minResult);
        Assert.NotEmpty(collection: epsilonResult);
    }

    [Theory]
    [InlineData(1.0f)]
    [InlineData(2.0f)]
    [InlineData(3.14159f)]
    [InlineData(0.1f)]
    [InlineData(1e-10f)]
    [InlineData(1e20f)]
    public void TestFloat_Exact_Consistency(float value)
    {
        var result = value.Repr(config: OldExactConfig);
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Theory]
    [InlineData(1.0f)]
    [InlineData(2.0f)]
    [InlineData(3.14159f)]
    [InlineData(0.1f)]
    [InlineData(1e-10f)]
    [InlineData(1e20f)]
    public void TestFloat_ExactBeta_Consistency(float value)
    {
        var result = value.Repr();
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.14159265358979)]
    [InlineData(0.1)]
    [InlineData(1e-100)]
    [InlineData(1e100)]
    public void TestDouble_Exact_Consistency(double value)
    {
        var result = value.Repr(config: OldExactConfig);
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.14159265358979)]
    [InlineData(0.1)]
    [InlineData(1e-100)]
    [InlineData(1e100)]
    public void TestDouble_ExactBeta_Consistency(double value)
    {
        var result = value.Repr();
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.141592653589793238462643383279)]
    [InlineData(0.1)]
    [InlineData(0.0000000000000000000000000001)]
    [InlineData(79228162514264328797450928128.0)]
    public void TestDecimal_Exact_Consistency(double doubleValue)
    {
        var decimalValue = (decimal)doubleValue;
        var result = decimalValue.Repr(config: OldExactConfig);
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.141592653589793238462643383279)]
    [InlineData(0.1)]
    [InlineData(0.0000000000000000000000000001)]
    [InlineData(79228162514264328797450928128.0)]
    public void TestDecimal_ExactBeta_Consistency(double doubleValue)
    {
        var decimalValue = (decimal)doubleValue;
        var result = decimalValue.Repr();
        Assert.NotNull(@object: result);
        Assert.NotEmpty(collection: result);
    }

    [Fact]
    public void TestHalf_Exact_WorstCaseScenarios()
    {
        // Worst case bit patterns for Half precision
        var half1 = BitConverter.UInt16BitsToHalf(value: 0x07FF); // Large mantissa, small exponent
        var half2 =
            BitConverter.UInt16BitsToHalf(value: 0x7BFF); // Max normal value with full mantissa

        var result1 = half1.Repr(config: OldExactConfig);
        var result2 = half2.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestHalf_ExactBeta_WorstCaseScenarios()
    {
        // Worst case bit patterns for Half precision
        var half1 = BitConverter.UInt16BitsToHalf(value: 0x07FF); // Large mantissa, small exponent
        var half2 =
            BitConverter.UInt16BitsToHalf(value: 0x7BFF); // Max normal value with full mantissa

        var result1 = half1.Repr();
        var result2 = half2.Repr();

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestFloat_Exact_WorstCaseScenarios()
    {
        // Worst case bit patterns for Single precision
        var float1 =
            BitConverter.UInt32BitsToSingle(value: 0x00FF_FFFF); // Large mantissa, small exponent
        var float2 =
            BitConverter.UInt32BitsToSingle(
                value: 0x7F7F_FFFF); // Max normal value with full mantissa

        var result1 = float1.Repr(config: OldExactConfig);
        var result2 = float2.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestFloat_ExactBeta_WorstCaseScenarios()
    {
        // Worst case bit patterns for Single precision
        var float1 =
            BitConverter.UInt32BitsToSingle(value: 0x00FF_FFFF); // Large mantissa, small exponent
        var float2 =
            BitConverter.UInt32BitsToSingle(
                value: 0x7F7F_FFFF); // Max normal value with full mantissa

        var result1 = float1.Repr();
        var result2 = float2.Repr();

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestDouble_Exact_WorstCaseScenarios()
    {
        // Worst case bit patterns for Double precision
        var double1 =
            BitConverter.UInt64BitsToDouble(
                value: 0x000F_FFFF_FFFF_FFFF); // Large mantissa, small exponent
        var double2 =
            BitConverter.UInt64BitsToDouble(
                value: 0x3FF7_FFFF_FFFF_FFFF); // Max normal value with full mantissa

        var result1 = double1.Repr(config: OldExactConfig);
        var result2 = double2.Repr(config: OldExactConfig);

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }

    [Fact]
    public void TestDouble_ExactBeta_WorstCaseScenarios()
    {
        // Worst case bit patterns for Double precision
        var double1 =
            BitConverter.UInt64BitsToDouble(
                value: 0x000F_FFFF_FFFF_FFFF); // Large mantissa, small exponent
        var double2 =
            BitConverter.UInt64BitsToDouble(
                value: 0x3FF7_FFFF_FFFF_FFFF); // Max normal value with full mantissa

        var result1 = double1.Repr();
        var result2 = double2.Repr();

        Assert.NotNull(@object: result1);
        Assert.NotNull(@object: result2);
        Assert.NotEmpty(collection: result1);
        Assert.NotEmpty(collection: result2);
    }
}