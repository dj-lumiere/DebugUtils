namespace DebugUtils.Repr.Models;

internal enum FloatTypeKind
{
    Half,
    Float,
    Double
}

internal readonly record struct FloatInfo(
    FloatSpec Spec,
    long Bits,
    int RealExponent,
    ulong Significand,
    FloatTypeKind TypeName)
{
    public bool IsNegative => Bits < 0;

    public long Mantissa => Bits & Spec.MantissaMask;

    public bool IsPositiveInfinity
    {
        get
        {
            var rawExponent = Bits >> Spec.MantissaBitSize & Spec.ExpMask;
            var mantissa = Bits & Spec.MantissaMask;
            return !IsNegative && rawExponent == Spec.ExpMask && mantissa == 0;
        }
    }

    public bool IsNegativeInfinity
    {
        get
        {
            var rawExponent = Bits >> Spec.MantissaBitSize & Spec.ExpMask;
            var mantissa = Bits & Spec.MantissaMask;
            return IsNegative && rawExponent == Spec.ExpMask && mantissa == 0;
        }
    }

    public bool IsQuietNaN
    {
        get
        {
            var rawExponent = Bits >> Spec.MantissaBitSize & Spec.ExpMask;
            var mantissa = Bits & Spec.MantissaMask;
            return rawExponent == Spec.ExpMask && mantissa != 0 &&
                   (Bits & Spec.MantissaMsbMask) != 0;
        }
    }

    public bool IsSignalingNaN
    {
        get
        {
            var rawExponent = Bits >> Spec.MantissaBitSize & Spec.ExpMask;
            var mantissa = Bits & Spec.MantissaMask;
            return rawExponent == Spec.ExpMask && mantissa != 0 &&
                   (Bits & Spec.MantissaMsbMask) == 0;
        }
    }

    public bool IsSubnormal => (Bits & Spec.ExpMask) == 0;
};