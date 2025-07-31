namespace DebugUtils;



public static partial class ReprExtensions
{
    public static readonly Dictionary<Type, string> CSharpTypeNames = new()
    {
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(decimal)] = "decimal",
        [typeof(object)] = "object",
    };

    public static readonly Dictionary<Type, string> FriendlyTypeNames = new()
    {
        [typeof(List<>)] = "List",
        [typeof(Dictionary<,>)] = "Dictionary",
        [typeof(HashSet<>)] = "HashSet",
        [typeof(LinkedList<>)] = "LinkedList",
        [typeof(Queue<>)] = "Queue",
        [typeof(Stack<>)] = "Stack",
        [typeof(SortedDictionary<,>)] = "SortedDictionary",
        [typeof(SortedSet<>)] = "SortedSet",
        [typeof(LinkedListNode<>)] = "LinkedListNode",
        [typeof(KeyValuePair<,>)] = "KeyValuePair",
        [typeof(ValueTuple<,>)] = "ValueTuple",
        [typeof(char)] = "char",
        [typeof(string)] = "string",
        [typeof(bool)] = "bool",
    };
}