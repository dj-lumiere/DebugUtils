using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(Guid))]
[ReprOptions(needsPrefix: true)]
public class GuidFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Guid)obj).ToString()}";
    }
}

[ReprFormatter(typeof(Uri))]
[ReprOptions(needsPrefix: true)]
public class UriFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Uri)obj).ToString()}";
    }
}

[ReprFormatter(typeof(Version))]
[ReprOptions(needsPrefix: true)]
public class VersionFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((Version)obj).ToString()}";
    }
}