using System.Text;
using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprFormatter(typeof(string))]
[ReprOptions(needsPrefix: false)]
internal class StringFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"\"{(string)obj}\"";
    }
}

[ReprFormatter(typeof(StringBuilder))]
[ReprOptions(needsPrefix: true)]
internal class StringBuilderFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{((StringBuilder)obj).ToString()}";
    }
}

[ReprFormatter(typeof(char))]
[ReprOptions(needsPrefix: false)]
internal class CharFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var value = (char)obj;
        return value switch
        {
            '\'' => "'''", // Single quote
            '\"' => "'\"'", // Double quote
            '\\' => @"'\\'", // Backslash
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
}

[ReprFormatter(typeof(Rune))]
[ReprOptions(needsPrefix: true)]
internal class RuneFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{(Rune)obj} @ \\U{((Rune)obj).Value:X8}";
    }
}