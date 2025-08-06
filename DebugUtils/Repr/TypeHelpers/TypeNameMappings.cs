namespace DebugUtils.Repr.TypeHelpers;

internal static class TypeNameMappings
{
    /// <summary>
    /// Maps .NET types to their corresponding C# keyword representations.
    /// Used to display types using familiar C# syntax rather than .NET type names.
    /// </summary>
    /// <value>
    /// A dictionary mapping Type objects to their C# keyword strings.
    /// For example, typeof(int) maps to "int", typeof(string) maps to "string".
    /// </value>
    /// <remarks>
    /// This mapping ensures consistent, readable type names in debugging output
    /// that match what developers see in their source code.
    /// </remarks>
    public static readonly Dictionary<Type, string> CSharpTypeNames = new()
    {
        [key: typeof(void)] = "void",
        [key: typeof(byte)] = "byte",
        [key: typeof(sbyte)] = "sbyte",
        [key: typeof(short)] = "short",
        [key: typeof(ushort)] = "ushort",
        [key: typeof(int)] = "int",
        [key: typeof(uint)] = "uint",
        [key: typeof(long)] = "long",
        [key: typeof(ulong)] = "ulong",
        [key: typeof(float)] = "float",
        [key: typeof(double)] = "double",
        [key: typeof(decimal)] = "decimal",
        [key: typeof(object)] = "object"
    };

    /// <summary>
    /// Represents a dictionary mapping generic and specific .NET `Type` objects to more user-friendly type name strings.
    /// </summary>
    /// <remarks>
    /// This dictionary is used to establish meaningful mappings for generic collections (e.g., `List`, `Dictionary`)
    /// and specific types like `char`, `string`, and `bool`. These mappings are particularly helpful when generating
    /// readable representations of types for debugging or display purposes.
    /// </remarks>
    public static readonly Dictionary<Type, string> FriendlyTypeNames = new()
    {
        [key: typeof(List<>)] = "List",
        [key: typeof(Dictionary<,>)] = "Dictionary",
        [key: typeof(HashSet<>)] = "HashSet",
        [key: typeof(LinkedList<>)] = "LinkedList",
        [key: typeof(Queue<>)] = "Queue",
        [key: typeof(Stack<>)] = "Stack",
        [key: typeof(SortedDictionary<,>)] = "SortedDictionary",
        [key: typeof(SortedSet<>)] = "SortedSet",
        [key: typeof(LinkedListNode<>)] = "LinkedListNode",
        [key: typeof(KeyValuePair<,>)] = "KeyValuePair",
        [key: typeof(ValueTuple<,>)] = "ValueTuple",
        [key: typeof(char)] = "char",
        [key: typeof(string)] = "string",
        [key: typeof(bool)] = "bool"
    };
}