namespace DebugUtils.Repr.Models;

/// <summary>
///     Encapsulates IEEE 754 floating-point format specifications.
///     Contains bit masks and offsets needed for precise floating-point analysis.
/// </summary>
internal readonly record struct FloatSpec(
    int ExpBitSize,
    int MantissaBitSize,
    long MantissaMask,
    long MantissaMsbMask,
    long ExpMask,
    int ExpOffset);