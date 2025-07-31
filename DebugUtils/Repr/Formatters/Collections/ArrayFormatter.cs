using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

public class ArrayFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var array = (Array)obj;
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(indices: new int[rank], dimension: 0,
            reprConfig: config, visited: visited);
        return content;
    }
}

internal static class ArrayFormatterLogic
{
    public static string ArrayToReprRecursive(this Array array, int[] indices, int dimension,
        ReprConfig reprConfig, HashSet<int>? visited)
    {
        if (dimension == array.Rank - 1)
        {
            // Last dimension - collect actual values
            var items = new List<string>();
            for (var i = 0; i < array.GetLength(dimension: dimension); i++)
            {
                indices[dimension] = i;
                var value = array.GetValue(indices: indices);
                if (value is Array innerArray)
                {
                    // If the element is a jagged array, recurse directly to format its content
                    // without adding another "Array(...)" wrapper.
                    items.Add(item: ArrayToReprRecursive(array: innerArray,
                        indices: new int[innerArray.Rank], dimension: 0,
                        reprConfig: reprConfig, visited: visited));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(
                        item: value?.Repr(config: reprConfig, visited: visited) ?? "null");
                }
            }

            return "[" + String.Join(separator: ", ", values: items) + "]";
        } // Not last dimension - recurse deeper

        var subArrays = new List<string>();
        for (var i = 0; i < array.GetLength(dimension: dimension); i++)
        {
            indices[dimension] = i;
            subArrays.Add(item: ArrayToReprRecursive(array: array, indices: indices,
                dimension: dimension + 1, reprConfig: reprConfig,
                visited: visited));
        }

        return "[" + String.Join(separator: ", ", values: subArrays) + "]";
    }
}