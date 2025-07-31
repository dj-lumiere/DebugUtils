using DebugUtils.Repr.Records;

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{
    public static readonly Dictionary<Type, string> CSharpTypeNames = new()
    {
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

    public static ReprConfig GetContainerConfig(this ReprConfig config)
    {
        return config.ContainerReprMode switch
        {
            ContainerReprMode.UseParentConfig => config,
            ContainerReprMode.UseSimpleFormats => ReprConfig.ContainerDefaults,
            ContainerReprMode.UseCustomConfig => config.CustomContainerConfig ??
                                                 ReprConfig.ContainerDefaults,
            _ => ReprConfig.GlobalDefaults
        };
    }
}