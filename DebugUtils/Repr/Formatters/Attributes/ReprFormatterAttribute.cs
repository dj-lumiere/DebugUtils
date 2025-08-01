namespace DebugUtils.Repr.Formatters;

[AttributeUsage(validOn: AttributeTargets.Class)]
public class ReprFormatterAttribute : Attribute
{
    public ReprFormatterAttribute(params Type[] targetTypes)
    {
        TargetTypes = targetTypes;
    }
    public Type[] TargetTypes { get; }
}