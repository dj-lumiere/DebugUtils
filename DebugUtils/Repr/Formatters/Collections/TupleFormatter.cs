using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

public class TupleFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var tuple = (ITuple)obj;
        visited ??= new HashSet<int>();
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var sb = new StringBuilder();
        sb.Append(value: "(");
        for (var i = 0; i < tuple.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(value: ", ");
            }

            sb.Append(value: tuple[index: i]
               .Repr(config: config, visited: visited));
        }

        sb.Append(value: ")");
        return sb.ToString();
    }
}