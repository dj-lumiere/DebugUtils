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
        IsAsync = method.IsDefined(attributeType: typeof(AsyncStateMachineAttribute));
    }

    public override string ToString()
    {
        return TypeName;
    }
}

internal static class ReturnInfoExtensions
{
    public static ReturnInfo ToReturnInfo(this MethodInfo methodInfo)
    {
        return new ReturnInfo(method: methodInfo);
    }
}