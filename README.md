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
// Output: Anonymous(Age: 30_i32, Name: "Alice", Scores: 1DArray([95_i32, 87_i32, 92_i32]))

// 🌳 Structured tree output for complex analysis
Console.WriteLine(data.ReprTree());
// Output: {
//   "type": "Anonymous",
//   "kind": "class",
//   "hashCode": "0xAAAAAAAA",
//   "Age": 30_i32,
//   "Name": { "type": "string", "kind": "class", "hashCode": "0xBBBBBBBB", "length": 5, "value": "Alice" },
//   "Scores": {
//     "type": "1DArray",
//     "kind": "class",
//     "hashCode": "0xCCCCCCCC",
//     "rank": 1,
//     "dimensions": [3],
//     "elementType": "int",
//     "value": [
//       "95_i32",
//       "87_i32",
//       "92_i32"
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
// Output: [Program.MyAlgorithm@Program.cs:21:8] Result: [1_i32, 4_i32, 9_i32, 16_i32, 25_i32]
```

## Features

### 🔍 Object Representation (`.Repr()`)

Works with any type - see actual data instead of useless type names.

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
// Integers with explicit bit-width suffixes
42.Repr()                                              // 42_i32
((byte)42).Repr()                                      // 42_u8
((long)42).Repr()                                      // 42_i64
42u.Repr()                                             // 42_u8

// Integers with different representations
42.Repr(new ReprConfig(IntFormatString: "X"))          // 0x2A_i32
42.Repr(new ReprConfig(IntFormatString: "O"))          // 0o52_i32
42.Repr(new ReprConfig(IntFormatString: "Q"))          // 0q222_i32
42.Repr(new ReprConfig(IntFormatString: "B"))          // 0b101010_i32

// Floating point with explicit bit-width suffixes and exact representation
// You can now recognize the real floating point value
// and find what went wrong when doing arithmetics!
(0.1 + 0.2).Repr()                            
// 3.00000000000000444089209850062616169452667236328125E-001_f64
0.3.Repr()                                    
// 2.99999999999999988897769753748434595763683319091796875E-001_f64

// Float types with explicit suffixes
3.14f.Repr()                                           // 3.14_f32
3.14.Repr()                                            // 3.14_f64  
((Half)3.14f).Repr()                                   // 3.140625_f16
3.14m.Repr()                                           // 3.14_m

// New special formatting modes
3.14f.Repr(new ReprConfig(FloatFormatString: "EX"))    // Exact number with _f32 suffix
3.14f.Repr(new ReprConfig(FloatFormatString: "HP"))    // Hex Power mode with _f32 suffix
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

### Member Safety & Ordering (v1.7)

**SAFETY FIRST**: DebugUtils prioritizes safety in object representation:

- **Safe by default**: Only accesses fields and auto-property backing fields (no property getters)
- **Optional property access**: Enable via `ViewMode: MemberReprMode.AllPublic` with 1ms timeout protection
- **Timeout protection**: Property getters exceeding `MaxMemberTimeMs` are marked as `[Timed Out]`
- **Exception handling**: Failed property getters show `[ExceptionType: Message]` instead of crashing
- **Production safety**: Set `MaxMemberTimeMs: 0` to disable all property getters entirely

**Member Ordering (v1.6)**

For object representation, DebugUtils uses deterministic alphabetical ordering within member categories:

1. **Public fields** (alphabetical by name)
2. **Public auto-properties** (alphabetical by name)
3. **Private fields** (alphabetical by name, prefixed with "private_")
4. **Private auto-properties** (alphabetical by name, prefixed with "private_")

```csharp
public class ClassifiedData
{
    public long Id = 5;                    // Category 1: Public field
    public int Age = 10;                   // Category 1: Public field
    public string Writer { get; set; }     // Category 2: Public auto-property
    public string Name { get; set; }       // Category 2: Public auto-property
    private DateTime Date = DateTime.Now;  // Category 3: Private field
    private string Password = "secret";    // Category 3: Private field
    private string Data { get; set; }      // Category 4: Private auto-property
    private Guid Key { get; set; }         // Category 4: Private auto-property
}

// Output with ViewMode: MemberReprMode.AllFieldAutoProperty
// ClassifiedData(Age: 10_i32, Id: 5_i64, Name: "Alice", Writer: "Bob", 
//                private_Date: DateTime(...), private_Password: "secret",
//                private_Data: "info", private_Key: Guid(...))
```

This ordering ensures deterministic output while grouping similar member types together. Auto-properties are accessed
via their backing fields to avoid potential side effects from getter calls.

**Safety Configuration:**

```csharp
// SAFE: Only fields and auto-properties (recommended for production)
var safeConfig = new ReprConfig(MaxMemberTimeMs: 0);

// GUARDED: Include properties with timeout protection (good for debugging)
var debugConfig = new ReprConfig(
    MaxMemberTimeMs: 1, 
    ViewMode: MemberReprMode.AllPublic
);

// COMPREHENSIVE: All members with longer timeout (development only)
var devConfig = new ReprConfig(
    MaxMemberTimeMs: 100, 
    ViewMode: MemberReprMode.Everything
);
```

## Configuration Options

### Float Formatting (NEW: Format Strings)

```csharp

var scientific = new ReprConfig(FloatFormatString: "E5");
3.14159.Repr(scientific); // Scientific notation with 5 decimal places + f64 suffix

var rounded = new ReprConfig(FloatFormatString: "F2");
3.14159.Repr(rounded);    // Fixed point with 2 decimal places + f64 suffix

// Special debugging mode
var exact = new ReprConfig(FloatFormatString: "EX");
3.14159.Repr(exact);      // Exact decimal representation down to very last digit + f64 suffix

var hexPower = new ReprConfig(FloatFormatString: "HP");
3.14f.Repr(hexPower);     // IEEE 754 hex power: 0x1.91EB86p+001_f32 (fast bit conversion)

```

### Integer Formatting

```csharp

var dec = new ReprConfig(IntFormatString: "D");
255.Repr(dec);        // Standard decimal: 255_i32

// Below all of them use efficient bit-grouping algorithms - no performance overhead
var hex = new ReprConfig(IntFormatString: "X");
255.Repr(hex);            // Hexadecimal: 0xFF_i32

var binary = new ReprConfig(IntFormatString: "B");
255.Repr(binary);         // Binary: 0b11111111_i32

var octal = new ReprConfig(IntFormatString: "O");
255.Repr(octal);          // Octal: 0o377_i32

var quaternary = new ReprConfig(IntFormatString: "Q");
255.Repr(quaternary);     // Quaternary: 0q3333_i32
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
//     "Age": "28_i32",
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