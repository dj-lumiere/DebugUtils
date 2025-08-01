using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

// The default formatter that opts for ToString. This formatter should not be used when
// ToString method overrides object.ToString.
[ReprOptions(needsPrefix:false)]
public class ToStringFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return obj.ToString() ?? "";
    }
}