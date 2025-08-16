using DebugUtils.Repr;
using DebugUtils.Repr.Extensions;
using Xunit;

namespace DebugUtils.Tests;

public class IntegerFormattingTest
{
    [Fact]
    public void TestBinaryFormatting()
    {
        // Test positive values
        Assert.Equal(expected: "0b101010i32",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "B")));
        Assert.Equal(expected: "0b11111111u8",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "B")));
        Assert.Equal(expected: "0b0i32",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "B")));

        // Test negative values
        Assert.Equal(expected: "-0b101010i32",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "B")));
    }

    [Fact]
    public void TestOctalFormatting()
    {
        // Test positive values
        Assert.Equal(expected: "0o52i32",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "O")));
        Assert.Equal(expected: "0o377u8",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "O")));
        Assert.Equal(expected: "0o0i32",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "O")));

        // Test negative values
        Assert.Equal(expected: "-0o52i32",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "O")));
    }

    [Fact]
    public void TestQuaternaryFormatting()
    {
        // Test positive values
        Assert.Equal(expected: "0q222i32",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "Q")));
        Assert.Equal(expected: "0q3333u8",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "Q")));
        Assert.Equal(expected: "0q0i32",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "Q")));

        // Test negative values
        Assert.Equal(expected: "-0q222i32",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "Q")));
    }
}