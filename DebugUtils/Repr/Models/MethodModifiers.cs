using System.Reflection;
using System.Runtime.CompilerServices;

namespace DebugUtils.Repr.Models;

internal readonly struct MethodModifiers
{
    public bool IsPublic { get; }
    public bool IsPrivate { get; }
    public bool IsProtected { get; }
    public bool IsInternal { get; }

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

        foreach (var (condition, name) in new[]
                 {
                     (IsPublic, "public"),
                     (IsPrivate, "private"),
                     (IsProtected, "protected"),
                     (IsInternal, "internal"),
                     (IsStatic, "static"),
                     (IsAbstract, "abstract"),
                     (IsVirtual, "virtual"),
                     (IsOverride, "override"),
                     (IsSealed, "sealed"),
                     (IsAsync, "async"),
                     (IsExtern, "extern"),
                     (IsUnsafe, "unsafe"),
                     (IsGeneric, "generic")
                 })
        {
            if (condition)
            {
                modifiers.Add(item: name);
            }
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

    public static bool IsOverrideMethod(this MethodInfo method)
    {
        // A method is override if it's virtual and has a base definition
        return method.IsVirtual &&
               method.GetBaseDefinition() != method;
    }
    public static bool IsAsyncMethod(this MethodInfo method)
    {
        // Check if a method is marked with an AsyncStateMachine attribute
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