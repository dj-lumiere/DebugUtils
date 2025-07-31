using DebugUtils.Records;

namespace DebugUtils.Formatters;

internal static class FloatAnalyzers
{
#if NET5_0_OR_GREATER
    private static FloatSpec halfSpec = new(5, 10, 16, 0x3FF, 0x3FF, 0x1F, 15);
#endif
    private static FloatSpec floatSpec = new(8, 23, 32, 0x7FFFFF, 0x7FFFFF, 0xFF, 127);

    private static FloatSpec doubleSpec =
        new(11, 52, 64, 0xFFFFFFFFFFFFFL, 0x8000000000000L, 0x7FFL, 1023);

#if NET5_0_OR_GREATER
    public static FloatInfo AnalyzeHalf(this Half value)
    {
        var bits = BitConverter.HalfToInt16Bits(value);
        var rawExponent = (int)(bits >> halfSpec.MantissaBitSize & halfSpec.ExpMask);
        var mantissa = bits & halfSpec.MantissaMask;

        return new FloatInfo(
            Spec: halfSpec,
            Bits: bits,
            IsNegative: bits < 0,
            IsPositiveInfinity: Half.IsPositiveInfinity(value),
            IsNegativeInfinity: Half.IsNegativeInfinity(value),
            IsQuietNaN: Half.IsNaN(value) && (bits & halfSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: Half.IsNaN(value) && (bits & halfSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - halfSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << halfSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(rawExponent, 2)
                .PadLeft(halfSpec.ExpBitSize, '0'),
            MantissaBits: Convert.ToString(mantissa, 2)
                .PadLeft(halfSpec.MantissaBitSize, '0'),
            TypeName: FloatTypeKind.Half
        );
    }
#endif
    public static FloatInfo AnalyzeFloat(this float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        var rawExponent = (int)(bits >> floatSpec.MantissaBitSize & floatSpec.ExpMask);
        var mantissa = bits & floatSpec.MantissaMask;

        return new FloatInfo(
            Spec: floatSpec,
            Bits: bits,
            IsNegative: bits < 0,
            IsPositiveInfinity: float.IsPositiveInfinity(value),
            IsNegativeInfinity: float.IsNegativeInfinity(value),
            IsQuietNaN: float.IsNaN(value) && (bits & floatSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: float.IsNaN(value) && (bits & floatSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - floatSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << floatSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(rawExponent, 2)
                .PadLeft(floatSpec.ExpBitSize, '0'),
            MantissaBits: Convert.ToString(mantissa, 2)
                .PadLeft(floatSpec.MantissaBitSize, '0'),
            TypeName: FloatTypeKind.Float
        );
    }
    public static FloatInfo AnalyzeDouble(this double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        var rawExponent = (int)(bits >> doubleSpec.MantissaBitSize & doubleSpec.ExpMask);
        var mantissa = bits & doubleSpec.MantissaMask;

        return new FloatInfo(
            Spec: doubleSpec,
            Bits: bits,
            IsNegative: bits < 0,
            IsPositiveInfinity: double.IsPositiveInfinity(value),
            IsNegativeInfinity: double.IsNegativeInfinity(value),
            IsQuietNaN: double.IsNaN(value) && (bits & doubleSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: double.IsNaN(value) && (bits & doubleSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - doubleSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1L << doubleSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(rawExponent, 2)
                .PadLeft(doubleSpec.ExpBitSize, '0'),
            MantissaBits: Convert.ToString(mantissa, 2)
                .PadLeft(doubleSpec.MantissaBitSize, '0'),
            TypeName: FloatTypeKind.Double
        );
    }
}