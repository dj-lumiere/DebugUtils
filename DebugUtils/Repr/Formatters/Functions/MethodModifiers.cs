using System.Reflection;
using System.Runtime.CompilerServices;

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
    public bool IsExtern { get; }
    public bool IsUnsafe { get; }

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
        IsExtern = IsExternMethod(method);
        IsUnsafe = IsUnsafeMethod(method);
    }

    private static string GetAccessLevelName(MethodInfo method)
    {
        if (method.IsPublic)
        {
            return "public";
        }

        if (method.IsPrivate)
        {
            return "private";
        }

        if (method.IsFamily)
        {
            return "protected";
        }

        if (method.IsAssembly)
        {
            return "internal";
        }

        if (method.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (method.IsFamilyAndAssembly)
        {
            return "private protected";
        }

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
            attributeType: typeof(AsyncStateMachineAttribute));
    }

    private static bool IsExtensionMethod(MethodInfo method)
    {
        // Extension methods are static and have ExtensionAttribute
        return method.IsStatic &&
               method.IsDefined(attributeType: typeof(ExtensionAttribute));
    }

    private static bool IsOperatorMethod(MethodInfo method)
    {
        return method.IsSpecialName &&
               (method.Name.StartsWith(value: "op_") || method.Name == "Implicit" ||
                method.Name == "Explicit");
    }

    private static bool IsPropertyMethod(MethodInfo method)
    {
        return method.IsSpecialName &&
               (method.Name.StartsWith(value: "get_") || method.Name.StartsWith(value: "set_"));
    }
    private static bool IsExternMethod(MethodInfo method)
    {
        return method.Attributes.HasFlag(flag: MethodAttributes.PinvokeImpl);
    }
    private static bool IsUnsafeMethod(MethodInfo method)
    {
        // A method is considered unsafe if its return type or any of its parameters are a pointer type.
        if (method.ReturnType.IsPointer)
        {
            return true;
        }

        if (method.GetParameters()
                  .Any(predicate: p => p.ParameterType.IsPointer))
        {
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        var modifiers = new List<string>();

        // 1. Add Access Modifier
        if (!String.IsNullOrEmpty(value: AccessLevelName))
        {
            modifiers.Add(item: AccessLevelName);
        }

        // 2. Add Scope/Inheritance Modifiers
        // Note: C# syntax doesn't allow 'virtual' and 'abstract' with 'sealed'
        // so the order here handles the common combinations correctly.
        if (IsStatic)
        {
            modifiers.Add(item: "static");
        }

        if (IsAbstract)
        {
            modifiers.Add(item: "abstract");
        }

        if (IsVirtual)
        {
            modifiers.Add(item: "virtual");
        }

        if (IsOverride)
        {
            modifiers.Add(item: "override");
        }

        if (IsSealed)
        {
            modifiers.Add(item: "sealed");
        }

        // 3. Add Other Modifiers
        if (IsAsync)
        {
            modifiers.Add(item: "async");
        }

        if (IsExtern)
        {
            modifiers.Add(item: "extern");
        }

        if (IsUnsafe)
        {
            modifiers.Add(item: "unsafe");
        }

        if (IsGeneric)
        {
            modifiers.Add(item: "generic");
        }

        // Join the list into a single string
        return String.Join(separator: " ", values: modifiers);
    }
}

internal static class MethodModifiersExtensions
{
    public static MethodModifiers ToMethodModifiers(this MethodInfo methodInfo)
    {
        return new MethodModifiers(method: methodInfo);
    }
}