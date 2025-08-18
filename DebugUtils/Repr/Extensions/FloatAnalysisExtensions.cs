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
    // IEEE 754 binary16 (Half): 1 sign bit + 5 exponent bits + 10 mantissa bits = 16 bits
    private static readonly FloatSpec F16Spec = new(ExpBitSize: 5, MantissaBitSize: 10,
        MantissaMask: 0x3FF, MantissaMsbMask: 0x200, ExpMask: 0x1F, ExpOffset: 15);
    #endif
    // IEEE 754 binary32 (Float): 1 sign bit + 8 exponent bits + 23 mantissa bits = 32 bits  
    private static readonly FloatSpec F32Spec = new(ExpBitSize: 8, MantissaBitSize: 23,
        MantissaMask: 0x7FFFFF, MantissaMsbMask: 0x400000, ExpMask: 0xFF,
        ExpOffset: 127);

    // IEEE 754 binary64 (Double): 1 sign bit + 11 exponent bits + 52 mantissa bits = 64 bits
    private static readonly FloatSpec F64Spec =
        new(ExpBitSize: 11, MantissaBitSize: 52, MantissaMask: 0xFFFFFFFFFFFFFL,
            MantissaMsbMask: 0x8000000000000L, ExpMask: 0x7FFL, ExpOffset: 1023);

    
    // IEEE 754 REAL EXPONENT CALCULATION:
    // Normal numbers: realExp = rawExp - bias - mantissaBits
    // Subnormal numbers: realExp = 1 - bias - mantissaBits (special case when rawExp = 0)
    // REASON: Subnormal numbers have implicit leading 0, not 1, and use minimum exponent
    
    #if NET5_0_OR_GREATER
    public static FloatInfo AnalyzeHalf(this Half value)
    {
        var bits = BitConverter.HalfToInt16Bits(value: value);
        var rawExponent = (int)(bits >> F16Spec.MantissaBitSize & F16Spec.ExpMask);
        var mantissa = bits & F16Spec.MantissaMask;

        return new FloatInfo(
            Spec: F16Spec,
            Bits: bits,
            RealExponent: rawExponent - F16Spec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - F16Spec.MantissaBitSize,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << F16Spec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Half
        );
    }
    #endif
    public static FloatInfo AnalyzeFloat(this float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value: value);
        var rawExponent = (int)(bits >> F32Spec.MantissaBitSize & F32Spec.ExpMask);
        var mantissa = bits & F32Spec.MantissaMask;

        return new FloatInfo(
            Spec: F32Spec,
            Bits: bits,
            RealExponent: rawExponent - F32Spec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - F32Spec.MantissaBitSize,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << F32Spec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Float
        );
    }
    public static FloatInfo AnalyzeDouble(this double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value: value);
        var rawExponent = (int)(bits >> F64Spec.MantissaBitSize & F64Spec.ExpMask);
        var mantissa = bits & F64Spec.MantissaMask;

        return new FloatInfo(
            Spec: F64Spec,
            Bits: bits,
            RealExponent: rawExponent - F64Spec.ExpOffset + (rawExponent == 0
                ? 1
                : 0) - F64Spec.MantissaBitSize,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1L << F64Spec.MantissaBitSize) + mantissa),
            TypeName: FloatTypeKind.Double
        );
    }
}