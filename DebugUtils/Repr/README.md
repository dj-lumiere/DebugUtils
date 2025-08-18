# DebugUtils.Repr for C#

A comprehensive object representation library for C# developers. **Stop wasting time with useless `ToString()` output
and get meaningful debugging information instantly.**

## Core Features

🔍 **`.Repr()`** - See actual content instead of type names  
🌳 **`.ReprTree()`** - Structured JSON-like output for complex analysis  
🔧 **`.FormatAsJsonNode()`** - Build custom tree formatters  
⚡ **Performance-focused** - Built for competitive programming and production debugging    
🎯 **Zero dependencies** - Just add to your project and go  
🔌 **Extensible** - Create custom formatters for your types

## The Problems We Solve

### Useless ToString() Output

```csharp
var arr = new int[] {1, 2, 3, 4};
Console.WriteLine(arr.ToString());  // 😞 "System.Int32[]"

var dict = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
Console.WriteLine(dict.ToString()); // 😞 "System.Collections.Generic.Dictionary`2[System.String,System.Int32]"
```

## The Solutions

### Meaningful Data Representation

```csharp
using DebugUtils.Repr;

var arr = new int[] {1, 2, 3, 4};
Console.WriteLine(arr.Repr());  // 😍 "1DArray([1_i32, 2_i32, 3_i32, 4_i32])"

var dict = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
Console.WriteLine(dict.Repr()); // 😍 "{\"a\": 1_i32, \"b\": 2_i32}"
```

## Features

### 🔍 Object Representation (`.Repr()`)

Works with any type—see actual data instead of useless type names.

### Collections

```csharp
// Arrays (1D, 2D, jagged)
new[] {1, 2, 3}.Repr()                    // 1DArray([1_i32, 2_i32, 3_i32])
new[,] {{1, 2}, {3, 4}}.Repr()              // 2DArray([[1_i32, 2_i32], [3_i32, 4_i32]])
new[][] {{1, 2}, {3, 4, 5}}.Repr()           // JaggedArray([[1_i32, 2_i32], [3_i32, 4_i32, 5_i32]])

// Lists, Sets, Dictionaries
new List<int> {1, 2, 3}.Repr()           // [1_i32, 2_i32, 3_i32]
new HashSet<string> {"a", "b"}.Repr()    // {"a", "b"}
new Dictionary<string, int> {{"x", 1}}.Repr() // {"x": 1_i32}
```

### Numeric Types

```csharp
// Integers with different representations
42.Repr()                                              // 42_i32
42.Repr(new ReprConfig(IntFormatString: "X"))          // 0x2A_i32
42.Repr(new ReprConfig(IntFormatString: "B"))          // 0b101010_i32

// Floating point with exact representation
// You can now recognize the real floating point value
// and find what went wrong when doing arithmetics!
(0.1f + 0.2f).Repr()                       
// 3.00000011920928955078125E-001_f32
0.3f.Repr()                                    
// 2.99999988079071044921875E-001_f32
```

### 🌳 Tree Representation (`.ReprTree()`)

Get structured, JSON-like output perfect for understanding complex object hierarchies:

```csharp
public class Student
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<string> Hobbies { get; set; }
}

var student = new Student { 
    Name = "Alice", 
    Age = 30, 
    Hobbies = new List<string> {"reading", "coding"} 
};

Console.WriteLine(student.ReprTree());
// hashCode can vary depending on when it got executed.
// Output: {
//   "type": "Student",
//   "kind": "class",
//   "hashCode": "0xABCDABCD",
//   "Age": "30_i32",
//   "Hobbies": {
//     "type": "List",
//     "kind": "class",
//     "count": 2,
//     "value": [
//       { "type": "string", "kind": "class", "length": 7, "hashCode" = "0xBBBBBBBB", "value": "reading" },
//       { "type": "string", "kind": "class", "length": 6, "hashCode" = "0xCCCCCCCC", "value": "coding" }
//     ]
//   },
//   "Name": { "type": "string", "kind": "class", "length": 5, "hashCode" = "0xAAAAAAAA", "value": "Alice" },
// }
```

### 🔧 Custom Formatters (`.FormatAsJsonNode()`)

Create your own formatters for specialized types:

```csharp
[ReprFormatter(typeof(Vector3))]
public class Vector3Formatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var v = (Vector3)obj;
        return $"({v.X}, {v.Y}, {v.Z})";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var v = (Vector3)obj;
        return new JsonObject
        {
            ["type"] = "Vector3",
            ["kind"] = "struct",
            ["X"] = v.X.FormatAsJsonNode(context.WithIncrementedDepth()),
            ["Y"] = v.Y.FormatAsJsonNode(context.WithIncrementedDepth()),
            ["Z"] = v.Z.FormatAsJsonNode(context.WithIncrementedDepth())
        };
    }
}
```

**Use cases:**

- **Error tracking** - Know exactly which method failed
- **Performance logging** - Track execution flow
- **Debugging algorithms** - See the call chain in complex recursion
- **Unit testing** - Better test failure messages

## Configuration Options

### Float Formatting (NEW: Format Strings)

```csharp

var scientific = new ReprConfig(FloatFormatString: "E5");
3.14159.Repr(scientific); // Scientific notation with 5 decimal places + _f64 suffix

var rounded = new ReprConfig(FloatFormatString: "F2");
3.14159.Repr(rounded);    // Fixed point with 2 decimal places + _f64 suffix

// Special debugging modes
var hexPower = new ReprConfig(FloatFormatString: "HP");
3.14f.Repr(hexPower);     // IEEE 754 hex power: 0x1.91EB86p+001_f32 (fast bit conversion)

var exact = new ReprConfig(FloatFormatString: "EX");
3.14159f.Repr(exact);     // Exact decimal representation down to very last digit + _f32 suffix
```

### Integer Formatting (NEW: Format Strings)

```csharp
var hex = new ReprConfig(IntFormatString: "X");
255.Repr(hex);            // Hexadecimal: 0xFF_i32

var octal = new ReprConfig(IntFormatString: "O");
255.Repr(octal);          // Octal: 0o377_i32

var quaternary = new ReprConfig(IntFormatString: "Q");
255.Repr(quaternary);     // Quaternary: 0q3333_i32

var binary = new ReprConfig(IntFormatString: "B");
255.Repr(binary);         // Binary: 0b11111111_i32

var decimal = new ReprConfig(IntFormatString: "D");
255.Repr(decimal);        // Standard decimal: 255_i32
```

### Type Display

```csharp
var hideTypes = new ReprConfig(
    TypeMode: TypeReprMode.AlwaysHide,
    ContainerReprMode: ContainerReprMode.UseParentConfig
    );
new[] {1, 2, 3}.Repr(hideTypes);  // [1_i32, 2_i32, 3_i32] (no type prefix to child element.)

var showTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysShow);
new[] {1, 2, 3}.Repr(showTypes);  // 1DArray([1_i32, 2_i32, 3_i32])

// IMPORTANT: Numeric types always show explicit bit-width suffixes (_i32, _f32, _u8, etc.)
// regardless of TypeMode setting. The suffix provides precision/bit-width information
// rather than just type decoration, making it always valuable for debugging.
var numbers = new ReprConfig(TypeMode: TypeReprMode.AlwaysHide);
42.Repr(numbers);     // 42_i32 (suffix always shown)
3.14f.Repr(numbers);  // 3.14_f32 (suffix always shown)
((byte)255).Repr(numbers);  // 255_u8 (suffix always shown)
```

## Real-World Use Cases

### Competitive Programming

Debug algorithms instantly without writing custom debug code:

```csharp
// Debug your DP table
int[,] dp = new int[n, m];
// ... fill DP table ...
Console.WriteLine($"DP table: {dp.Repr()}");

// Track algorithm execution
public int Solve(int[] arr)
{
    Console.WriteLine($"Input: {arr.Repr()}");
    
    var result = ProcessArray(arr);
    Console.WriteLine($"Result: {result.Repr()}");
    return result;
}
```

### Production Debugging

```csharp
public async Task<ApiResponse> ProcessRequest(RequestData request)
{
    logger.Info($"Request: {request.Repr()}");
    
    try 
    {
        var response = await ProcessData(request.Data);
        logger.Info($"Success: {response.Repr()}");
        return response;
    }
    catch (Exception ex)
    {
        logger.Error($"Failed processing: {ex.Message}");
        logger.Error($"Request data: {request.ReprTree()}");  // Full structure for debugging
        throw;
    }
}
```

### Unit Testing

```csharp
[Fact]
public void TestComplexAlgorithm()
{
    var input = GenerateTestData();
    var expected = CalculateExpected(input);
    var actual = MyAlgorithm(input);
    
    // Amazing error messages when tests fail
    Assert.Equal(expected, actual, 
        $"Input: {input.Repr()}\nExpected: {expected.Repr()}\nActual: {actual.Repr()}");
}
```

## API Reference

### Main Methods

```csharp
// String representation (human-readable)
obj.Repr()                           // Uses default config
obj.Repr(config)                     // Uses custom config

// Tree representation (structured JSON-like)
obj.ReprTree()                       // Uses default config  
obj.ReprTree(config)                 // Uses custom config

// JsonNode for custom formatters
obj.FormatAsJsonNode(context)        // For building custom tree structures, should pass context.
```

### Configuration

```csharp
var config = new ReprConfig(
    FloatFormatString: "EX",           // Exact floating-point representation
    IntFormatString: "D",              // Decimal integers
    TypeMode: TypeReprMode.HideObvious,
    MaxDepth: 5,
    MaxItemsPerContainer: 50,
    EnablePrettyPrintForReprTree: true
);

// Use with any method
obj.Repr(config);
obj.ReprTree(config);
```

## Circular Reference Handling

Automatically detects and handles circular references safely:

```csharp
public class Node
{
    public string Name { get; set; }
    public Node Child { get; set; }
    public Node Parent { get; set; }
}

var parent = new Node { Name = "Parent" };
var child = new Node { Name = "Child", Parent = parent };
parent.Child = child;

Console.WriteLine(parent.Repr());
// Output: Node(Child: Node(Child: null, Name: "Child", Parent: <Circular Reference to Node @0x01F550A4>), Name: "Parent", Parent: null)
// (hashCode may vary)
```

## Target Frameworks

- .NET 6.0 (compatible with BOJ and older systems)
- .NET 7.0 (includes Int128 support, compatible with AtCoder)
- .NET 8.0
- .NET 9.0 (compatible with Codeforces)

## Performance

- **Efficient circular reference detection** using RuntimeHelpers.GetHashCode
- **Minimal allocations** for simple representations
- **Automatic cleanup** prevents memory leaks
- **Depth limiting** prevents stack overflow on deep hierarchies

## Contributing

Built to solve real debugging pain points in C# development. If you have ideas for additional features or find bugs,
contributions are welcome!

**Ideas for new features:**

- Additional numeric formatting modes
- Custom attribute-based configuration
- Performance profiling integration
- IDE debugging visualizers

## License

This project follows MIT license.

---

**Stop debugging blind. See your actual data with crystal clarity. 🎯**
