﻿using System.Text;
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
[ReprOptions(needsPrefix: true)]
internal class RuneFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        return $"{(Rune)obj} @ \\U{((Rune)obj).Value:X8}";
    }
}