using System.Reflection;
using System.Runtime.CompilerServices;

namespace DebugUtils.Repr.Formatters.Functions;

public class ReturnInfo
{
    public string TypeName { get; }
    public bool IsRef { get; }
    public bool IsAsync { get; }

    public ReturnInfo(MethodInfo method)
    {
        var returnType = method.ReturnType;
        TypeName = returnType.GetReprTypeNameByTypeName();
        IsRef = returnType.IsByRef;
        IsAsync = method.IsDefined(typeof(AsyncStateMachineAttribute));
    }

    public override string ToString()
    {
        var result = IsRef
            ? "ref "
            : "";
        result += IsAsync
            ? $"async {TypeName}"
            : TypeName;
        return result;
    }
}

internal static class ReturnInfoExtensions
{
    public static ReturnInfo ToReturnInfo(this MethodInfo methodInfo)
    {
        return new ReturnInfo(methodInfo);
    }
}