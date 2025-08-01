using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters.Fallback;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Functions;

internal class MethodModifiers
{
    public bool IsPublic { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsProtected { get; set; }
    public bool IsInternal { get; set; }

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
        IsPublic = method.IsPublic;
        IsPrivate = method.IsPrivate || method.IsFamilyAndAssembly;
        IsProtected = method.IsFamily || method.IsFamilyOrAssembly;
        IsInternal = method.IsAssembly;
        IsStatic = method.IsStatic;
        IsVirtual = method is { IsVirtual: true, IsFinal: false }; // Virtual but not sealed
        IsOverride = method.IsOverrideMethod();
        IsAbstract = method.IsAbstract;
        IsSealed = method is { IsFinal: true, IsVirtual: true }; // Sealed override
        IsAsync = method.IsAsyncMethod();
        IsGeneric = method.IsGenericMethod;
        IsExtern = method.IsExternMethod();
        IsUnsafe = method.IsUnsafeMethod();
    }

    public override string ToString()
    {
        var modifiers = new List<string>();

        // 1. Add Access Modifier
        if (IsPublic)
        {
            modifiers.Add(item: "public");
        }

        if (IsPrivate)
        {
            modifiers.Add(item: "private");
        }

        if (IsProtected)
        {
            modifiers.Add(item: "protected");
        }

        if (IsInternal)
        {
            modifiers.Add(item: "internal");
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
    public static JsonObject ToJsonObject(this MethodModifiers methodModifiers, ReprConfig config,
        HashSet<int> visited, int depth)
    {
        var result = new JsonObject();
        result.Add(propertyName: "isPublic",
            value: methodModifiers.IsPublic.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isPrivate",
            value: methodModifiers.IsPrivate.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isProtected",
            value: methodModifiers.IsProtected.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isInternal",
            value: methodModifiers.IsInternal.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isStatic",
            value: methodModifiers.IsStatic.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isVirtual",
            value: methodModifiers.IsVirtual.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isOverride",
            value: methodModifiers.IsOverride.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isAbstract",
            value: methodModifiers.IsAbstract.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isSealed",
            value: methodModifiers.IsSealed.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isAsync",
            value: methodModifiers.IsAsync.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isGeneric",
            value: methodModifiers.IsGeneric.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isExtern",
            value: methodModifiers.IsExtern.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "isUnsafe",
            value: methodModifiers.IsUnsafe.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        return result;
    }

    public static bool IsOverrideMethod(this MethodInfo method)
    {
        // A method is override if it's virtual and has a base definition
        return method.IsVirtual &&
               method.GetBaseDefinition() != method;
    }
    public static bool IsAsyncMethod(this MethodInfo method)
    {
        // Check if method is marked with AsyncStateMachine attribute
        return method.IsDefined(
            attributeType: typeof(AsyncStateMachineAttribute));
    }
    public static bool IsExternMethod(this MethodInfo method)
    {
        return method.Attributes.HasFlag(flag: MethodAttributes.PinvokeImpl);
    }
    public static bool IsUnsafeMethod(this MethodInfo method)
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
}