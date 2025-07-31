using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Interfaces;

/// <summary>
///     Defines a contract for a formatter.
/// </summary>
public interface IReprFormatter
{
    string ToRepr(object obj, ReprConfig config, HashSet<int>? visited = null);
}