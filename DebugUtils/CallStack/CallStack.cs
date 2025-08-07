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
    /// A string in the format "{ClassName}.{MethodName}" representing the immediate caller
    /// of the method where this is used. Returns descriptive error messages if caller 
    /// information cannot be determined.
    /// </returns>
    /// <remarks>
    /// <para>This method performs stack frame inspection to determine the calling method.</para>
    /// <para>Performance note: Stack trace inspection has overhead and should be used judiciously.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Main
    /// {
    ///     public void MyMethod()
    ///     {
    ///         Console.WriteLine(CallStack.GetCallerName()); // Output: "Main.MyMethod"
    ///     }
    /// }
    /// </code>
    /// </example>
    public static string GetCallerName()
    {
        try
        {
            var frame = new StackFrame(skipFrames: 1);
            var method = frame.GetMethod();

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
}