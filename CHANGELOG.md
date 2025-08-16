# Changelog

# [v1.7.0] Released at 2025.08.14

## 🚀 Major Features

### Consistent Explicit Bit-Width Numeric Suffixes
- **NEW**: All numeric types now use explicit bit-width suffixes for maximum consistency
- **Integer suffixes**: `sbyte=>i8`, `byte=>u8`, `short=>i16`, `ushort=>u16`, `int=>i32`, `uint=>u32`, `long=>i64`, `ulong=>u64`, `Int128=>i128`, `UInt128=>u128`
- **Pointer suffixes**: `IntPtr=>iptr`, `UIntPtr=>uptr` 
- **Float suffixes**: `Half=>f16`, `float=>f32`, `double=>f64`
- **Special types**: `BigInteger=>n`, `decimal=>m` (unchanged)
- **Unity/Cross-platform optimized**: Explicit bit widths are ideal for Unity development and cross-platform debugging
- **Consistent pattern**: All types follow `{i|u|f}{bitwidth}` pattern except special cases

### Benefits
- **Perfect consistency**: No exceptions to the bit-width pattern for standard numeric types
- **Unity-friendly**: Explicit bit widths matter for Unity's cross-platform nature and performance debugging
- **Future-proof**: Scales naturally as new numeric types are added to C#
- **Clear semantics**: Immediately see signedness and size information

### Enhanced ReprOptionsAttribute with TypeAppendMode
- **BREAKING**: `ReprOptionsAttribute` constructor now takes `TypeAppendMode` instead of `bool needsPrefix`
- **NEW**: `TypeAppendMode` enum provides three positioning options:
  - `Prefix` - Traditional "TypeName(content)" format
  - `Suffix` - New "content + type suffix" format (perfect for numeric types like "42i32")
  - `None` - Just content without type information
- **Nullable support ready**: Architecture prepared for nullable type suffix handling (e.g., "42i32?")
- **Improved flexibility**: More precise control over type information positioning

### Safety-First Member Access System
- **NEW**: `MemberReprMode` enum with `ViewMode` parameter replaces boolean flags
- **Safety by default**: Only accesses fields and auto-property backing fields (no risky property getters)
- **Timeout protection**: `MaxMemberTimeMs` parameter with 1ms default timeout for property getters  
- **Exception handling**: Failed property getters show `[ExceptionType: Message]` instead of crashing
- **Fine-grained control** over member visibility:
  - `PublicFieldAutoProperty` - Public fields and auto-property backing fields (default, safe)
  - `AllPublic` - All public fields and properties (with timeout protection when enabled)
  - `AllFieldAutoProperty` - All fields and auto-property backing fields (public and private)
  - `Everything` - All accessible members including computed properties (the highest risk)

### Major Configuration Simplification
- **Massive ReprConfig cleanup**
- **Complete enum removal**: All deprecated `FloatReprMode`, `FloatPrecision`, and `IntMode` enums removed
- **Format string consolidation**: Simplified to `FloatFormatString` and `IntFormatString` only
- **Efficient HexPower mode**: "HP" format string for fast IEEE 754 hexadecimal representation/.net decimal bit 
  representation (simple bit conversion), replacing "BF" mode for more readable bit field representation
- **Streamlined documentation**: Cleaner API surface with comprehensive examples

### Integer Formatting Expansion  
- **Complete implementation**: Binary ("B"), octal ("O"), and quaternary ("Q") formatting fully implemented
- **High-performance bit-grouping algorithm**: New efficient approach processes bits in groups without intermediate lists
- **Consistent byte-based approach**: All formats use the same two's complement logic with proper magnitude conversion
- **Format string support**: "B", "O", "Q", "X" with optional padding (e.g., "B8", "O12", "Q16", "X4")
- **Prefix consistency**: Binary uses "0b", quaternary uses "0q", octal uses "0o", hex uses "0x"

## 📋 Breaking Changes
- **BREAKING**: All numeric type suffixes changed from C# literal style to explicit bit-width style
  - `int: 42` → `42i32`
  - `uint: 42u` → `42u32` 
  - `long: 42L` → `42i64`
  - `ulong: 42UL` → `42u64`
  - `float: 3.14f` → `3.14f32`
  - `double: 3.14d` → `3.14f64`
- **BREAKING**: `ReprOptionsAttribute` constructor changed from `bool needsPrefix` to `TypeAppendMode` parameter
- **BREAKING**: All enum-based formatting completely removed (`FloatReprMode`, `FloatPrecision`, `IntMode`)
- **BREAKING**: Member visibility now controlled via `ViewMode: MemberReprMode` instead of boolean flags
- **BREAKING**: `FormattingMode.Reflection` removed from `ReprFormatterRegistry`  
- **BREAKING**: Integer binary/quaternary/octal/hex formatting now uses format strings instead of enums
- **BREAKING**: Significant API surface reduction in ReprConfig

## 🔧 Code Architecture Improvements
- **File reorganization**: 
  - `DecimalExactExtensions.cs` → `DecimalFormattingExtensions.cs`
  - Deleted `FloatExactExtensions.cs` (functionality merged into `FloatFormattingExtensions.cs`)
  - Deleted `ToStringFormatter.cs` (no longer needed)
  - Deleted `ExactFormatBenchmarkTest.cs` (replaced by new performance tests)
- **New utility classes**: `ObjectExtensions` with compiler-generated name detection
- **Cleaner extension methods**: Simplified integer and float formatting logic
- **Test consolidation**: Updated all tests to use new format string system
- **Culture info support**: Fixed missing culture info injection in integer and float formatters

## 💡 Migration Guide
```csharp
// OLD: Enum-based configuration (removed)
var oldConfig = new ReprConfig(
    ShowNonPublicProperties: true,
    FloatMode: FloatReprMode.Exact,
    IntMode: IntReprMode.Binary
);

// NEW: Format string + visibility mode (recommended)
var newConfig = new ReprConfig(
    ViewMode: MemberReprMode.Everything,
    FloatFormatString: "EX",    // Exact representation
    IntFormatString: "B"        // Binary (also supports "O" octal, "Q" quaternary, "X" hex)
);
```

## 📚 Documentation Updates
- **Updated README**: Reflects new member visibility system and format strings
- **Migration examples**: Clear before/after comparisons for breaking changes
- **Performance benchmarks**: Documentation of new exact formatting performance gains

# [v1.6.0] Released at 2025.08.13
## 📋 Breaking Changes
- Temporarily deleted unlimited property access from Object
- Added support for auto property fields
- **Deterministic member ordering**: Fixed sorting order of fields to ensure consistent output
- Removed ToString fallback even from FormattingMode.Smart

## 🔧 Member Ordering Improvements
**New deterministic ordering strategy** for object representation:
1. **Public fields** (alphabetical by name)
2. **Public auto-properties** (alphabetical by name) 
3. **Private fields** (alphabetical by name, prefixed with "private_")
4. **Private auto-properties** (alphabetical by name, prefixed with "private_")

**Benefits:**
- **Deterministic output**: Same object always produces identical representation
- **Alphabetical sorting**: Within each category, members are sorted alphabetically
- **Safe property access**: Auto-properties accessed via backing fields to avoid side effects
- **Consistent behavior**: Works identically in both `ToRepr()` and `ToReprTree()` methods

**Example:**
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

// Output: ClassifiedData(Age: int(10), Id: long(5), Name: "Alice", Writer: "Bob", 
//                        private_Date: DateTime(...), private_Password: "secret",
//                        private_Data: "info", private_Key: Guid(...))
```

# [v1.5.0] Released at 2025.08.13

## 🚀 Major Features

### Format String-Based Numeric Formatting
- **NEW**: `FloatFormatString` and `IntFormatString` properties in `ReprConfig`
- **Full .NET format string support**: Use standard .NET format strings like `"F2"`, `"E5"`, `"X"`, `"D"`
- **Special debugging modes**:
  - `"EX"` - Exact decimal representation for floating-point precision debugging
  - `"HB"` - Raw hex bytes representation for low-level memory analysis
  - `"BF"` - IEEE 754 bit field analysis (sign|exponent|mantissa)
  - `"B"` - Binary representation for versions less than .NET 8
- **Backward compatibility**: Existing enum-based configuration still works with deprecation warnings

### Enhanced Time Formatting with Tick-Level Precision
- **Improved DateTime/DateTimeOffset formatting**: Now includes milliseconds and sub-millisecond ticks
  - Format: `yyyy.MM.dd HH:mm:ss.fff####` where `fff` = milliseconds, `####` = sub-millisecond ticks
- **Enhanced TimeSpan formatting**: 
  - Compact day notation: `5D 14:30:15.1234567` (5 days format)
  - Negative handling: `-2D-14:30:15.1234567` for negative durations
  - Tick-level precision throughout
- **TimeOnly formatting**: Full precision with `HH:mm:ss.fff####` format
- **Comprehensive tick representation**: Both `totalTicks` and `subTicks` (ticks % 10000) in ReprTree

## 📋 Breaking Changes
- **Deprecated enum-based formatting**: `FloatMode`, `FloatPrecision`, and `IntMode` properties are now obsolete
- **Migration path**: Clear guidance provided in deprecation warnings and documentation
- **No immediate breaking changes**: All existing code continues to work with warnings

## 🔧 API Improvements
- **New properties**: `FloatFormatString` and `IntFormatString` in `ReprConfig`
- **Enhanced default configurations**: `GlobalDefaults` and `ContainerDefaults` now use format strings
- **Comprehensive XML documentation**: Detailed migration guides and format string examples

## 📚 Documentation Updates
- **Updated README files**: Both main and Repr-specific documentation show new format string approach
- **Migration examples**: Side-by-side comparison of old and new approaches
- **Format string reference**: Complete guide to standard and special format modes

## 💡 Use Cases & Examples
```csharp
// NEW: Format string approach (recommended)
42.Repr(new ReprConfig(IntFormatString: "X"))          // int(0x2A)
3.14f.Repr(new ReprConfig(FloatFormatString: "BF"))    // IEEE 754 bit field
DateTime.Now.Repr()                                    // 2025.08.13 15:30:45.1234567 Local

// OLD: Enum approach (deprecated but supported)
42.Repr(new ReprConfig(IntMode: IntReprMode.Hex))      // Still works with warnings
```

# [v1.4.0] Released at 2025.08.12

## ✨ New Features

- Added `Memory<T>`, `ReadOnlyMemory<T>`, `Span<T>`, `ReadOnlySpan<T>`, `Range`, `Index` type support

## 🐛 Bug Fixes
- string gets proper length even after truncation
- StringBuilder properly truncates string in `ReprTree`

# [v1.3.2] Released at 2025.08.11

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