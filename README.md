# DebugUtils for C#

A collection of debugging utilities for C# developers.

## Core Features

## 🔍 Object Representation ([`Repr`, `ReprTree` Documentation](DebugUtils/Repr/README.md))

Stop getting useless `ToString()` output. See actual object contents.

## 📍 Call Stack Tracking ([`CallStack` Documentation](DebugUtils/CallStack/README.md))

Know exactly where your code is executing and which methods are calling what.

## Installation

Add this project as a reference or copy the source files to your project.

## Quick Start

```csharp
using DebugUtils.CallStack;
using DebugUtils.Repr;

// 🔍 Better object representation
var data = new { Name = "Alice", Age = 30, Scores = new[] {95, 87, 92} };
Console.WriteLine(data.Repr());
// Output: Anonymous(Age: int(30), Name: "Alice", Scores: 1DArray([int(95), int(87), int(92)]))

// 🌳 Structured tree output for complex analysis
Console.WriteLine(data.ReprTree());
// Output: {
//   "type": "Anonymous",
//   "kind": "class",
//   "hashCode": "0xAAAAAAAA",
//   "Age": { "type": "int", "kind": "struct", "value": "30" },
//   "Name": { "type": "string", "kind": "class", "hashCode": "0xBBBBBBBB", "length": 5, "value": "Alice" },
//   "Scores": {
//     "type": "1DArray",
//     "kind": "class",
//     "hashCode": "0xCCCCCCCC",
//     "rank": 1,
//     "dimensions": [3],
//     "elementType": "int",
//     "value": [
//       { "type": "int", "kind": "struct", "value": "95" },
//       { "type": "int", "kind": "struct", "value": "87" },
//       { "type": "int", "kind": "struct", "value": "92" }
//     ]
//   }
// } (hashCode may vary.)

// 📍 Caller tracking for debugging
public class Program
{
    public void MyAlgorithm()
    {
        Console.WriteLine($"[{CallStack.GetCallerName()}] Starting algorithm...");
        
        var result = ComputeResult();
        Console.WriteLine($"[{CallStack.GetCallerInfo()}] Result: {result.Repr()}");
    }
}

// Output: [Program.MyAlgorithm] Starting algorithm...
// Output: [Program.MyAlgorithm@Program.cs:21:8] Result: [int(1), int(4), int(9), int(16), int(25)]
```

## Features

### 🔍 Object Representation (`.Repr()`)

Works with any type - see actual data instead of useless type names.

### Collections

```csharp
// Arrays (1D, 2D, jagged)
new[] {1, 2, 3}.Repr()                    // 1DArray([int(1), int(2), int(3)])
new[,] {{1, 2}, {3, 4}}.Repr()              // 2DArray([[int(1), int(2)], [int(3), int(4)]])
new[][] {{1, 2}, {3, 4, 5}}.Repr()           // JaggedArray([[int(1), int(2)], [int(3), int(4), int(5)]])

// Lists, Sets, Dictionaries
new List<int> {1, 2, 3}.Repr()           // [int(1), int(2), int(3)]
new HashSet<string> {"a", "b"}.Repr()    // {"a", "b"}
new Dictionary<string, int> {{"x", 1}}.Repr() // {"x": int(1)}
```

### Numeric Types

```csharp
// Integers with different representations
42.Repr()                                              // int(42)
42.Repr(new ReprConfig(IntFormatString: "X"))          // int(0x2A)
42.Repr(new ReprConfig(IntFormatString: "B"))          // int(0b101010)
42.Repr(new ReprConfig(IntFormatString: "HB"))         // int(0x0000002A) - hex bytes

// Floating point with exact representation
// You can now recognize the real floating point value
// and find what went wrong when doing arithmetics!
(0.1 + 0.2).Repr()                            
// double(3.00000000000000444089209850062616169452667236328125E-001)
0.3.Repr()                                    
// double(2.99999999999999988897769753748434595763683319091796875E-001)

(0.1 + 0.2).Repr(new ReprConfig(FloatFormatString: "G"))
// double(0.30000000000000004)

// New special formatting modes
3.14f.Repr(new ReprConfig(FloatFormatString: "BF"))    // IEEE 754 bit field
3.14f.Repr(new ReprConfig(FloatFormatString: "HB"))    // Raw hex bytes
```

### 📍 Caller Method Tracking (`GetCallerName()`)

Perfect for logging, error tracking, and debugging call flows:

```csharp
// DataProcessor.cs
public class DataProcessor 
{
    public void ProcessFile(string filename)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Processing file: {filename}");
        
        if (!File.Exists(filename))
        {
            Console.WriteLine($"[{caller}] ERROR: File not found");
            return;
        }
        
        try 
        {
            var data = LoadData(filename);
            Console.WriteLine($"[{caller}] Loaded {data.Length} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{caller}] FAILED: {ex.Message}");
            throw;
        }
    }
}

// Output: [DataProcessor.ProcessFile] Processing file: data.txt
// Output: [DataProcessor.ProcessFile] Loaded 150 records
```

### ℹ️ Detailed Caller Information (`GetCallerInfo()`)

Get the file, line, and column number for even more precise debugging.

```csharp
// DataProcessor.cs
public class DataProcessor 
{
    public void ProcessFile(string filename)
    {
        var caller = CallStack.GetCallerInfo();
        Console.WriteLine($"[{caller}] Processing file: {filename}");
        
        if (!File.Exists(filename))
        {
            Console.WriteLine($"[{caller}] ERROR: File not found");
            return;
        }
    }
}

// Output: [DataProcessor.ProcessFile@DataProcessor.cs:6:8] Processing file: data.txt
```

**Use cases:**

- **Error tracking** - Know the exact line that failed
- **Performance logging** - Pinpoint slow sections of code
- **Debugging algorithms** - Trace execution flow with precision
- **Unit testing** - Get detailed failure locations

## Configuration Options

### Float Formatting (NEW: Format Strings)

```csharp
// NEW APPROACH: Format strings (recommended)
var exact = new ReprConfig(FloatFormatString: "EX");
3.14159.Repr(exact);      // Exact decimal representation down to very last digit

var scientific = new ReprConfig(FloatFormatString: "E5");
3.14159.Repr(scientific); // Scientific notation with 5 decimal places

var rounded = new ReprConfig(FloatFormatString: "F2");
3.14159.Repr(rounded);    // Fixed point with 2 decimal places

// Special debugging modes
var bitField = new ReprConfig(FloatFormatString: "BF");
3.14f.Repr(bitField);     // IEEE 754 bit field: 0|10000000|10010001111010111000011

var hexBytes = new ReprConfig(FloatFormatString: "HB");
3.14f.Repr(hexBytes);     // Raw hex bytes: 0x4048F5C3

// OLD APPROACH: Enum modes (deprecated but still supported)
var oldExact = new ReprConfig(FloatMode: FloatReprMode.Exact);
var oldRounded = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 2);
```

### Integer Formatting (NEW: Format Strings)

```csharp
// NEW APPROACH: Format strings (recommended)
var hex = new ReprConfig(IntFormatString: "X");
255.Repr(hex);            // Hexadecimal: int(0xFF)

var binary = new ReprConfig(IntFormatString: "B");
255.Repr(binary);         // Binary: int(0b11111111)

var hexBytes = new ReprConfig(IntFormatString: "HB");
255.Repr(hexBytes);       // Hex bytes: int(0x000000FF)

var decimal = new ReprConfig(IntFormatString: "D");
255.Repr(decimal);        // Standard decimal: int(255)

// OLD APPROACH: Enum modes (deprecated but still supported)
var oldHex = new ReprConfig(IntMode: IntReprMode.Hex);
var oldBinary = new ReprConfig(IntMode: IntReprMode.Binary);
```

### Type Display

```csharp
var hideTypes = new ReprConfig(
    TypeMode: TypeReprMode.AlwaysHide,
    ContainerReprMode: ContainerReprMode.UseParentConfig
    );
new[] {1, 2, 3}.Repr(hideTypes);  // [1, 2, 3] (no type prefix to child element.)

var showTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysShow);
new[] {1, 2, 3}.Repr(showTypes);  // 1DArray([int(1), int(2), int(3)])
```

### Hierarchical Display

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
var a = new Person(name: "Lumi", age: 28);
a.ReprTree();
//  Output: {
//     "type": "Person",
//     "kind": "class",
//     "hashCode": "0xAAAAAAAA",
//     "Age": {"type": "int", "kind": "struct", "value": "28"},
//     "Name": {"type": "string", "kind": "class", "hashCode": "0xBBBBBBBB", "length": 4, "value": "Lumi"}
// } (hashCode may vary)
```

## Real-World Use Cases

### Competitive Programming

Debug algorithms instantly without writing custom debug code:

```csharp
// Debug your DP table
int[,] dp = new int[n, m];
// ... fill DP table ...
Console.WriteLine($"[{CallStack.GetCallerInfo()}] DP: {dp.Repr()}");

// Track algorithm execution
public int Solve(int[] arr)
{
    Console.WriteLine($"[{CallStack.GetCallerInfo()}] Input: {arr.Repr()}");
    
    var result = ProcessArray(arr);
    Console.WriteLine($"[{CallStack.GetCallerInfo()}] Result: {result}");
    return result;
}
```

### Production Debugging

```csharp
public async Task<ApiResponse> ProcessRequest(RequestData request)
{
    var caller = CallStack.GetCallerInfo();
    logger.Info($"[{caller}] Request: {request.Repr()}");
    
    try 
    {
        var response = await ProcessData(request.Data);
        logger.Info($"[{caller}] Success: {response.Repr()}");
        return response;
    }
    catch (Exception ex)
    {
        logger.Error($"[{caller}] Failed processing: {ex.Message}");
        logger.Error($"[{caller}] Request data: {request.Repr()}");
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
    Console.WriteLine($"[{CallStack.GetCallerInfo()}] Input: {input.Repr()}");
    Console.WriteLine($"[{CallStack.GetCallerInfo()}] Expected: {expected.Repr()}");
    Console.WriteLine($"[{CallStack.GetCallerInfo()}] Actual: {actual.Repr()}");
    
    Assert.Equal(expected, actual);
}
```

## Target Frameworks

- .NET 6.0 (compatible with BOJ and older systems)
- .NET 7.0 (includes Int128 support, compatible with AtCoder)
- .NET 8.0
- .NET 9.0 (compatible with Codeforces)

## Roadmap

**Current Features:**

✅ `.Repr()` - Comprehensive object representation  
✅ `.ReprTree()` - Structured JSON tree output  
✅ `.FormatAsJsonNode()` - Custom formatter building blocks  
✅ `GetCallerName()` - Simple call stack tracking  
✅ `GetCallerInfo()` - Detailed call stack tracking  
✅ **NEW in v1.5:** Format string-based numeric formatting (`FloatFormatString`, `IntFormatString`)  
✅ **NEW in v1.5:** Special debugging modes (`EX`, `HB`, `BF`, `B`)  
✅ **NEW in v1.5:** Enhanced time formatting with tick-level precision  
✅ Multi-framework support (.NET 6-9)  
✅ Zero dependencies  
✅ Circular reference detection  
✅ Custom formatter system

**Planned Features:**

- 🔄 Performance profiling utilities
- 🔄 Memory usage tracking
- 🔄 Advanced logging helpers
- 🔄 Unity debugging extensions
- 🔄 Benchmark timing utilities

*This library started as a solution to competitive programming debugging but is growing into a comprehensive debugging
toolkit.*

## Contributing

Built out of frustration with C# debugging pain points. If you have ideas for additional debugging utilities or find
bugs, contributions are welcome!

**Ideas for new features:**

- Timer/stopwatch utilities for performance testing
- Memory allocation tracking
- Advanced stack trace analysis
- Custom assertion helpers

## License

This project follows MIT license.

---

**Stop debugging blind. See your actual data with crystal clarity and know exactly where your code executes. 🎯**