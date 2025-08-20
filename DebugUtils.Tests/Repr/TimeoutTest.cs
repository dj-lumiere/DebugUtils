using System.Diagnostics;
using DebugUtils.Repr;
using Xunit;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        var config = ReprConfig.Configure()
                               .MaxMemberTime(1)
                               .ViewMode(MemberReprMode.AllPublic)
                               .Build(); // 1ms timeout

        var result = obj.Repr(config: config);
        var expected = "SlowObject(FastProperty: \"I'm fast!\", SlowProperty: [Timed Out], VerySlowProperty: [Timed Out])";
        Assert.Equal(expected: expected, actual: result);
    }

    [Fact]
    public void TestMemberTimeout_FastProperty_ShouldNotTimeout()
    {
        var obj = new SlowObject();
        var config = ReprConfig.Configure()
                               .MaxMemberTime(100)
                               .ViewMode(MemberReprMode.AllPublic)
                               .Build(); // 100ms timeout - plenty of time

        var result = obj.Repr(config: config);
        var expected = "SlowObject(FastProperty: \"I'm fast!\", SlowProperty: \"I'm slow!\", VerySlowProperty: \"I'm very slow!\")";
        // All properties should work with a generous timeout
        Assert.Equal(expected: expected, actual: result);
    }

    [Fact]
    public void TestMemberTimeout_JsonTree_ShouldTimeout()
    {
        var obj = new SlowObject();
        var config = ReprConfig.Configure()
                               .MaxMemberTime(1)
                               .ViewMode(MemberReprMode.AllPublic)
                               .Build();

        var jsonResult = obj.ReprTree(config: config);

        // Parse the JSON result
        var jsonNode = JsonNode.Parse(jsonResult);
        Assert.NotNull(jsonNode);

        // Verify the root object structure
        Assert.Equal("SlowObject", jsonNode["type"]
          ?.ToString());
        Assert.Equal("class", jsonNode["kind"]
          ?.ToString());
        Assert.NotNull(jsonNode["hashCode"]);

        // Verify FastProperty - should work fine
        var fastProp = jsonNode["FastProperty"];
        Assert.NotNull(fastProp);
        Assert.Equal("string", fastProp["type"]
          ?.ToString());
        Assert.Equal("class", fastProp["kind"]
          ?.ToString());
        Assert.Equal("I'm fast!", fastProp["value"]
          ?.ToString());
        Assert.Equal(9, fastProp["length"]
          ?.GetValue<int>());

        // Verify SlowProperty - should be timed out
        var slowProp = jsonNode["SlowProperty"];
        Assert.NotNull(slowProp);
        Assert.Equal("[Timed Out]", slowProp.ToString());

        // Verify VerySlowProperty - should be timed out
        var verySlowProp = jsonNode["VerySlowProperty"];
        Assert.NotNull(verySlowProp);
        Assert.Equal("[Timed Out]", verySlowProp.ToString());
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
        var config = ReprConfig.Configure()
                               .ViewMode(MemberReprMode.AllPublic)
                               .MaxMemberTime(5)
                               .Build();

        var result = obj.Repr(config: config);

        // Should handle exceptions gracefully
        Assert.Equal(expected: "ErrorObject(BadProperty: [InvalidOperationException: I always fail!], GoodProperty: \"I work fine\")", actual: result);
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
        var config = ReprConfig.Configure()
                               .MaxMemberTime(10)
                               .ViewMode(MemberReprMode.AllPublic)
                               .Build(); // 10ms timeout

        var result = obj.Repr(config: config);

        // Should handle infinite loops by timing out
        Assert.Equal("InfiniteLoopClass(LoopingProperty: [Timed Out])", result);
    }
}