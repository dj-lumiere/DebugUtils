using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

internal static class ArrayExtensions
{
    public static string ArrayToReprRecursive(this Array array, int[] indices, int dimension,
        ReprConfig config, HashSet<int>? visited)
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
                    items.Add(item: innerArray.ArrayToReprRecursive(
                        indices: new int[innerArray.Rank], dimension: 0,
                        config: config, visited: visited));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(
                        item: value?.Repr(config: config, visited: visited) ?? "null");
                }
            }

            return "[" + String.Join(separator: ", ", values: items) + "]";
        } // Not last dimension - recurse deeper

        var subArrays = new List<string>();
        for (var i = 0; i < array.GetLength(dimension: dimension); i++)
        {
            indices[dimension] = i;
            subArrays.Add(item: ArrayToReprRecursive(array: array, indices: indices,
                dimension: dimension + 1, config: config,
                visited: visited));
        }

        return "[" + String.Join(separator: ", ", values: subArrays) + "]";
    }
}