using System.Diagnostics;

namespace DebugUtils.CallStack;

/// <summary>
/// Provides utilities for inspecting the call stack and retrieving caller information.
/// Useful for debugging, logging, and diagnostic purposes.
/// </summary>
public class CallStack
{
    /// <summary>
    /// Retrieves information about the calling method using stack trace inspection.
    /// This method looks one level up in the call stack to identify the immediate caller.
    /// </summary>
    /// <returns>
    /// A string in the format "{ClassName}.{MethodName}@{FileName}:{LineNumber}:{ColumnNumber}"
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
    ///         // Output: "Main.MyMethod@Main.cs:5:8"
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

            var fileName = Path.GetFileName(path: frame?.GetFileName()) ?? "[unknown file]";
            if (fileName == "")
            {
                fileName = "[unknown file]";
            }

            var line = frame?.GetFileLineNumber() ?? 0;
            var column = frame?.GetFileColumnNumber() ?? 0;
            var lineText = $"{line}:{column}";
            if (line == 0 || column == 0)
            {
                lineText = "[unknown line]";
            }

            if (method == null)
            {
                return "[unknown method]";
            }

            if (method.DeclaringType == null)
            {
                return $"[unknown class].{method.Name}@{fileName}:{lineText}";
            }

            return $"{method.DeclaringType.Name}.{method.Name}@{fileName}:{lineText}";
        }
        catch (Exception ex)
        {
            return $"[error getting caller: {ex.Message}]";
        }
    }
    
    
}