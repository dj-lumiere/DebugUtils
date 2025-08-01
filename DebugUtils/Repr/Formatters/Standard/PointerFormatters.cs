using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

// ReSharper disable BuiltInTypeReferenceStyle

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(IntPtr))]
[ReprOptions(needsPrefix: true)]
internal class IntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((IntPtr)obj).ToInt32():X8}"
            : $"0x{((IntPtr)obj).ToInt64():X16}";
    }
}

[ReprFormatter(typeof(UIntPtr))]
[ReprOptions(needsPrefix: true)]
internal class UIntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((UIntPtr)obj).ToUInt32():X8}"
            : $"0x{((UIntPtr)obj).ToUInt64():X16}";
    }
}