using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(Guid))]
[ReprOptions(needsPrefix: true)]
internal class GuidFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Guid)obj).ToString()}";
    }
}

[ReprFormatter(typeof(Uri))]
[ReprOptions(needsPrefix: true)]
internal class UriFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Uri)obj).ToString()}";
    }
}

[ReprFormatter(typeof(Version))]
[ReprOptions(needsPrefix: true)]
internal class VersionFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Version)obj).ToString()}";
    }
}