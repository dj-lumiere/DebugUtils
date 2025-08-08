using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(string))]
[ReprOptions(needsPrefix: false)]
internal class StringFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var s = (string)obj;
        if (s.Length <= context.Config.MaxStringLength)
        {
            return $"\"{(string)obj}\"";
        }

        var truncatedLetterCount = s.Length - context.Config.MaxStringLength;
        s = s[..context.Config.MaxStringLength];
        return $"\"{s}... ({truncatedLetterCount} more letters)\"";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var s = (string)obj;
        if (s.Length > context.Config.MaxStringLength)
        {
            var truncatedLetterCount = s.Length - context.Config.MaxStringLength;
            s = s[..context.Config.MaxStringLength] + $"... ({truncatedLetterCount} more letters)";
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: "string");
        result.Add(propertyName: "kind", value: "class");
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        result.Add(propertyName: "length", value: s.Length);
        result.Add(propertyName: "value", value: s);
        return result;
    }
}

[ReprFormatter(typeof(StringBuilder))]
[ReprOptions(needsPrefix: true)]
internal class StringBuilderFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var sb = (StringBuilder)obj;
        var s = sb.ToString();
        if (s.Length > context.Config.MaxStringLength)
        {
            var truncatedLetterCount = s.Length - context.Config.MaxStringLength;
            s = s[..context.Config.MaxStringLength] + $"... ({truncatedLetterCount} more letters)";
        }

        return $"{s}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var result = new JsonObject();
        var type = obj.GetType();
        var sb = (StringBuilder)obj;
        var s = sb.ToString();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "hashCode", value: RuntimeHelpers.GetHashCode(o: obj));
        result.Add(propertyName: "length", value: s.Length);
        result.Add(propertyName: "value", value: ToRepr(obj: obj, context: context));
        return result;
    }
}

[ReprFormatter(typeof(char))]
[ReprOptions(needsPrefix: false)]
internal class CharFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var value = (char)obj;
        return value switch
        {
            '\'' => "'''", // Single quote
            '\"' => "'\"'", // Double quote
            '\\' => @"'\'", // Backslash
            '\0' => @"'\0'", // Null
            '\a' => @"'\a'", // Alert
            '\b' => @"'\b'", // Backspace
            '\f' => @"'\f'", // Form feed
            '\n' => @"'\n'", // Newline
            '\r' => @"'\r'", // Carriage return
            '\t' => @"'\t'", // Tab
            '\v' => @"'\v'", // Vertical tab
            '\u00a0' => "'nbsp'", // Non-breaking space
            '\u00ad' => "'shy'", // Soft Hyphen
            _ when Char.IsControl(c: value) => $"'\\u{(int)value:X4}'", // Control character
            _ => $"'{value}'"
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var c = (char)obj;
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "char");
        result.Add(propertyName: "kind", value: "struct");
        // should truncate ' prefix and suffix
        result.Add(propertyName: "value", value: ToRepr(obj: c, context: context)[1..^1]);
        result.Add(propertyName: "unicodeValue", value: $"0x{(int)c:X4}");
        return result;
    }
}

[ReprFormatter(typeof(Rune))]
[ReprOptions(needsPrefix: true)]
internal class RuneFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return $"{(Rune)obj} @ \\U{((Rune)obj).Value:X8}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var rune = (Rune)obj;
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "Rune");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "value", value: rune.ToString());
        result.Add(propertyName: "unicodeValue", value: $"0x{rune.Value:X8}");
        return result;
    }
}