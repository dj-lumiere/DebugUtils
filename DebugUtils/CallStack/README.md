# DebugUtils.CallStack for C#

A lightweight call stack tracking utility for C# developers. **Stop wondering where errors and logs are coming from -
know exactly which method is executing.**

## Core Features

📍 **`CallStack.GetCallerName()`** - Get a simple `ClassName.MethodName` string.  
ℹ️ **`CallStack.GetCallerInfo()`** - Get a detailed `CallerInfo` object with file, line, and column numbers.  
⚡ **Performance-focused** - A hybrid approach balances performance and detail.  
🎯 **Zero dependencies** - Just add to your project and go.  
🔍 **Robust Error Handling** - Gracefully handles failures and reports error messages.

## The Problem We Solve

When an error occurs deep in your application, a simple log message often isn't enough.

```csharp
public class OrderProcessor
{
    private void SaveToDatabase(Order order)
    {
        // Something fails deep in call stack
        throw new Exception("Database connection failed"); // 😞 Where did this come from?
    }
}

// Log Output: Database connection failed
// 😞 Which method? Which class? Which operation?
```

## The Solution

Instantly enrich your logs with precise location information.

### Option 1: Simple Caller Name

Use `GetCallerName()` for a clean, simple `ClassName.MethodName` string.

```csharp
// OrderProcessor.cs
using DebugUtils.CallStack;

public class OrderProcessor
{
    public void ProcessOrder(Order order)
    {
        var caller = CallStack.GetCallerName();
        try
        {
            _logger.Info($"[{caller}] Starting order processing...");
            SaveToDatabase(order);
        }
        catch (Exception ex)
        {
            _logger.Error($"[{caller}] ERROR: {ex.Message}");
            throw;
        }
    }
    
    private void SaveToDatabase(Order order)
    {
        var caller = CallStack.GetCallerName();
        _logger.Info($"[{caller}] Saving to database...");
        throw new Exception("Database connection failed");
    }
}

// Log Output:
// INFO [OrderProcessor.ProcessOrder] Starting order processing...
// INFO [OrderProcessor.SaveToDatabase] Saving to database...
// ERROR [OrderProcessor.ProcessOrder] ERROR: Database connection failed
```

### Option 2: Detailed Caller Info

Use `GetCallerInfo()` to get a rich `CallerInfo` object with the file, line, and column number.

```csharp
// OrderProcessor.cs
using DebugUtils.CallStack;

public class OrderProcessor
{
    public void ProcessOrder(Order order)
    {
        var caller = CallStack.GetCallerInfo();
        try
        {
            _logger.Info($"[{caller}] Starting order processing...");
            SaveToDatabase(order);
        }
        catch (Exception ex)
        {
            // The ToString() override provides a formatted string
            _logger.Error($"[{caller}] ERROR: {ex.Message}");
            throw;
        }
    }
    
    private void SaveToDatabase(Order order)
    {
        var caller = CallStack.GetCallerInfo();
        _logger.Info($"[{caller}] Saving to database...");
        throw new Exception("Database connection failed");
    }
}

// Log Output:
// INFO [OrderProcessor.ProcessOrder@OrderProcessor.cs:8:12] Starting order processing...
// INFO [OrderProcessor.SaveToDatabase@OrderProcessor.cs:21:12] Saving to database...
// ERROR [OrderProcessor.ProcessOrder@OrderProcessor.cs:15:12] ERROR: Database connection failed
// 😍 Now you know the exact file and line where each message came from!
```

## Installation

Add this project as a reference or copy the `CallStack.cs` and `CallerInfo.cs` files to your project.

## API Reference

### `CallStack.GetCallerName()`

```csharp
public static string GetCallerName()
```

**Returns:** A `string` in the format `"ClassName.MethodName"`.

**Possible Return Values:**

- `"MyClass.MyMethod"` - Normal case
- `"[unknown method]"` - When method information cannot be determined
- `"[unknown class].MethodName"` - When class information cannot be determined
- `"[error getting caller: exception message]"` - When an exception occurs during stack inspection

### `CallStack.GetCallerInfo()`

```csharp
public static CallerInfo GetCallerInfo(
    [CallerFilePath] string filePath = "", 
    [CallerLineNumber] int lineNumber = 0)
```

**Returns:** A `CallerInfo` struct containing detailed information about the call site.

**Remarks:** This method uses a hybrid approach. It gets the file path and line number at compile time using
attributes (for high performance) and inspects the `StackTrace` at runtime for the class name, method name, and column
number.

### `CallerInfo` Struct

Represents the information retrieved by `GetCallerInfo()`.

**Properties:**

- `string? ClassName { get; }`
- `string? MethodName { get; }`
- `string? FileName { get; }`
- `int LineNumber { get; }`
- `int ColumnNumber { get; }`
- `string? ErrorMessage { get; }`: If not `null`, an error occurred.
- `bool IsValid { get; }`: `true` if the information is valid and no error occurred.

**`ToString()` Override:**

- **Success:** `"ClassName.MethodName@FileName:LineNumber:ColumnNumber"`
- **Failure:** `"[Unknown Caller]"`
- **Error:** `"[Error getting caller info: The error message]"`

## Performance Considerations

- **`GetCallerName()`**: Uses `StackTrace` for a single frame lookup. It has some overhead and should be used
  judiciously, but is generally safe for logging.
- **`GetCallerInfo()`**: Uses a hybrid approach. It's more expensive than `GetCallerName()` due to the `StackTrace`
  inspection, but provides significantly more detail.
- **Best Practice**: Avoid calling either method in tight, performance-critical loops. For processing collections, call
  it once outside the loop and pass the information down.

```csharp
// ✅ Good: Call once outside the loop
public void ProcessItems(List<Item> items)
{
    var caller = CallStack.GetCallerInfo();
    _logger.Debug($"[{caller}] Processing {items.Count} items...");
    foreach (var item in items)
    {
        ProcessItem(item); 
    }
}

// ❌ Bad: Calling inside a high-frequency loop
for (int i = 0; i < 1000000; i++)
{
    var caller = CallStack.GetCallerInfo(); // Unnecessary overhead!
    ProcessItem(i);
}
```

## Target Frameworks

- .NET 6.0 and higher
- .NET Framework 4.7.2 and higher
- .NET Standard 2.0

## Contributing

If you find bugs or have ideas for improvements, contributions are welcome!

## License

This project follows the MIT license.
