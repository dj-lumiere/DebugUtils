namespace DebugUtils.Formatters;

public enum FloatTypeKind
{
    Half,
    Float,
    Double
}

/// <summary>
/// Encapsulates IEEE 754 floating-point format specifications.
/// Contains bit masks and offsets needed for precise floating-point analysis.
/// </summary>
public record FloatsSpec(
    int ExpBitSize,
    int MantissaBitSize,
    int TotalSize,
    long MantissaMask,
    long MantissaMsbMask,
    long ExpMask,
    int ExpOffset
)
{
    public static FloatsSpec HalfSpec =>
        new(5, 10, 16, 0x3FF, 0x3FF, 0x1F, 15);

    public static FloatsSpec FloatSpec => new(8, 23, 32, 0x7FFFFF, 0x7FFFFF, 0xFF, 127);

    public static FloatsSpec DoubleSpec =>
        new(11, 52, 64, 0xFFFFFFFFFFFFFL, 0x8000000000000L, 0x7FFL, 1023);
}

public record FloatInfo(
    FloatsSpec Spec,
    long Bits,
    bool IsNegative,
    bool IsPositiveInfinity,
    bool IsNegativeInfinity,
    bool IsQuietNaN,
    bool IsSignalingNaN,
    int RealExponent,
    long Mantissa,
    ulong Significand,
    string ExpBits,
    string MantissaBits,
    FloatTypeKind TypeName
);

internal static class FloatAnalyzers
{
    private static FloatsSpec halfSpec = FloatsSpec.HalfSpec;
    private static FloatsSpec floatSpec = FloatsSpec.FloatSpec;
    private static FloatsSpec doubleSpec = FloatsSpec.DoubleSpec;
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