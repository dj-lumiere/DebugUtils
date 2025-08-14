using System.Diagnostics;
using DebugUtils.Repr;
using Xunit;

namespace DebugUtils.Tests;

public class TimeoutTest
{
    // Test class with intentionally slow properties
    public class SlowObject
    {
        public string FastProperty => "I'm fast!";

        public string SlowProperty
        {
            get
            {
                Thread.Sleep(millisecondsTimeout: 5); // 5ms - should timeout with 1ms limit
                return "I'm slow!";
            }
        }

        public string VerySlowProperty
        {
            get
            {
                Thread.Sleep(millisecondsTimeout: 10); // 10ms - definitely should timeout
                return "I'm very slow!";
            }
        }
    }

    [Fact]
    public void TestMemberTimeout_SlowProperty_ShouldTimeout()
    {
        var obj = new SlowObject();
        var config =
            new ReprConfig(MaxMemberTimeMs: 1, ViewMode: MemberReprMode.AllPublic); // 1ms timeout

        var result = obj.Repr(config: config);

        // Should contain timeout indicators for slow properties
        Assert.Contains(expectedSubstring: "SlowProperty: [Timed Out]", actualString: result);
        Assert.Contains(expectedSubstring: "VerySlowProperty: [Timed Out]", actualString: result);
        // But fast property should work fine
        Assert.Contains(expectedSubstring: "FastProperty: \"I'm fast!\"", actualString: result);
    }

    [Fact]
    public void TestMemberTimeout_FastProperty_ShouldNotTimeout()
    {
        var obj = new SlowObject();
        var config =
            new ReprConfig(MaxMemberTimeMs: 100,
                ViewMode: MemberReprMode.AllPublic); // 100ms timeout - plenty of time

        var result = obj.Repr(config: config);

        // All properties should work with generous timeout
        Assert.Contains(expectedSubstring: "FastProperty: \"I'm fast!\"", actualString: result);
        Assert.Contains(expectedSubstring: "SlowProperty: \"I'm slow!\"", actualString: result);
        Assert.Contains(expectedSubstring: "VerySlowProperty: \"I'm very slow!\"",
            actualString: result);

        // Should not contain any timeout indicators
        Assert.DoesNotContain(expectedSubstring: "[Timed Out]", actualString: result);
    }

    [Fact]
    public void TestMemberTimeout_JsonTree_ShouldTimeout()
    {
        var obj = new SlowObject();
        var config = new ReprConfig(MaxMemberTimeMs: 1, ViewMode: MemberReprMode.AllPublic);

        var jsonResult = obj.ReprTree(config: config);
        var jsonString = jsonResult;

        // Should contain timeout indicators in JSON format
        Assert.Contains(expectedSubstring: "[Timed Out]",
            actualString: jsonString); // Note the different spelling in JSON
    }

    // Test class with property that throws exceptions
    public class ErrorObject
    {
        public string GoodProperty => "I work fine";

        public string BadProperty =>
            throw new InvalidOperationException(message: "I always fail!");
    }

    [Fact]
    public void TestMemberError_ShouldCatchException()
    {
        var obj = new ErrorObject();
        var config = new ReprConfig(ViewMode: MemberReprMode.AllPublic);

        var result = obj.Repr(config: config);

        // Should handle exceptions gracefully
        Assert.Equal(
            "ErrorObject(BadProperty: [InvalidOperationException: I always fail!], GoodProperty: \"I work fine\")",
            result);
    }

    public class InfiniteLoopClass
    {
        public string LoopingProperty
        {
            get
            {
                // Infinite loop but not stack overflow - this can be timed out
                while (true)
                {
                    Thread.Sleep(millisecondsTimeout: 1);
                }
            }
        }
    }

    [Fact]
    public void TestMemberTimeout_InfiniteLoop_ShouldTimeout()
    {
        var obj = new InfiniteLoopClass();
        var config =
            new ReprConfig(MaxMemberTimeMs: 10,
                ViewMode: MemberReprMode.AllPublic); // 10ms timeout

        var result = obj.Repr(config: config);

        // Should handle infinite loops by timing out
        Assert.Contains(expectedSubstring: "LoopingProperty: [Timed Out]", actualString: result);
    }
}