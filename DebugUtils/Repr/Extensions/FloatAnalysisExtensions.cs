using DebugUtils.Repr.Models;

namespace DebugUtils.Repr.Extensions;

/// <summary>
/// IEEE 754 floating point analysis extensions.
/// 
/// CORE FUNCTIONALITY: Extract IEEE 754 binary representation components and convert
/// to normalized form for exact decimal representation.
/// 
/// IEEE 754 FORMAT: [Sign][Exponent][Mantissa]
/// - Normal numbers: value = (-1)^sign * (1.mantissa) * 2^(exponent - bias)
/// - Subnormal numbers: value = (-1)^sign * (0.mantissa) * 2^(1 - bias)
/// - Special cases: Infinity (exp = all 1s, mantissa = 0), NaN (exp = all 1s, mantissa ≠ 0)
/// 
/// OUTPUT: FloatInfo with RealExponent and Significand for exact decimal conversion
/// </summary>
internal static class FloatAnalysisExtensions
{
    #if NET5_0_OR_GREATER
    // IEEE 754 binary16 (Half): 1 sign + 5 exponent + 10 mantissa = 16 bits
    private static readonly FloatSpec halfSpec = new(ExpBitSize: 5, MantissaBitSize: 10,
        TotalSize: 16, MantissaMask: 0x3FF, MantissaMsbMask: 0x200, ExpMask: 0x1F, ExpOffset: 15);
    #endif
    // IEEE 754 binary32 (Float): 1 sign + 8 exponent + 23 mantissa = 32 bits  
    private static readonly FloatSpec floatSpec = new(ExpBitSize: 8, MantissaBitSize: 23,
        TotalSize: 32, MantissaMask: 0x7FFFFF, MantissaMsbMask: 0x400000, ExpMask: 0xFF,
        ExpOffset: 127);

    // IEEE 754 binary64 (Double): 1 sign + 11 exponent + 52 mantissa = 64 bits
    private static readonly FloatSpec doubleSpec =
        new(ExpBitSize: 11, MantissaBitSize: 52, TotalSize: 64, MantissaMask: 0xFFFFFFFFFFFFFL,
            MantissaMsbMask: 0x8000000000000L, ExpMask: 0x7FFL, ExpOffset: 1023);

    #if NET5_0_OR_GREATER
    public static FloatInfo AnalyzeHalf(this Half value)
    {
        var bits = BitConverter.HalfToInt16Bits(value: value);
        var rawExponent = (int)(bits >> halfSpec.MantissaBitSize & halfSpec.ExpMask);
        var mantissa = bits & halfSpec.MantissaMask;

        return new FloatInfo(
            Spec: halfSpec,
            Bits: bits,
            // IEEE 754 REAL EXPONENT CALCULATION:
            // Normal numbers: realExp = rawExp - bias - mantissaBits  
            // Subnormal numbers: realExp = 1 - bias - mantissaBits (special case when rawExp = 0)
            // REASON: Subnormals have implicit leading 0, not 1, and use minimum exponent
            RealExponent: rawExponent - halfSpec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - halfSpec.MantissaBitSize,
            // IEEE 754 SIGNIFICAND CONSTRUCTION:
            // Normal: (2^mantissaBits + mantissa) - adds implicit leading 1
            // Subnormal: mantissa - no implicit leading 1 (rawExponent == 0)
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << halfSpec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Half
        );
    }
    #endif
    public static FloatInfo AnalyzeFloat(this float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value: value);
        var rawExponent = (int)(bits >> floatSpec.MantissaBitSize & floatSpec.ExpMask);
        var mantissa = bits & floatSpec.MantissaMask;

        return new FloatInfo(
            Spec: floatSpec,
            Bits: bits,
            RealExponent: rawExponent - floatSpec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - floatSpec.MantissaBitSize,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << floatSpec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Float
        );
    }
    public static FloatInfo AnalyzeDouble(this double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value: value);
        var rawExponent = (int)(bits >> doubleSpec.MantissaBitSize & doubleSpec.ExpMask);
        var mantissa = bits & doubleSpec.MantissaMask;

        return new FloatInfo(
            Spec: doubleSpec,
            Bits: bits,
            RealExponent: rawExponent - doubleSpec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - doubleSpec.MantissaBitSize,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1L << doubleSpec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Double
        );
    }
}