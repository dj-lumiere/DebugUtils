using System.Reflection;

namespace DebugUtils.Repr.Extensions;

internal static class ObjectExtensions
{
    public static bool TryGetAutoPropInfo(this FieldInfo f, out string propName)
    {
        propName = null!;
        var n = f.Name; // "<PropName>k__BackingField"
        if (n.Length < 5 || n[index: 0] != '<')
        {
            return false;
        }

        var close = n.IndexOf(value: '>');
        if (close <= 1)
        {
            return false;
        }

        if (!n.EndsWith(value: "k__BackingField", comparisonType: StringComparison.Ordinal))
        {
            return false;
        }

        propName = n.Substring(startIndex: 1, length: close - 1);
        return true;
    }
    public static bool TryGetAnonymousInfo(this FieldInfo f, out string propName)
    {
        propName = null!;
        var n = f.Name; // "<PropName>i__Field"
        if (n.Length < 5 || n[index: 0] != '<')
        {
            return false;
        }

        var close = n.IndexOf(value: '>');
        if (close <= 1)
        {
            return false;
        }

        if (!n.EndsWith(value: "i__Field", comparisonType: StringComparison.Ordinal))
        {
            return false;
        }

        propName = n.Substring(startIndex: 1, length: close - 1);
        return true;
    }
    public static bool IsCompilerGeneratedName(this string fieldName)
    {
        return fieldName.Contains(value: "k__BackingField") || // Auto-property backing fields
               fieldName.Contains(value: "i__Field") || // Auto-property backing fields
               fieldName.StartsWith(value: "<") || // Most compiler-generated fields
               fieldName.Contains(value: "__") || // Various compiler patterns
               fieldName.Contains(value: "DisplayClass") || // Closure fields
               fieldName.EndsWith(value: "__0") || // State machine fields
               fieldName.Contains(value: "c__Iterator") || // Iterator fields
               fieldName == "EqualityContract"; // Record EqualityContract
    }
}