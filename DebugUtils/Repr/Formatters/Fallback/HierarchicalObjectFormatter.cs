using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
public class HierarchicalObjectFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        config = config.GetContainerConfig() with { TypeMode = TypeReprMode.AlwaysHide };
        var visited2 = new HashSet<int>();
        return obj.GetJson(config: config, visited: visited2, depth: 0)
                  .ToString();
    }
}