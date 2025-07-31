using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Records;
using DebugUtils.Interfaces;

namespace DebugUtils.Formatters;

public class TupleFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var tuple = (ITuple)obj;
        visited ??= new HashSet<int>();
        // Apply container defaults if configured
        config = config.ForceFloatModeInContainer && config.ForceIntModeInContainer
            ? config
            : ReprConfig.ContainerDefaults;

        StringBuilder sb = new StringBuilder();
        sb.Append("(");
        for (int i = 0; i < tuple.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }
            sb.Append(tuple[i].Repr(config, visited));
        }
        sb.Append(")");
        return sb.ToString();
    }
}