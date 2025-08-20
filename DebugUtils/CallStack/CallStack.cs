using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DebugUtils;

/// <summary>
/// Provides utilities for inspecting the call stack and retrieving caller information.
/// Useful for debugging, logging, and diagnostic purposes.
/// </summary>
public static class CallStack
{
    /// <summary>
    /// Retrieves information about the calling method using stack trace inspection.
    /// This method looks one level up in the call stack to identify the immediate caller.
    /// </summary>
    /// <returns>
    /// A string in the format "{ClassName}.{MethodName}"
    /// representing the immediate caller of the method where this is used. Returns descriptive
    /// error messages if caller information cannot be determined.
    /// </returns>
    /// <remarks>
    /// <para>This method performs stack frame inspection to determine the calling method.</para>
    /// <para>Performance note: Stack trace inspection has overhead and should be used judiciously.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Main.cs
    /// public class Main
    /// {
    ///     public void MyMethod()
    ///     {
    ///         Console.WriteLine(CallStack.GetCallerName());
    ///         // Output: "Main.MyMethod"
    ///         // or [unknown method] if stack info isn't available.
    ///     }
    /// }
    /// </code>
    /// </example>
    public static string GetCallerName()
    {
        try
        {
            var stack = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
            var frame = stack.GetFrame(index: 0);
            var method = frame?.GetMethod();


            if (method == null)
            {
                return "[unknown method]";
            }

            if (method.DeclaringType == null)
            {
                return $"[unknown class].{method.Name}";
            }

            return $"{method.DeclaringType.Name}.{method.Name}";
        }
        catch (Exception ex)
        {
            return $"[error getting caller: {ex.Message}]";
        }
    }

    /// <summary>
    /// Retrieves detailed information about the calling method using a hybrid approach.
    /// It uses Caller Info Attributes for file and line numbers, and StackTrace for class, method, and column.
    /// </summary>
    /// <returns>
    /// A CallerInfo object containing the details of the call site.
    /// If an error occurs, the returned object's IsValid will be false and ErrorMessage will be set.
    /// </returns>
    /// <remarks>
    /// This method provides a balance of performance and detail. File and line info are retrieved at compile time,
    /// while the rest of the info is retrieved at runtime. Debug symbols (PDB files) are required for full accuracy.
    /// </remarks>
    public static CallerInfo GetCallerInfo([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var stack = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
            var frame = stack.GetFrame(index: 0);
            var method = frame?.GetMethod();
            var className = method?.DeclaringType?.Name;
            var methodName = method?.Name;
            var column = frame?.GetFileColumnNumber() ?? 0;

            var result = new CallerInfo(className: className, methodName: methodName,
                fileName: Path.GetFileName(path: filePath), lineNumber: lineNumber,
                columnNumber: column);

            return result;
        }
        catch (Exception ex)
        {
            return new CallerInfo(errorMessage: ex.Message);
        }
    }
}