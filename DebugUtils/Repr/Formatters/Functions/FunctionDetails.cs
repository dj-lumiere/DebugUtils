using System.Reflection;

namespace DebugUtils.Repr.Formatters.Functions;

public class FunctionDetails
{
    public string Name { get; }
    public ParameterDetails[] Parameters { get; }
    public ReturnInfo Return { get; }
    public MethodModifiers Modifiers { get; }

    public FunctionDetails(MethodInfo methodInfo)
    {
        Name = methodInfo.GetSanitizedName();
        Parameters = methodInfo.GetParameterDetails();
        Return = methodInfo.ToReturnInfo();
        Modifiers = methodInfo.ToMethodModifiers();
    }

    public override string ToString()
    {
        var parameterStr = String.Join(separator: ", ",
            values: Parameters.Select(selector: p => p.ToString()));
        return
            $"{Modifiers} {Return} {Name}({parameterStr})";
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
        var sanitizedName = "";
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
            sanitizedName = "Lambda";
            return sanitizedName;
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