namespace DebugUtils.Repr.Formatters;

internal enum FloatTypeKind
{
    Half,
    Float,
    Double
}

internal record FloatInfo(
    FloatSpec Spec,
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