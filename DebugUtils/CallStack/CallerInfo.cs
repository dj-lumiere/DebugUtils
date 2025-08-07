namespace DebugUtils.CallStack;

/// <summary>
/// Represents information about the caller of a method, including class name, method name,
/// file name, line number, and column number. Can also represent an error state.
/// </summary>
public readonly struct CallerInfo
{
    /// <summary>
    /// Gets the name of the class where the method was called from, or null if unavailable.
    /// </summary>
    public string? ClassName { get; }

    /// <summary>
    /// Gets the name of the method from the caller's information.
    /// Returns the method name if available; otherwise, null.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets the name of the file in which the caller is located.
    /// Returns null if the information is not available.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the line number in the source file where the caller method is located.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column number in the source file where the caller is located.
    /// This value represents the horizontal position of the calling code in the source file.
    /// </summary>
    public int ColumnNumber { get; }

    /// <summary>
    /// Gets the error message if an error occurred while retrieving caller information.
    /// If this is not null, the other properties may not be reliable.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Indicates whether the caller information is valid and no errors occurred.
    /// Returns true if ErrorMessage is null and key properties are populated; otherwise, false.
    /// </summary>
    public bool IsValid => ErrorMessage == null && ClassName != null && MethodName != null &&
                           FileName != null;

    /// <summary>
    /// Constructor for successful retrieval of caller information.
    /// </summary>
    public CallerInfo(string? className, string? methodName, string? fileName,
        int lineNumber, int columnNumber)
    {
        ClassName = className;
        MethodName = methodName;
        FileName = fileName;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        ErrorMessage = null;
    }

    /// <summary>
    /// Constructor for failed retrieval of caller information.
    /// </summary>
    public CallerInfo(string errorMessage)
    {
        ClassName = null;
        MethodName = null;
        FileName = null;
        LineNumber = 0;
        ColumnNumber = 0;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Converts the caller information to its string representation.
    /// If an error occurred, it returns the error message.
    /// </summary>
    /// <returns>A string representing the caller information or the error.</returns>
    public override string ToString()
    {
        if (ErrorMessage != null)
        {
            return $"[Error getting caller info: {ErrorMessage}]";
        }

        return !IsValid
            ? "[Unknown Caller]"
            : $"{ClassName}.{MethodName}@{FileName}:{LineNumber}:{ColumnNumber}";
    }
}