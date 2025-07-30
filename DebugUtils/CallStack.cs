
namespace DebugUtils;
public static class CallStack
{
    /// <summary>
    /// Retrieves information about the calling method using stack trace inspection.
    /// Useful for debugging and logging to identify which method initiated a call.
    /// </summary>
    /// <returns>A string in the format "{ClassName}.{MethodName}" representing the immediate caller 
    /// of the method where this is used. Returns "[unknown]" if caller information cannot be determined.</returns>
    /// <remarks>
    /// This method looks one level up in the call stack, so it will return information
    /// about the method that directly called GetCallerMethod().
    /// </remarks>
    public static string GetCallerMethod()
    {
        try
        {
            var frame = new System.Diagnostics.StackFrame(1);
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
        catch (Exception _)
        {
            return "[error getting caller]";
        }
    }
}