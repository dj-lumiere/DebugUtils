using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

/// <summary>
///     The default object formatter that handles any type not specifically registered.
///     It uses reflection to represent the object's fields and properties based on ViewMode.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class ObjectFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var members = obj.GetObjectMembers(context: context);
        var parts = new List<string>();

        foreach (var member in members.publicFields)
        {
            parts.Add(item: obj.ToReprParts(f: member, context: context));
        }

        foreach (var member in members.publicAutoProps)
        {
            parts.Add(item: obj.ToReprParts(pair: member, context: context));
        }

        if (context.Config.MaxMemberTimeMs > 0)
        {
            foreach (var member in members.publicProperties)
            {
                parts.Add(item: obj.ToReprParts(p: member, context: context));
            }
        }

        foreach (var member in members.privateFields)
        {
            parts.Add(item: obj.ToPrivateReprParts(f: member, context: context));
        }

        foreach (var member in members.privateAutoProps)
        {
            parts.Add(item: obj.ToPrivateReprParts(pair: member, context: context));
        }

        if (context.Config.MaxMemberTimeMs > 0)
        {
            foreach (var member in members.privateProperties)
            {
                parts.Add(item: obj.ToPrivateReprParts(p: member, context: context));
            }
        }

        if (members.truncated)
        {
            parts.Add(item: "...");
        }

        return String.Join(separator: ", ", values: parts);
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        var type = obj.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        if (!type.IsValueType)
        {
            result.Add(propertyName: "hashCode",
                value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        }

        var members = obj.GetObjectMembers(context: context);

        foreach (var member in members.publicFields)
        {
            result.Add(property: obj.ToReprTreeParts(f: member, context: context));
        }

        foreach (var member in members.publicAutoProps)
        {
            result.Add(property: obj.ToReprTreeParts(pair: member, context: context));
        }

        foreach (var member in members.publicProperties)
        {
            result.Add(property: obj.ToReprTreeParts(p: member, context: context));
        }

        foreach (var member in members.privateFields)
        {
            result.Add(property: obj.ToPrivateReprTreeParts(f: member, context: context));
        }

        foreach (var member in members.privateAutoProps)
        {
            result.Add(property: obj.ToPrivateReprTreeParts(pair: member, context: context));
        }

        foreach (var member in members.privateProperties)
        {
            result.Add(property: obj.ToPrivateReprTreeParts(p: member, context: context));
        }

        return result;
    }
}