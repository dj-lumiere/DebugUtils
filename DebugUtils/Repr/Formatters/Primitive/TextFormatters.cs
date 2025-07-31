using System.Text;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Primitive;

[ReprFormatter(typeof(string))]
public class StringFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"\"{(string)obj}\"";
    }
}

[ReprFormatter(typeof(char))]
public class CharFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var value = (char)obj;
        switch (value)
        {
            case '\'': return "'''"; // Single quote
            case '\"': return "'\"'"; // Double quote
            case '\\': return @"'\\'"; // Backslash
            case '\0': return @"'\0'"; // Null
            case '\a': return @"'\a'"; // Alert
            case '\b': return @"'\b'"; // Backspace
            case '\f': return @"'\f'"; // Form feed
            case '\n': return @"'\n'"; // Newline
            case '\r': return @"'\r'"; // Carriage return
            case '\t': return @"'\t'"; // Tab
            case '\v': return @"'\v'"; // Vertical tab
            case '\u00a0': return "'nbsp'"; // Non-breaking space
            case '\u00ad': return "'shy'"; // Soft Hyphen
        }

        if (Char.IsControl(c: value))
        {
            return $"'\\u{(int)value:X4}'";
        }

        return $"'{value}'";
    }
}

[ReprFormatter(typeof(Rune))]
public class RuneFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{(Rune)obj} @ \\U{((Rune)obj).Value:X8}";
    }
}