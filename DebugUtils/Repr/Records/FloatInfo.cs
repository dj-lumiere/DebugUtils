namespace DebugUtils.Repr.Records;

public enum FloatTypeKind
{
    Half,
    Float,
    Double
}

public record FloatInfo(
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