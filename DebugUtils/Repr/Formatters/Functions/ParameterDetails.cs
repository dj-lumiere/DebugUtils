using System.Reflection;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Functions;

internal readonly struct ParameterDetails
{
    public string Name { get; }
    public string TypeReprName { get; }
    public string Modifier { get; }
    public object? DefaultValue { get; }

    public ParameterDetails(ParameterInfo parameterInfo)
    {
        Name = parameterInfo.Name ?? "unnamed";
        TypeReprName = parameterInfo.ParameterType.GetReprTypeName();
        Modifier = parameterInfo.GetParameterModifier();
        DefaultValue = parameterInfo.HasDefaultValue
            ? parameterInfo.DefaultValue
            : null;
    }

    public override string ToString()
    {
        List<string> parts = new();
        if (!String.IsNullOrEmpty(value: Modifier))
        {
            parts.Add(item: Modifier);
        }

        if (!String.IsNullOrEmpty(value: TypeReprName))
        {
            parts.Add(item: TypeReprName);
        }

        if (!String.IsNullOrEmpty(value: Name))
        {
            parts.Add(item: Name);
        }

        return String.Join(separator: " ", values: parts);
    }
}

internal static class ParameterDetailsExtensions
{
    public static ParameterDetails ToParameterDetails(this ParameterInfo parameterInfo)
    {
        return new ParameterDetails(parameterInfo: parameterInfo);
    }

    public static string GetParameterModifier(this ParameterInfo param)
    {
        // Check for ref/out/in modifiers
        if (param.ParameterType.IsByRef)
        {
            if (param.IsOut)
            {
                return "out";
            }

            if (param.IsIn)
            {
                return "in";
            }

            return "ref"; // Default for ByRef that's not in/out
        }

        // Check for params array
        if (param.IsDefined(attributeType: typeof(ParamArrayAttribute)))
        {
            return "params";
        }

        return ""; // No modifier
    }
}