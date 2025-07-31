using DebugUtils.Records;
using DebugUtils.Interfaces;

namespace DebugUtils.Formatters;

public class ArrayFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig reprConfig, HashSet<int>? visited)
    {
        var array = (Array)obj;
        // Apply container defaults if configured
        reprConfig = reprConfig.ForceFloatModeInContainer && reprConfig.ForceIntModeInContainer
            ? reprConfig
            : ReprConfig.ContainerDefaults;

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(new int[rank], 0, reprConfig, visited);
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
            for (int i = 0; i < array.GetLength(dimension); i++)
            {
                indices[dimension] = i;
                var value = array.GetValue(indices);
                if (value is Array innerArray)
                {
                    // If the element is a jagged array, recurse directly to format its content
                    // without adding another "Array(...)" wrapper.
                    items.Add(ArrayToReprRecursive(innerArray, new int[innerArray.Rank], 0,
                        reprConfig, visited));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(value?.Repr(reprConfig, visited) ?? "null");
                }
            }

            return "[" + string.Join(", ", items) + "]";
        } // Not last dimension - recurse deeper

        var subArrays = new List<string>();
        for (int i = 0; i < array.GetLength(dimension); i++)
        {
            indices[dimension] = i;
            subArrays.Add(ArrayToReprRecursive(array, indices, dimension + 1, reprConfig,
                visited));
        }

        return "[" + string.Join(", ", subArrays) + "]";
    }
}