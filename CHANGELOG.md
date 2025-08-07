# Changelog

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
- Changed numeric properties (count, length, rank, dimensions) to use actual integers instead of strings in JsonNode output
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