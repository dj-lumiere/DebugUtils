using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class FunctionFormatterTests
{
    public static int Add(int a, int b)
    {
        return a + b;
    }

    internal static long Add2(int a)
    {
        return a;
    }

    private T Add3<T>(T a)
    {
        return a;
    }

    private static void Add4(in int a, out int b)
    {
        b = a + 1;
    }

    private async Task<int> Lambda(int a)
    {
        // Added delay for truly async function.
        // However, this would not do much because it is only used for testing purposes
        // and not being called, only investigated the metadata of it.
        await Task.Delay(millisecondsDelay: 1);
        return a;
    }

    [Fact]
    public void TestFunction()
    {
        var Add5 = (int a) => a + 1;
        var a = Add;
        var b = Add2;
        var c = Add3<short>;
        var d = Add4;
        var e = Lambda;

        Assert.Equal(expected: "internal int Lambda(int a)", actual: Add5.Repr());
        Assert.Equal(expected: "public static int Add(int a, int b)", actual: a.Repr());
        Assert.Equal(expected: "internal static long Add2(int a)", actual: b.Repr());
        Assert.Equal(expected: "private generic short Add3(short a)", actual: c.Repr());
        Assert.Equal(expected: "private static void Add4(in ref int a, out ref int b)",
            actual: d.Repr());
        Assert.Equal(expected: "private async Task<int> Lambda(int a)", actual: e.Repr());
    }
}