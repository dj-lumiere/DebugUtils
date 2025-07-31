namespace DebugUtils.Repr.Records;

/// <summary>
///     Encapsulates IEEE 754 floating-point format specifications.
///     Contains bit masks and offsets needed for precise floating-point analysis.
/// </summary>
public record FloatSpec(
    int ExpBitSize,
    int MantissaBitSize,
    int TotalSize,
    long MantissaMask,
    long MantissaMsbMask,
    long ExpMask,
    int ExpOffset
);