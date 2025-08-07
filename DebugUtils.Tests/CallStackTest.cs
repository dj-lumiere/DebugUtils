namespace DebugUtils.Tests;

public class CallStackTest
{
    [Fact]
    public void TestGetCallerName_Basic()
    {
        var callerName = CallStack.CallStack.GetCallerName();
        Assert.Equal(expected: "CallStackTest.TestGetCallerName_Basic",
            actual: callerName);
    }

    private class NestedClass
    {
        public string GetCallerNameFromNested()
        {
            return CallStack.CallStack.GetCallerName();
        }
    }

    [Fact]
    public void TestGetCallerName_FromNestedClass()
    {
        var nested = new NestedClass();
        var callerName = nested.GetCallerNameFromNested();
        Assert.Equal(expected: "NestedClass.GetCallerNameFromNested",
            actual: callerName);
    }

    [Fact]
    public void TestGetCallerName_FromLambda()
    {
        var lambdaCaller = new Func<string>(CallStack.CallStack.GetCallerName);
        var callerName = lambdaCaller();
        // The exact name for lambda can vary based on compiler, but it should contain the test method name
        Assert.Equal(expected: "CallStackTest.TestGetCallerName_FromLambda",
            actual: callerName);
    }

    [Fact]
    public void TestGetCallerInfo_Basic()
    {
        var callerInfo = CallStack.CallStack.GetCallerInfo();
        Assert.Equal(expected: "CallStackTest.TestGetCallerInfo_Basic@CallStackTest.cs:43:9",
            actual: callerInfo.ToString());
    }
}