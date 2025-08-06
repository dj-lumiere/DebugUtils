# DebugUtils.CallStack for C#

A lightweight call stack tracking utility for C# developers. **Stop wondering where errors and logs are coming from -
know exactly which method is executing.**

## Core Features

📍 **`CallStack.GetCallerName()`** - Know exactly where code is executing  
⚡ **Performance-focused** - Minimal overhead for production use  
🎯 **Zero dependencies** - Just add to your project and go  
🔍 **Error tracking** - Perfect for logging and debugging

## The Problem We Solve

### Mystery Error Locations

```csharp
public class OrderProcessor
{
    public void ProcessOrder(Order order)
    {
        ValidateOrder(order);
        CalculateTotal(order);
        SaveToDatabase(order);
    }
    
    private void SaveToDatabase(Order order)
    {
        // Something fails deep in call stack
        throw new Exception("Database connection failed"); // 😞 Where did this come from?
    }
}

// Output: Database connection failed
// 😞 Which method? Which class? Which operation?
```

## The Solution

### Instant Method Location Tracking

```csharp
using DebugUtils;

public class OrderProcessor
{
    public void ProcessOrder(Order order)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Starting order processing...");
        
        try
        {
            ValidateOrder(order);
            CalculateTotal(order);
            SaveToDatabase(order);
            
            Console.WriteLine($"[{caller}] Order processed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{caller}] ERROR: {ex.Message}");
            throw;
        }
    }
    
    private void SaveToDatabase(Order order)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Saving to database...");
        
        if (connectionString == null)
        {
            Console.WriteLine($"[{caller}] ERROR: No connection string configured");
            throw new Exception("Database connection failed");
        }
        
        // Database save logic...
    }
}

// Output: [OrderProcessor.ProcessOrder] Starting order processing...
// Output: [OrderProcessor.SaveToDatabase] Saving to database...
// Output: [OrderProcessor.SaveToDatabase] ERROR: No connection string configured
// Output: [OrderProcessor.ProcessOrder] ERROR: Database connection failed
// 😍 Now you know exactly where each message came from!
```

## Installation

Add this project as a reference or copy the `CallStack.cs` file to your project.

## Quick Start

```csharp
using DebugUtils;

public class MyClass
{
    public void MyMethod()
    {
        // Get the name of the current method
        var methodName = CallStack.GetCallerName();
        Console.WriteLine($"[{methodName}] Method started");
        
        DoSomeWork();
        
        Console.WriteLine($"[{methodName}] Method completed");
    }
    
    private void DoSomeWork()
    {
        var methodName = CallStack.GetCallerName();
        Console.WriteLine($"[{methodName}] Doing work...");
    }
}

// Output: [MyClass.MyMethod] Method started
// Output: [MyClass.DoSomeWork] Doing work...
// Output: [MyClass.MyMethod] Method completed
```

## Features

### 📍 Method Name Resolution

Get clean, readable method names in the format `ClassName.MethodName`:

```csharp
public class UserService
{
    public async Task<User> GetUserAsync(int userId)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Fetching user {userId}");
        
        // Implementation...
        return user;
    }
}

// Output: [UserService.GetUserAsync] Fetching user 123
```

### 🛡️ Error Handling

Graceful handling when stack information isn't available:

```csharp
// If method info can't be determined:
CallStack.GetCallerName(); // Returns: "[unknown method]"

// If class info can't be determined:
CallStack.GetCallerName(); // Returns: "[unknown class].MethodName"

// If an exception occurs during stack inspection:
CallStack.GetCallerName(); // Returns: "[error getting caller: exception message]"
```

## Real-World Use Cases

### Error Tracking & Logging

Perfect for production logging where you need to know exactly where errors occur:

```csharp
public class PaymentProcessor
{
    private readonly ILogger _logger;
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var caller = CallStack.GetCallerName();
        _logger.Info($"[{caller}] Processing payment for ${request.Amount}");
        
        try
        {
            var validation = ValidatePayment(request);
            if (!validation.IsValid)
            {
                _logger.Warning($"[{caller}] Payment validation failed: {validation.Error}");
                return PaymentResult.Failed(validation.Error);
            }
            
            var result = await ChargeCard(request);
            _logger.Info($"[{caller}] Payment successful: {result.TransactionId}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"[{caller}] Payment processing failed: {ex.Message}");
            throw;
        }
    }
    
    private async Task<PaymentResult> ChargeCard(PaymentRequest request)
    {
        var caller = CallStack.GetCallerName();
        _logger.Debug($"[{caller}] Charging card ending in {request.CardNumber.Substring(12)}");
        
        // Payment gateway logic...
        return result;
    }
}

// Log Output:
// INFO  [PaymentProcessor.ProcessPaymentAsync] Processing payment for $99.99
// DEBUG [PaymentProcessor.ChargeCard] Charging card ending in 1234
// INFO  [PaymentProcessor.ProcessPaymentAsync] Payment successful: TXN-789123
```

### Algorithm Debugging

Track execution flow in complex algorithms:

```csharp
public class GraphAlgorithm
{
    public List<int> FindShortestPath(int start, int end)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Finding path from {start} to {end}");
        
        var visited = new HashSet<int>();
        var path = new List<int>();
        
        if (DFS(start, end, visited, path))
        {
            Console.WriteLine($"[{caller}] Path found: {string.Join(" -> ", path)}");
            return path;
        }
        
        Console.WriteLine($"[{caller}] No path found");
        return new List<int>();
    }
    
    private bool DFS(int current, int target, HashSet<int> visited, List<int> path)
    {
        var caller = CallStack.GetCallerName();
        Console.WriteLine($"[{caller}] Visiting node {current}");
        
        visited.Add(current);
        path.Add(current);
        
        if (current == target)
        {
            Console.WriteLine($"[{caller}] Target {target} reached!");
            return true;
        }
        
        foreach (var neighbor in GetNeighbors(current))
        {
            if (!visited.Contains(neighbor))
            {
                if (DFS(neighbor, target, visited, path))
                    return true;
            }
        }
        
        Console.WriteLine($"[{caller}] Backtracking from {current}");
        path.RemoveAt(path.Count - 1);
        return false;
    }
}

// Output:
// [GraphAlgorithm.FindShortestPath] Finding path from 1 to 5
// [GraphAlgorithm.DFS] Visiting node 1
// [GraphAlgorithm.DFS] Visiting node 2
// [GraphAlgorithm.DFS] Visiting node 4
// [GraphAlgorithm.DFS] Backtracking from 4
// [GraphAlgorithm.DFS] Visiting node 5
// [GraphAlgorithm.DFS] Target 5 reached!
// [GraphAlgorithm.FindShortestPath] Path found: 1 -> 2 -> 5
```

### Unit Testing & Debugging

Better test failure messages and debugging information:

```csharp
[Test]
public void TestComplexCalculation()
{
    var caller = CallStack.GetCallerName();
    
    // Arrange
    var input = GenerateTestData();
    Console.WriteLine($"[{caller}] Test input generated");
    
    // Act
    var result = ComplexCalculation(input);
    Console.WriteLine($"[{caller}] Calculation completed");
    
    // Assert
    var expected = CalculateExpected(input);
    
    if (result != expected)
    {
        Console.WriteLine($"[{caller}] TEST FAILED:");
        Console.WriteLine($"[{caller}] Expected: {expected}");
        Console.WriteLine($"[{caller}] Actual: {result}");
        Console.WriteLine($"[{caller}] Input: {input}");
    }
    
    Assert.AreEqual(expected, result);
}

private decimal ComplexCalculation(decimal input)
{
    var caller = CallStack.GetCallerName();
    Console.WriteLine($"[{caller}] Starting calculation with {input}");
    
    var step1 = ProcessStep1(input);
    Console.WriteLine($"[{caller}] Step 1 result: {step1}");
    
    var step2 = ProcessStep2(step1);
    Console.WriteLine($"[{caller}] Step 2 result: {step2}");
    
    return step2;
}

// Output:
// [TestClass.TestComplexCalculation] Test input generated
// [TestClass.ComplexCalculation] Starting calculation with 100.5
// [TestClass.ComplexCalculation] Step 1 result: 150.75
// [TestClass.ComplexCalculation] Step 2 result: 301.5
// [TestClass.TestComplexCalculation] Calculation completed
```

### Production Monitoring

Track method execution in production systems:

```csharp
public class OrderService
{
    private readonly IMetrics _metrics;
    
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var caller = CallStack.GetCallerName();
        using var timer = _metrics.StartTimer($"{caller}.Duration");
        
        _logger.Info($"[{caller}] Creating order for customer {request.CustomerId}");
        
        try
        {
            var order = await BuildOrder(request);
            await ValidateInventory(order);
            await SaveOrder(order);
            
            _metrics.Increment($"{caller}.Success");
            _logger.Info($"[{caller}] Order {order.Id} created successfully");
            
            return order;
        }
        catch (Exception ex)
        {
            _metrics.Increment($"{caller}.Error");
            _logger.Error($"[{caller}] Order creation failed: {ex.Message}");
            throw;
        }
    }
}

// Metrics automatically tagged with method names:
// orderservice.createorderasync.duration
// orderservice.createorderasync.success
// orderservice.createorderasync.error
```

## API Reference

### CallStack.GetCallerName()

```csharp
public static string GetCallerName()
```

**Returns:** A string in the format `"ClassName.MethodName"` representing the method that called `GetCallerName()`.

**Possible Return Values:**

- `"MyClass.MyMethod"` - Normal case
- `"[unknown method]"` - When method information cannot be determined
- `"[unknown class].MethodName"` - When class information cannot be determined
- `"[error getting caller: exception message]"` - When an exception occurs during stack inspection

**Performance:** Uses `StackFrame(1)` for efficient single-frame lookup. Minimal overhead suitable for production
logging.

## Performance Considerations

- **Lightweight**: Uses `StackFrame(skipFrames: 1)` for efficient caller lookup
- **Production-safe**: Minimal overhead, suitable for production logging
- **Exception-safe**: Gracefully handles cases where stack information isn't available
- **Memory-efficient**: No allocations except for the returned string

## Best Practices

### ✅ Do's

```csharp
// ✅ Use for error tracking
public void ProcessData()
{
    var caller = CallStack.GetCallerName();
    try
    {
        // Your logic
    }
    catch (Exception ex)
    {
        _logger.Error($"[{caller}] Failed: {ex.Message}");
        throw;
    }
}

// ✅ Use for debugging complex flows
public void ComplexAlgorithm()
{
    var caller = CallStack.GetCallerName();
    _logger.Debug($"[{caller}] Starting algorithm");
    // Algorithm steps...
}

// ✅ Use for production monitoring
public async Task<Result> ImportantOperation()
{
    var caller = CallStack.GetCallerName();
    using var timer = _metrics.StartTimer($"{caller}.duration");
    // Operation logic...
}
```

### ❌ Don'ts

```csharp
// ❌ Don't use in tight loops (performance impact)
for (int i = 0; i < 1000000; i++)
{
    var caller = CallStack.GetCallerName(); // Too expensive!
    ProcessItem(i);
}

// ❌ Don't use for normal program flow
public int Add(int a, int b)
{
    var caller = CallStack.GetCallerName(); // Unnecessary overhead
    return a + b;
}

// ✅ Instead, use conditionally or cache the value
public void ProcessItems(List<Item> items)
{
    var caller = CallStack.GetCallerName();
    foreach (var item in items)
    {
        ProcessItem(item, caller); // Pass caller name down
    }
}
```

## Target Frameworks

- .NET 6.0 and higher
- .NET Framework 4.7.2 and higher
- .NET Standard 2.0

## Contributing

If you find bugs or have ideas for improvements to call stack tracking, contributions are welcome!

## License

This project follows MIT license.

---

**Stop wondering where your code is executing. Know exactly which method is running. 🎯**