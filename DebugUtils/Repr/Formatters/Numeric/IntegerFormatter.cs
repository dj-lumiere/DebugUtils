using System.ComponentModel;
using System.Numerics;
using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Numeric;

[ReprFormatter(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int),
    typeof(uint), typeof(long), typeof(ulong), typeof(BigInteger)
    #if NET7_0_OR_GREATER
    , typeof(Int128), typeof(UInt128)
    #endif
)]
[ReprOptions(needsPrefix: true)]
public class IntegerFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited = null)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName: nameof(obj));
        }

        return config.IntMode switch
        {
            IntReprMode.Binary => obj.FormatAsBinary(),
            IntReprMode.Decimal => obj.ToString()!,
            IntReprMode.Hex => obj.FormatAsHex(),
            IntReprMode.HexBytes => obj.FormatAsHexBytes(),
            _ => throw new InvalidEnumArgumentException(message: "Invalid Repr Config")
        };
    }
}