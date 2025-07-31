using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Primitive;

public class IntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((IntPtr)obj).ToInt32():X8}"
            : $"0x{((IntPtr)obj).ToInt64():X16}";
    }
}

public class UIntPtrFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return IntPtr.Size == 4
            ? $"0x{((UIntPtr)obj).ToUInt32():X8}"
            : $"0x{((UIntPtr)obj).ToUInt64():X16}";
    }
}