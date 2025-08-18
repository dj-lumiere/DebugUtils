using System.Reflection;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Models;

internal readonly struct FunctionDetails
{
    public string Name { get; }
    public ParameterDetails[] Parameters { get; }
    public string ReturnTypeReprName { get; }
    public MethodModifiers Modifiers { get; }

    public FunctionDetails(MethodInfo methodInfo)
    {
        Name = methodInfo.GetSanitizedName();
        Parameters = methodInfo.GetParameterDetails();
        ReturnTypeReprName = methodInfo.ReturnType.GetReprTypeName();
        Modifiers = methodInfo.ToMethodModifiers();
    }

    public override string ToString()
    {
        var parameterStr = String.Join(separator: ", ",
            values: Parameters.Select(selector: p => p.ToString()));
        var parts = new List<string>();
        if (!String.IsNullOrEmpty(value: Modifiers.ToString()))
        {
            parts.Add(item: Modifiers.ToString());
            parts.Add(item: " ");
        }

        if (!String.IsNullOrEmpty(value: ReturnTypeReprName))
        {
            parts.Add(item: ReturnTypeReprName);
            parts.Add(item: " ");
        }

        if (!String.IsNullOrEmpty(value: Name))
        {
            parts.Add(item: Name);
        }

        parts.Add(item: "(" + parameterStr + ")");
        return String.Join(separator: "", values: parts);
    }
}

internal static class FunctionDetailsExtensions
{
    public static FunctionDetails ToFunctionDetails(this MethodInfo methodInfo)
    {
        return new FunctionDetails(methodInfo: methodInfo);
    }

    public static string GetSanitizedName(this MethodInfo methodInfo)
    {
        var unsanitizedName = methodInfo.Name;
        if (unsanitizedName.Contains(value: "g__"))
        {
            // Since we are finding "g__" and "|", which consist of ascii character,
            // it doesn't suffer from localization/cultural issues that matter how letters are counted.
            var start = unsanitizedName.IndexOf(value: "g__") + 3;
            var end = unsanitizedName.IndexOf(value: '|', startIndex: start);
            return end > start
                ? unsanitizedName.Substring(startIndex: start, length: end - start)
                : "local func";
        }

        // lambda functions always contain "b__".
        if (unsanitizedName.Contains(value: "b__"))
        {
            return "Lambda";
        }

        return unsanitizedName;
    }

    public static ParameterDetails[] GetParameterDetails(
        this MethodInfo methodInfo)
    {
        return methodInfo.GetParameters()
                         .Select(selector: p => p.ToParameterDetails())
                         .ToArray();
    }
}