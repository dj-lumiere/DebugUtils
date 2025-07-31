using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(bool))]
public class BoolFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return (bool)obj
            ? "true"
            : "false";
    }
}

[ReprFormatter(typeof(Enum))]
public class EnumFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{obj.GetReprTypeName()}.{obj}";
    }
}