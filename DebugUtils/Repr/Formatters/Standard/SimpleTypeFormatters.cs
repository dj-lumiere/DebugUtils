using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(bool))]
[ReprOptions(needsPrefix: false)]
internal class BoolFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return (bool)obj
            ? "true"
            : "false";
    }
}

[ReprFormatter(typeof(Enum))]
[ReprOptions(needsPrefix: false)]
internal class EnumFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{obj.GetReprTypeName()}.{obj}";
    }
}