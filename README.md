# DebugUtils

A comprehensive debugging toolkit for C# developers. **Stop wasting time with useless `ToString()` output and unclear error logs.**

## Core Features

🔍 **`.Repr()`** - See actual content instead of type names  
📍 **`GetCallerMethod()`** - Know exactly where errors/logs originated  
⚡ **Performance-focused** - Built for competitive programming and production debugging  
🎯 **Zero dependencies** - Just add to your project and go

## The Problems We Solve

### 1. Useless ToString() Output
```csharp
var arr = new int[] {1, 2, 3, 4};
Console.WriteLine(arr.ToString());  // 😞 "System.Int32[]"

var dict = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
Console.WriteLine(dict.ToString()); // 😞 "System.Collections.Generic.Dictionary`2[System.String,System.Int32]"
```

### 2. Mystery Error Locations
```csharp
public void ProcessData()
{
    // Something fails deep in call stack
    throw new Exception("Data processing failed"); // 😞 Where did this come from?
}
```

## The Solutions

### 1. Meaningful Data Representation
```csharp
using ProblemSolving.DebugUtils;

var arr = new int[] {1, 2, 3, 4};
Console.WriteLine(arr.Repr());  // 😍 "[1, 2, 3, 4]"

var dict = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
Console.WriteLine(dict.Repr()); // 😍 "{"a": 1, "b": 2}"
```

### 2. Instant Error Location Tracking
```csharp
using ProblemSolving.DebugUtils;

public void ProcessData()
{
    var caller = DebugUtils.GetCallerMethod();
    Console.WriteLine($"[{caller}] Processing started...");
    
    try 
    {
        // Your code here
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{caller}] Error: {ex.Message}");
        throw;
    }
}

// Output: [MainClass.RunAlgorithm] Processing started...
// Output: [MainClass.RunAlgorithm] Error: Invalid input format
```

## Installation

Add this project as a reference or copy the source files to your project.

## Quick Start

```csharp
using ProblemSolving.DebugUtils;

// 🔍 Better object representation
var data = new { Name = "Alice", Age = 30, Scores = new[] {95, 87, 92} };
Console.WriteLine(data.Repr());
// Output: { Name: "Alice", Age: 30, Scores: 1DArray([95, 87, 92]) }

// 📍 Caller tracking for debugging
public void MyAlgorithm()
{
    Console.WriteLine($"[{DebugUtils.GetCallerMethod()}] Starting algorithm...");
    
    var result = ComputeResult();
    Console.WriteLine($"[{DebugUtils.GetCallerMethod()}] Result: {result.Repr()}");
}

// Output: [Program.Main] Starting algorithm...  
// Output: [Program.Main] Result: [1, 4, 9, 16, 25]
```

## Features

### 🔍 Object Representation (`.Repr()`)

Works with any type - see actual data instead of useless type names.

### Collections
```csharp
// Arrays (1D, 2D, jagged)
new[] {1, 2, 3}.Repr()                    // 1DArray([1, 2, 3])
new[,] {{1,2}, {3,4}}.Repr()              // 2DArray([[1, 2], [3, 4]])
new[][] {{1,2}, {3,4,5}}.Repr()           // 3DArray([[1, 2], [3, 4, 5]])

// Lists, Sets, Dictionaries
new List<int> {1, 2, 3}.Repr()           // [1, 2, 3]
new HashSet<string> {"a", "b"}.Repr()    // {"a", "b"}
new Dictionary<string, int> {{"x", 1}}.Repr() // {"x": 1}
```

### Numeric Types
```csharp
// Integers with different representations
42.Repr()                                              // int(42)
42.Repr(new ReprConfig(IntMode: IntReprMode.Hex))      // int(0x2A)
42.Repr(new ReprConfig(IntMode: IntReprMode.Binary))   // int(0b101010)

// Floating point with exact representation
(0.1 + 0.2).Repr()                            // double(3.0000000000000004E-1)
(0.1 + 0.2).Repr(new ReprConfig(FloatMode: FloatReprMode.General))  // 0.30000000000000004
```

### 📍 Caller Method Tracking (`GetCallerMethod()`)

Perfect for logging, error tracking, and debugging call flows:

```csharp
public class DataProcessor 
{
    public void ProcessFile(string filename)
    {
        var caller = DebugUtils.GetCallerMethod();
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

// When called from Main():
// Output: [Program.Main] Processing file: data.txt
// Output: [Program.Main] Loaded 150 records
```

**Use cases:**
- **Error tracking** - Know exactly which method failed
- **Performance logging** - Track execution flow
- **Debugging algorithms** - See the call chain in complex recursion
- **Unit testing** - Better test failure messages

## Configuration Options

### Float Formatting
```csharp
var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
3.14159.Repr(config);     // Exact decimal representation

var scientific = new ReprConfig(FloatMode: FloatReprMode.Scientific);  
3.14159.Repr(scientific); // Scientific notation

var rounded = new ReprConfig(FloatMode: FloatReprMode.Round, FloatPrecision: 2);
3.14159.Repr(rounded);    // Rounded to 2 decimal places
```

### Integer Formatting
```csharp
var hex = new ReprConfig(IntMode: IntReprMode.Hex);
255.Repr(hex);            // Hexadecimal Representation

var binary = new ReprConfig(IntMode: IntReprMode.Binary);
255.Repr(binary);         // Binary Representation

var bytes = new ReprConfig(IntMode: IntReprMode.HexBytes);
255.Repr(bytes);          // Bytestream representation
```

### Type Display
```csharp
var hideTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysHide);
new[] {1, 2, 3}.Repr(hideTypes);  // [1, 2, 3] (no type prefix)

var showTypes = new ReprConfig(TypeMode: TypeReprMode.AlwaysShow);
new[] {1, 2, 3}.Repr(showTypes);  // 1DArray([1, 2, 3])
```

## Real-World Use Cases

### Competitive Programming
Debug algorithms instantly without writing custom debug code:

```csharp
// Debug your DP table
int[,] dp = new int[n, m];
// ... fill DP table ...
Console.WriteLine($"[{DebugUtils.GetCallerMethod()}] DP: {dp.Repr()}");

// Track algorithm execution
public int Solve(int[] arr)
{
    Console.WriteLine($"[{DbgUtils.GetCallerMethod()}] Input: {arr.Repr()}");
    
    var result = ProcessArray(arr);
    Console.WriteLine($"[{DbgUtils.GetCallerMethod()}] Result: {result}");
    return result;
}
```

### Production Debugging
```csharp
public async Task<ApiResponse> ProcessRequest(RequestData request)
{
    var caller = DbgUtils.GetCallerMethod();
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
    Console.WriteLine($"[{DbgUtils.GetCallerMethod()}] Input: {input.Repr()}");
    Console.WriteLine($"[{DbgUtils.GetCallerMethod()}] Expected: {expected.Repr()}");
    Console.WriteLine($"[{DbgUtils.GetCallerMethod()}] Actual: {actual.Repr()}");
    
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
- ✅ `.Repr()` - Comprehensive object representation
- ✅ `GetCallerMethod()` - Call stack tracking
- ✅ Multi-framework support (.NET 6-9)
- ✅ Zero dependencies

**Planned Features:**
- 🔄 Performance profiling utilities
- 🔄 Memory usage tracking
- 🔄 Advanced logging helpers
- 🔄 Unity debugging extensions
- 🔄 Benchmark timing utilities

*This library started as a solution to competitive programming debugging but is growing into a comprehensive debugging toolkit.*

## Contributing

Built out of frustration with C# debugging pain points. If you have ideas for additional debugging utilities or find bugs, contributions are welcome!

**Ideas for new features:**
- Timer/stopwatch utilities for performance testing
- Memory allocation tracking
- Advanced stack trace analysis
- Custom assertion helpers

## License

This project follows MIT license.

---

**Stop debugging blind. See your actual data and know where your code fails. 🎯**