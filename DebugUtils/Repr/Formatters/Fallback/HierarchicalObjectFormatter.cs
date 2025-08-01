using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
internal class HierarchicalObjectFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        config = config.GetContainerConfig() with { TypeMode = TypeReprMode.AlwaysHide };
        var visited2 = new HashSet<int>();
        return obj.ToJsonObject(config: config, visited: visited2, depth: 0)
                  .ToString();
    }
}