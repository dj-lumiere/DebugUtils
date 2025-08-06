using static DebugUtils.CallStack.CallStack;
using Xunit;

namespace DebugUtils.Tests;

public class CallStackTest
{
    [Fact]
    public void TestGetCallerName_Basic()
    {
        var callerName = GetCallerName();
        Assert.Equal("CallStackTest.TestGetCallerName_Basic", callerName);
    }

    private class NestedClass
    {
        public string GetCallerNameFromNested()
        {
            return GetCallerName();
        }
    }

    [Fact]
    public void TestGetCallerName_FromNestedClass()
    {
        var nested = new NestedClass();
        var callerName = nested.GetCallerNameFromNested();
        Assert.Equal("NestedClass.GetCallerNameFromNested", callerName);
    }

    [Fact]
    public void TestGetCallerName_FromLambda()
    {
        var lambdaCaller = new Func<string>(() => GetCallerName());
        var callerName = lambdaCaller();
        // The exact name for lambda can vary based on compiler, but it should contain the test method name
        Assert.Contains("TestGetCallerName_FromLambda", callerName);
    }
}