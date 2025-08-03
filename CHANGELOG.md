# Changelog

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