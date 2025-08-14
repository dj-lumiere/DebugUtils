using System.Reflection;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;

namespace DebugUtils.Repr.Formatters;

/// <summary>
///     A generic formatter for any record type.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class RecordFormatter : IReprFormatter
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

        foreach (var member in members.privateFields)
        {
            parts.Add(item: obj.ToPrivateReprParts(f: member, context: context));
        }

        foreach (var member in members.privateAutoProps)
        {
            parts.Add(item: obj.ToPrivateReprParts(pair: member, context: context));
        }

        if (members.truncated)
        {
            parts.Add(item: "...");
        }

        return $"{{ {String.Join(separator: ", ", values: parts)} }}";
    }
}