using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Functions;

public class FunctionFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited = null)
    {
        var del = (Delegate)obj;
        var functionDetails = del.Method.ToFunctionDetails();
        return functionDetails.ToString();
    }
}