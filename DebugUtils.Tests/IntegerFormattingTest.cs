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
        Assert.Equal("int(0b101010)", 42.Repr(new ReprConfig(IntFormatString:"B")));
        Assert.Equal("byte(0b11111111)", ((byte)255).Repr(new ReprConfig(IntFormatString:"B")));
        Assert.Equal("int(0b0)", 0.Repr(new ReprConfig(IntFormatString:"B")));
        
        // Test negative values
        Assert.Equal("int(-0b101010)", (-42).Repr(new ReprConfig(IntFormatString:"B")));
    }

    [Fact]
    public void TestOctalFormatting()
    {
        // Test positive values
        Assert.Equal("int(0o52)", 42.Repr(new ReprConfig(IntFormatString:"O")));
        Assert.Equal("byte(0o377)", ((byte)255).Repr(new ReprConfig(IntFormatString:"O")));
        Assert.Equal("int(0o0)", 0.Repr(new ReprConfig(IntFormatString:"O")));
        
        // Test negative values
        Assert.Equal("int(-0o52)", (-42).Repr(new ReprConfig(IntFormatString:"O")));
    }

    [Fact]
    public void TestQuaternaryFormatting()
    {
        // Test positive values
        Assert.Equal("int(0q222)", 42.Repr(new ReprConfig(IntFormatString:"Q")));
        Assert.Equal("byte(0q3333)", ((byte)255).Repr(new ReprConfig(IntFormatString:"Q")));
        Assert.Equal("int(0q0)", 0.Repr(new ReprConfig(IntFormatString:"Q")));
        
        // Test negative values
        Assert.Equal("int(-0q222)", (-42).Repr(new ReprConfig(IntFormatString:"Q")));
    }
}