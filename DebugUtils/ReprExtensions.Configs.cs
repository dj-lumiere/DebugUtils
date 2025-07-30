namespace DebugUtils;

/// <summary>
/// Specifies how floating-point numbers should be represented.
/// </summary>
public enum FloatReprMode
{
    /// <summary>Exact decimal representation without rounding</summary>
    Exact,
    /// <summary>Scientific notation (e.g., 1.23E+4)</summary>
    Scientific,
    /// <summary>Fixed-point notation with specified precision</summary>
    Round,
    /// <summary>Default .NET formatting</summary>
    General,
    /// <summary>Raw bytes as hexadecimal</summary>
    HexBytes,
    /// <summary>IEEE 754 bit field representation (sign|exponent|mantissa)</summary>
    BitField
}

/// <summary>
/// Specifies how integers should be represented.
/// </summary>
public enum IntReprMode
{
    /// <summary>Hexadecimal notation</summary>
    Hex,

    /// <summary>Binary notation</summary>
    Binary,

    /// <summary>Decimal notation</summary>
    Decimal,

    /// <summary>Raw bytes as hexadecimal</summary>
    HexBytes
}

/// <summary>
/// Specifies when types should be represented.
/// </summary>
public enum TypeReprMode
{
    /// <summary>
    /// Always show type info besides null.
    /// </summary>
    AlwaysShow,
    /// <summary>
    /// Hide obvious type names.
    /// Those include Tuples, List, Set, Dictionary, string, char.
    /// </summary>
    HideObvious,
    /// <summary>
    /// Do not show type names.
    /// </summary>
    AlwaysHide
}

/// <summary>
/// Configuration options for controlling how objects are represented in string form.
/// </summary>
/// <param name="FloatMode">Specifies how floating-point numbers should be formatted</param>
/// <param name="FloatPrecision">Number of decimal places for floating-point formatting (when applicable)</param>
/// <param name="ForceFloatModeInContainer">Whether to apply FloatMode when formatting floats inside containers</param>
/// <param name="IntMode">Specifies how integers should be formatted (decimal, hex, binary, etc.)</param>
/// <param name="ForceIntModeInContainer">Whether to apply IntMode when formatting integers inside containers</param>
/// <param name="TypeMode">When true, suppresses type prefixes even for types that normally show them</param>
/// <example>
/// <code>
/// // Exact floating-point representation
/// var exactConfig = new ReprConfig(FloatMode: FloatReprMode.Exact);
/// 
/// // Debug-friendly configuration
/// var debugConfig = new ReprConfig(
///     FloatMode: FloatReprMode.BitField,
///     IntMode: IntReprMode.Hex
/// );
/// 
/// // Use predefined configurations
/// obj.Repr(ReprConfig.GlobalDefaults);
/// obj.Repr(ReprConfig.ContainerDefaults);
/// </code>
/// </example>
public record ReprConfig(
    FloatReprMode FloatMode = FloatReprMode.General,
    int FloatPrecision = 2,
    bool ForceFloatModeInContainer = true,
    IntReprMode IntMode = IntReprMode.Decimal,
    bool ForceIntModeInContainer = true,
    TypeReprMode TypeMode = TypeReprMode.HideObvious
)
{
    public static ReprConfig ContainerDefaults => new(
        FloatMode: FloatReprMode.General,
        FloatPrecision: 2,
        ForceFloatModeInContainer: true,
        IntMode: IntReprMode.Decimal,
        ForceIntModeInContainer: true,
        TypeMode: TypeReprMode.HideObvious);

    public static ReprConfig GlobalDefaults => new(
        FloatMode: FloatReprMode.Exact,
        FloatPrecision: -1,
        ForceFloatModeInContainer: true,
        IntMode: IntReprMode.Decimal,
        ForceIntModeInContainer: true,
        TypeMode: TypeReprMode.HideObvious);
}

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