using System.Reflection;

namespace DebugUtils.Repr.Formatters.Functions;

public class MethodModifiers
{
    // Access modifiers
    public string AccessLevelName { get; }

    // Other modifiers
    public bool IsStatic { get; }
    public bool IsVirtual { get; }
    public bool IsOverride { get; }
    public bool IsAbstract { get; }
    public bool IsSealed { get; }
    public bool IsAsync { get; }
    public bool IsGeneric { get; }
    public bool IsExtension { get; }
    public bool IsExtern { get; }
    public bool IsUnsafe { get; }

    // Special cases
    public bool IsConstructor { get; }
    public bool IsOperator { get; }
    public bool IsProperty { get; }

    public MethodModifiers(MethodInfo method)
    {
        AccessLevelName = GetAccessLevelName(method);
        IsStatic = method.IsStatic;
        IsVirtual = method.IsVirtual && !method.IsFinal; // Virtual but not sealed
        IsOverride = IsOverrideMethod(method);
        IsAbstract = method.IsAbstract;
        IsSealed = method.IsFinal && method.IsVirtual; // Sealed override
        IsAsync = IsAsyncMethod(method);
        IsGeneric = method.IsGenericMethod;
        IsExtension = IsExtensionMethod(method);
        IsConstructor = method.IsConstructor;
        IsOperator = IsOperatorMethod(method);
        IsProperty = IsPropertyMethod(method);
        IsExtern = IsExternMethod(method);
        IsUnsafe = IsUnsafeMethod(method);
    }

    private static string GetAccessLevelName(MethodInfo method)
    {
        if (method.IsPublic) return "public";
        if (method.IsPrivate) return "private";
        if (method.IsFamily) return "protected";
        if (method.IsAssembly) return "internal";
        if (method.IsFamilyOrAssembly) return "protected internal";
        if (method.IsFamilyAndAssembly) return "private protected";
        return "unknown";
    }

    private static bool IsOverrideMethod(MethodInfo method)
    {
        // A method is override if it's virtual and has a base definition
        return method.IsVirtual &&
               method.GetBaseDefinition() != method;
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        // Check if method is marked with AsyncStateMachine attribute
        return method.IsDefined(
            typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute));
    }

    private static bool IsExtensionMethod(MethodInfo method)
    {
        // Extension methods are static and have ExtensionAttribute
        return method.IsStatic &&
               method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
    }

    private static bool IsOperatorMethod(MethodInfo method)
    {
        return method.IsSpecialName &&
               (method.Name.StartsWith("op_") || method.Name == "Implicit" ||
                method.Name == "Explicit");
    }

    private static bool IsPropertyMethod(MethodInfo method)
    {
        return method.IsSpecialName &&
               (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"));
    }
    private static bool IsExternMethod(MethodInfo method)
    {
        return method.Attributes.HasFlag(MethodAttributes.PinvokeImpl);
    }
    private static bool IsUnsafeMethod(MethodInfo method)
    {
        // A method is considered unsafe if its return type or any of its parameters are a pointer type.
        if (method.ReturnType.IsPointer)
        {
            return true;
        }

        if (method.GetParameters()
                  .Any(p => p.ParameterType.IsPointer))
        {
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        var modifiers = new List<string>();

        // 1. Add Access Modifier
        if (!string.IsNullOrEmpty(AccessLevelName))
        {
            modifiers.Add(AccessLevelName);
        }

        // 2. Add Scope/Inheritance Modifiers
        // Note: C# syntax doesn't allow 'virtual' and 'abstract' with 'sealed'
        // so the order here handles the common combinations correctly.
        if (IsStatic) modifiers.Add("static");
        if (IsAbstract) modifiers.Add("abstract");
        if (IsVirtual) modifiers.Add("virtual");
        if (IsOverride) modifiers.Add("override");
        if (IsSealed) modifiers.Add("sealed");

        // 3. Add Other Modifiers
        if (IsAsync) modifiers.Add("async");
        if (IsExtern) modifiers.Add("extern");
        if (IsUnsafe) modifiers.Add("unsafe");
        if (IsGeneric) modifiers.Add("generic");

        // Join the list into a single string
        return string.Join(" ", modifiers);
    }
}

internal static class MethodModifiersExtensions
{
    public static MethodModifiers ToMethodModifiers(this MethodInfo methodInfo)
    {
        return new MethodModifiers(methodInfo);
    }
}