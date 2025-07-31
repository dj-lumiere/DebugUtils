using DebugUtils.Records;

namespace DebugUtils.Formatters;

internal static class FloatAnalyzers
{
#if NET5_0_OR_GREATER
    private static readonly FloatSpec halfSpec = new(ExpBitSize: 5, MantissaBitSize: 10,
        TotalSize: 16, MantissaMask: 0x3FF, MantissaMsbMask: 0x3FF, ExpMask: 0x1F, ExpOffset: 15);
#endif
    private static readonly FloatSpec floatSpec = new(ExpBitSize: 8, MantissaBitSize: 23,
        TotalSize: 32, MantissaMask: 0x7FFFFF, MantissaMsbMask: 0x7FFFFF, ExpMask: 0xFF,
        ExpOffset: 127);

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
            IsNegative: bits < 0,
            IsPositiveInfinity: Half.IsPositiveInfinity(value: value),
            IsNegativeInfinity: Half.IsNegativeInfinity(value: value),
            IsQuietNaN: Half.IsNaN(value: value) && (bits & halfSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: Half.IsNaN(value: value) && (bits & halfSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - halfSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << halfSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(value: rawExponent, toBase: 2)
                .PadLeft(totalWidth: halfSpec.ExpBitSize, paddingChar: '0'),
            MantissaBits: Convert.ToString(value: mantissa, toBase: 2)
                .PadLeft(totalWidth: halfSpec.MantissaBitSize, paddingChar: '0'),
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
            IsNegative: bits < 0,
            IsPositiveInfinity: Single.IsPositiveInfinity(f: value),
            IsNegativeInfinity: Single.IsNegativeInfinity(f: value),
            IsQuietNaN: Single.IsNaN(f: value) && (bits & floatSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: Single.IsNaN(f: value) && (bits & floatSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - floatSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1 << floatSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(value: rawExponent, toBase: 2)
                .PadLeft(totalWidth: floatSpec.ExpBitSize, paddingChar: '0'),
            MantissaBits: Convert.ToString(value: mantissa, toBase: 2)
                .PadLeft(totalWidth: floatSpec.MantissaBitSize, paddingChar: '0'),
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
            IsNegative: bits < 0,
            IsPositiveInfinity: Double.IsPositiveInfinity(d: value),
            IsNegativeInfinity: Double.IsNegativeInfinity(d: value),
            IsQuietNaN: Double.IsNaN(d: value) && (bits & doubleSpec.MantissaMsbMask) != 0,
            IsSignalingNaN: Double.IsNaN(d: value) && (bits & doubleSpec.MantissaMsbMask) == 0,
            RealExponent: rawExponent - doubleSpec.ExpOffset,
            Mantissa: mantissa,
            Significand: (ulong)(rawExponent == 0
                ? mantissa
                : (1L << doubleSpec.MantissaBitSize) + mantissa),
            ExpBits: Convert.ToString(value: rawExponent, toBase: 2)
                .PadLeft(totalWidth: doubleSpec.ExpBitSize, paddingChar: '0'),
            MantissaBits: Convert.ToString(value: mantissa, toBase: 2)
                .PadLeft(totalWidth: doubleSpec.MantissaBitSize, paddingChar: '0'),
            TypeName: FloatTypeKind.Double
        );
    }
}