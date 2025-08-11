# Changelog

# [v1.3.2] Released at 2025-08-11

## ✨ Enhanced Consistency
- All exact representations (Half, Float, Double, Decimal) now use consistent `E+/-YYY` notation
- Format now matches .NET scientific notation conventions (`ToString("E")` style)

## ⚙️ Configuration Changes
- **FloatPrecision in Scientific mode**: Now represents digits after decimal point (matching C# `ToString("E")` behavior)
- **Precision values 100+**: Redirect to exact representation (avoids C# formatting limitations)
- **Precision value negative**: Redirects to exact representation for unlimited precision

# [v1.3.1] Released at 2025.08.11
## 🚀 Performance Improvements
- The current exact float representation engine improved by ~10%

## 🔧 Fixes & Changes
- Decimal zero sign handling normalized (both engines now show positive zero)

## ⚠️ Breaking Changes
- Exact representation of decimal zero now always shows positive sign (was inconsistent)

# [v1.3.0] Released at 2025.08.11

## High-Performance Exact Floating-Point Formatting

## 🚀 Performance Improvements
- **New BigInteger-free exact formatting engine**
- **Up to 2x faster** for worst-case floating-point scenarios
- **5-22% faster** for Half precision values
- **Similar or better performance** across all data types

## 🔧 Implementation Options
- **FloatReprMode.Exact** - New custom arithmetic engine (default)
- **FloatReprMode.Exact_Old** - Original BigInteger-based implementation
- **Identical output** - Both produce exactly the same results

## 💡 Why This Matters
- Better performance for edge cases and complex numbers
- No external dependencies (Unity-ready)
- Future-proof against BigInteger compatibility issues

## 💡 Use Cases
- Use `Exact_Old` for maximum performance on modern .NET
- Use `Exact` for environments where BigInteger might be problematic
- Both implementations produce identical results

## 🔄 API Changes
- `FloatReprMode.Exact` now uses the new high-performance engine
- `FloatReprMode.Exact_Old` preserves the previous BigInteger-based implementation

## 🐛 Bug Fixes
- Exact representation now correctly handles subnormal numbers

## ⚠️ Migration Guide
- No code changes needed - `FloatReprMode.Exact` automatically uses new engine
- Use `FloatReprMode.Exact_Old` if you need the previous behavior
- Performance-critical code should see immediate improvements


## [v1.2.5] Released at 2025.08.09

### 📋 Breaking Changes
- Container types use Container type default even in global setting mode (which is `ContainerMode.UseSimpleFormat`)
- Function parameter type doesn't nest itself and should go straight down to parameter type info itself

## [v1.2.4] Released at 2025.08.09
### 📋 Changes
- ArrayExtensions, DecimalExtensions, FloatExtensions, IntegerExtensions - changed to `DebugUtils.Repr.Extensions` 
  namespace
- FloatInfo, FloatSpec, FunctionDetails, MethodModifiers, ParameterDetails - changed to `DebugUtils.Repr.Models` 
  namespace
- Fixed Repr documentation to correctly reflect the outcome and added formatting to separate code blocks

## [v1.2.3] Released at 2025.08.08

### 📋 Breaking Changes

- **BREAKING**: Simplified namespace structure for cleaner API
    - **Repr**:
        - `DebugUtils.Repr.Formatters.*` → `DebugUtils.Repr.Formatters`
        - All other sub-namespaces simplified similarly
    - **Moved core types to main namespace**:
        - `ReprConfig` and `ReprContext` moved from `DebugUtils.Repr.Records` to `DebugUtils.Repr`
        - These are commonly used types that belong in the main API
    - **Reason**: Directory structure was leaking into public API, making imports verbose and confusing
    - **Migration**: Update your using statements:
  #### Before (v1.2.2):
    ```csharp
    using DebugUtils.CallStack;
    using DebugUtils.Repr;
    using DebugUtils.Repr.Records;
    
    var caller = CallStack.CallStack.GetCallerName();
    var config = new ReprConfig();
    var result = myObject.Repr(config);
    ```

  #### After (v1.2.3):

    ```csharp
    using DebugUtils.CallStack;
    using DebugUtils.Repr;
    
    var caller = CallStack.GetCallerName();
    var config = new ReprConfig();
    var result = myObject.Repr(config);
    
    using DebugUtils.Repr;
    using DebugUtils.Repr.Attributes;
    using DebugUtils.Repr.Interfaces;
    
    [ReprFormatter(typeof(MyType))]
    public class MyTypeFormatter : IReprFormatter
    {
    public string ToRepr(object obj, ReprContext context) // ReprContext in main namespace
    {
    // Implementation
    }
    }
    ```

### Technical Notes

- File locations reorganized for a better structure
- All functionality remains identical—only import statements change
- Preparation for DebugUtils.Unity 1.0.0 release
- Follows .NET naming conventions and common library patterns

## [v1.2.2] Released at 2025.08.07

### ✨ New Features

- Added `Type` type support
- Added `GetCallerInfo` for detailed caller info

## [v1.2.1] Released at 2025.08.07

### 🐛 Bug Fixes & Improvements

- Changed string's hashcode to hexadecimal notation

## [v1.2.0] Released at 2025.08.07

### ✨ New Features

- Added `PriorityQueue<TElement, TPriority>` support with priority-element formatting
- Added `hashCode` property for reference types in ReprTree output
- Collections now include underlying element type information when available
- Arrays now show `dimensions` instead of `length` for better multi-dimensional support

### 🐛 Bug Fixes & Improvements

- Fixed jagged arrays to display inner arrays as proper structured objects
- Changed numeric properties (count, length, rank, dimensions) to use actual integers instead of strings in JsonNode
  output
- Fixed README on main repository to correctly reference
- Fixed README not printing line feed properly

## [v1.1.0] Released at 2025.08.06

### ✨ New Features

- Added Features
    - **Tree Representation (`.ReprTree()`)** - Structured JSON output for debugging tools and IDEs
        - Complete type information for every value
        - Hierarchical object relationships
        - Machine-readable format perfect for analysis tools
        - Pretty printing support with configurable indentation
    - **Custom Formatter System** - Build your own object representations
        - `IReprFormatter` interface for custom string formatting
        - `IReprTreeFormatter` interface for custom JSON tree formatting
        - `ReprFormatterAttribute` for automatic formatter registration
        - `.FormatAsJsonNode()` method for building complex tree structures

- Advanced Configuration & Limits

    - **Comprehensive Limit Controls** - Prevent performance issues with large objects
        - `MaxDepth` - Control recursion depth (supports unlimited with `-1`)
        - `MaxElementsPerCollection` - Limit array/list elements shown
        - `MaxPropertiesPerObject` - Limit object properties displayed
        - `MaxStringLength` - Truncate long strings with character counts
    - **Enhanced Type Display Options**
        - `ShowNonPublicProperties` - Access private fields and properties for deep debugging
        - `EnablePrettyPrintForReprTree` - Enable pretty printing for tree output

### 🔧 API Improvements

- **Two-Tier API Design**
    - End-user API: Simple `ReprConfig` parameters
    - Plugin/Formatter API: Advanced `ReprContext` state management
    - Non-null context enforcement for formatter safety

- **Enhanced Method Signatures**
    - `obj.Repr(config)` - String representation with configuration
    - `obj.ReprTree(config)` - JSON tree with pretty printing options
    - `obj.Repr(context)` - Advanced context control for plugin developers
    - `obj.FormatAsJsonNode(context)` - Building block for custom formatters

### 🐛 Bug Fixes & Improvements

- Fixed double circular reference checking in complex object hierarchies
- Improved nullable struct handling in hierarchical JSON mode
- Enhanced hash code formatting for consistent object identification
- Resolved property counting inconsistencies in object formatters

### 📋 Breaking Changes

- **JSON Tree Output Format** - Tree representation now includes comprehensive metadata
    - Added `type`, `kind` fields for all objects

**Migration Notes:** Existing `.Repr()` calls remain unchanged. New `.ReprTree()` method provides additional
JSON tree functionality. Custom formatters can be gradually adopted using the new interface system.

## [v1.0.3] Released at 2025.08.04

### 🐛 Bug Fixes

- Removed indents and line feeds when in hierarchical mode for nullable structs

## [v1.0.2] Released at 2025.08.04

### 🐛 Bug Fixes

- Fixed double circular reference checking issues
- Removed the experimental status of hierarchical mode
- Removed indents and line feeds when in hierarchical mode
- Added new property "unicodeValue" for char and Rune

## [v1.0.1] Released at 2025.08.01

### ✨ New Features

- Improved JSON hierarchical mode with cleaner circular reference detection
- Better circular reference representation in JSON format

  ### 🐛 Bug Fixes
- Improved hash code formatting consistency

  ### 📚 Documentation
- Clarified experimental feature status

  ### ⚠️ Notes
- Hierarchical JSON mode is experimental and may change in future versions
- For production use, recommend standard string mode: `obj.Repr()`

  ## [v1.0.0] Released at 2025.08.01
  ### 🎉 Initial Release
- Python-style `repr()` for C# objects
- Support for collections, primitives, custom objects
- Configurable number formatting (hex, binary, exact decimals)
- Circular reference detection
- Experimental hierarchical JSON mode