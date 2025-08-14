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
        Assert.Equal(expected: "int(0b101010)",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "B")));
        Assert.Equal(expected: "byte(0b11111111)",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "B")));
        Assert.Equal(expected: "int(0b0)",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "B")));

        // Test negative values
        Assert.Equal(expected: "int(-0b101010)",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "B")));
    }

    [Fact]
    public void TestOctalFormatting()
    {
        // Test positive values
        Assert.Equal(expected: "int(0o52)",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "O")));
        Assert.Equal(expected: "byte(0o377)",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "O")));
        Assert.Equal(expected: "int(0o0)",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "O")));

        // Test negative values
        Assert.Equal(expected: "int(-0o52)",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "O")));
    }

    [Fact]
    public void TestQuaternaryFormatting()
    {
        // Test positive values
        Assert.Equal(expected: "int(0q222)",
            actual: 42.Repr(config: new ReprConfig(IntFormatString: "Q")));
        Assert.Equal(expected: "byte(0q3333)",
            actual: ((byte)255).Repr(config: new ReprConfig(IntFormatString: "Q")));
        Assert.Equal(expected: "int(0q0)",
            actual: 0.Repr(config: new ReprConfig(IntFormatString: "Q")));

        // Test negative values
        Assert.Equal(expected: "int(-0q222)",
            actual: (-42).Repr(config: new ReprConfig(IntFormatString: "Q")));
    }
}