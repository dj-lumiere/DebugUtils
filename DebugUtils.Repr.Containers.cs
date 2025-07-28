using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
public static partial class DebugUtils
{
    private static string ReprIList(this IList list, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var items = list.Cast<object>()
            .Select(item => item?.Repr(floatReprConfig, intReprConfig) ?? "null");
        return "[" + string.Join(", ", items) + "]";
    }
    private static string ReprIEnumerable(this IEnumerable list, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var items = list.Cast<object>()
            .Select(item => item?.Repr(floatReprConfig, intReprConfig) ?? "null");
        return $"[{string.Join(", ", items)}]";
    }
    private static string ReprIDictionary(this IDictionary dict, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        if (dict.Count == 0) return "{}";
        var items = new List<string>();
        // Check if it's a generic dictionary
        if (dict.GetType()
            .IsGenericType)
        {
            // Generic dictionary - enumerate as KeyValuePair
            foreach (var kvp in dict)
            {
                // kvp is actually DictionaryEntry when using non-generic IEnumerable
                // But we can access it through the IDictionary interface
                var entry = (DictionaryEntry)kvp;
                var key = entry.Key?.Repr(floatReprConfig, intReprConfig) ?? "null";
                var value = entry.Value?.Repr(floatReprConfig, intReprConfig) ?? "null";
                items.Add($"{key}: {value}");
            }
        }
        else
        {
            // Non-generic dictionary
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key?.Repr(floatReprConfig, intReprConfig) ?? "null";
                var value = entry.Value?.Repr(floatReprConfig, intReprConfig) ?? "null";
                items.Add($"{key}: {value}");
            }
        }

        items.Sort(); // For deterministic output
        return "{" + string.Join(", ", items) + "}";
    }
    private static string ReprISet(this IEnumerable set, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var items = set
            .Cast<object>()
            .OrderBy(item => item?.Repr(floatReprConfig, intReprConfig) ?? "")
            .Select(item => item?.Repr(floatReprConfig, intReprConfig) ?? "null");
        return "{" + string.Join(", ", items) + "}";
    }
    private static string ReprQueue(this IEnumerable queue, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var items = queue.Cast<object>()
            .Select(item => item?.Repr(floatReprConfig, intReprConfig) ?? "null");
        return "Queue([" + string.Join(", ", items) + "])";
    }
    private static string ReprStack(this IEnumerable stack, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var items = stack.Cast<object>()
            .Select(item => item?.Repr(floatReprConfig, intReprConfig) ?? "null");
        return "Stack([" + string.Join(", ", items) + "])";
    }
    public static string ReprArray(this Array array, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (array == null) return "null";

        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        var rank = array.Rank;
        var content = ReprArrayRecursive(array, new int[rank], 0, floatReprConfig, intReprConfig);


        if (array.Rank != 1)
        {
            return $"{rank}DArray({content})";
        }

        if (array.GetType()
                .GetElementType()
                ?
                .IsArray ?? false)
        {
            return $"JaggedArray({content})";
        }

        return $"{rank}DArray({content})";
    }
    private static string ReprTuple(this ITuple tuple, FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
    {
        if (!floatReprConfig.forceFloatReprModeInContainer)
        {
            floatReprConfig = DefaultContainderFloatConfig;
        }

        if (!intReprConfig.forceIntReprModeInContainer)
        {
            intReprConfig = DefaultContainerIntConfig;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("(");
        for (int i = 0; i < tuple.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(tuple[i]
                .Repr(floatReprConfig, intReprConfig));
        }

        sb.Append(")");
        return sb.ToString();
    }
    private static string ReprArrayRecursive(Array array, int[] indices, int dimension,
        FloatReprConfig floatReprConfig,
        IntReprConfig intReprConfig)
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
                    items.Add(ReprArrayRecursive(innerArray, new int[innerArray.Rank], 0,
                        floatReprConfig, intReprConfig));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(value?.Repr(floatReprConfig, intReprConfig) ?? "null");
                }
            }

            return "[" + string.Join(", ", items) + "]";
        }

        // Not last dimension - recurse deeper
        var subArrays = new List<string>();
        for (int i = 0; i < array.GetLength(dimension); i++)
        {
            indices[dimension] = i;
            subArrays.Add(ReprArrayRecursive(array, indices, dimension + 1, floatReprConfig,
                intReprConfig));
        }

        return "[" + string.Join(", ", subArrays) + "]";
    }
}