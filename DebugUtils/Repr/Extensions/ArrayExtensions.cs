using System.Text.Json.Nodes;

namespace DebugUtils.Repr.Extensions;

internal static class ArrayExtensions
{
    public static string ArrayToReprRecursive(this Array array, int[] indices, int dimension,
        ReprContext context)
    {
        if (dimension == array.Rank - 1)
        {
            // Last dimension - collect actual values
            var items = new List<string>();
            for (var i = 0; i < array.GetLength(dimension: dimension); i += 1)
            {
                indices[dimension] = i;
                if (context.Config.MaxItemsPerContainer >= 0 &&
                    i >= context.Config.MaxItemsPerContainer)
                {
                    var truncatedItemCount = array.GetLength(dimension: dimension) -
                                             context.Config.MaxItemsPerContainer;
                    items.Add(item: $"... {truncatedItemCount} more items");
                    break;
                }

                var value = array.GetValue(indices: indices);
                if (value is Array innerArray)
                {
                    // If the element is a jagged array, recurse directly to format its content
                    // without adding another "Array(...)" wrapper.
                    items.Add(item: innerArray.ArrayToReprRecursive(
                        indices: new int[innerArray.Rank], dimension: 0,
                        context: context.WithIncrementedDepth()));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(
                        item: value?.Repr(context: context.WithIncrementedDepth()) ?? "null");
                }
            }

            return "[" + String.Join(separator: ", ", values: items) + "]";
        } // Not last dimension - recurse deeper

        var subArrays = new List<string>();
        for (var i = 0; i < array.GetLength(dimension: dimension); i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                var truncatedItemCount = array.GetLength(dimension: dimension) -
                                         context.Config.MaxItemsPerContainer;
                subArrays.Add(item: $"... {truncatedItemCount} more items");
                break;
            }

            indices[dimension] = i;
            subArrays.Add(item: ArrayToReprRecursive(array: array, indices: indices,
                dimension: dimension + 1, context: context.WithIncrementedDepth()));
        }

        return "[" + String.Join(separator: ", ", values: subArrays) + "]";
    }

    public static JsonNode ArrayToHierarchicalReprRecursive(this Array array, int[] indices,
        int dimension,
        ReprContext context)
    {
        if (dimension == array.Rank - 1)
        {
            // Last dimension - collect actual values
            var items = new JsonArray();
            for (var i = 0; i < array.GetLength(dimension: dimension); i += 1)
            {
                indices[dimension] = i;
                if (context.Config.MaxItemsPerContainer >= 0 &&
                    i >= context.Config.MaxItemsPerContainer)
                {
                    var truncatedItemCount = array.GetLength(dimension: dimension) -
                                             context.Config.MaxItemsPerContainer;
                    items.Add(item: $"... ({truncatedItemCount} more items)");
                    break;
                }

                var value = array.GetValue(indices: indices);
                if (value is Array innerArray)
                {
                    // If the element is a jagged array, recurse directly to format its content
                    // without adding another "Array(...)" wrapper.
                    items.Add(
                        item: innerArray.FormatAsJsonNode(
                            context: context.WithIncrementedDepth()));
                }
                else
                {
                    // Otherwise, format the element normally.
                    // Also, visited must be not null at this point, because the Repr call
                    // at the first time should have made visited not null.
                    items.Add(
                        item: value.FormatAsJsonNode(context: context.WithIncrementedDepth()));
                }
            }

            return items;
        } // Not last dimension - recurse deeper

        var subArrays = new JsonArray();
        for (var i = 0; i < array.GetLength(dimension: dimension); i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                var truncatedItemCount = array.GetLength(dimension: dimension) -
                                         context.Config.MaxItemsPerContainer;
                subArrays.Add(item: $"... ({truncatedItemCount} more items)");
                break;
            }

            indices[dimension] = i;
            subArrays.Add(item: array.ArrayToHierarchicalReprRecursive(indices: indices,
                dimension: dimension + 1, context: context.WithIncrementedDepth()));
        }

        return subArrays;
    }
}