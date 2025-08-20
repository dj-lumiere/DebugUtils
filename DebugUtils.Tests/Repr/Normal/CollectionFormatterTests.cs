using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class CollectionFormatterTests
{
    // Collections
    [Fact]
    public void TestListRepr()
    {
        Assert.Equal(expected: "[]", actual: new List<int>().Repr());
        Assert.Equal(expected: "[1_i32, 2_i32, 3_i32]",
            actual: new List<int> { 1, 2, 3 }.Repr());
        Assert.Equal(expected: "[\"a\", null, \"c\"]",
            actual: new List<string?> { "a", null, "c" }.Repr());
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        Assert.Equal(expected: "RangeIterator([1_i32, 2_i32, 3_i32])", actual: Enumerable
           .Range(start: 1, count: 3)
           .Repr());
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        Assert.Equal(expected: "[[1_i32, 2_i32], [3_i32, 4_i32, 5_i32], []]",
            actual: nestedList.Repr());
    }

    // Arrays
    [Fact]
    public void TestArrayRepr()
    {
        Assert.Equal(expected: "1DArray([])", actual: Array.Empty<int>()
                                                           .Repr());
        Assert.Equal(expected: "1DArray([1_i32, 2_i32, 3_i32])",
            actual: new[] { 1, 2, 3 }.Repr());
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new[]
            { new[] { 1, 2 }, new[] { 3 } };
        Assert.Equal(expected: "JaggedArray([[1_i32, 2_i32], [3_i32]])",
            actual: jagged2D.Repr());
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new[,] { { 1, 2 }, { 3, 4 } };
        Assert.Equal(expected: "2DArray([[1_i32, 2_i32], [3_i32, 4_i32]])",
            actual: array2D.Repr());
    }

    // Dictionaries, Sets, Queues
    [Fact]
    public void TestDictionaryRepr()
    {
        var dict = new Dictionary<string, int> { [key: "a"] = 1, [key: "b"] = 2 };
        // Note: Dictionary order is not guaranteed, so we check for both possibilities
        var possibleOutputs = new[]
        {
            "{\"a\": 1_i32, \"b\": 2_i32}",
            "{\"b\": 2_i32, \"a\": 1_i32}"
        };
        Assert.Contains(expected: dict.Repr(), collection: possibleOutputs);
    }

    [Fact]
    public void TestHashSetRepr()
    {
        var set = new HashSet<int> { 1, 2 };
        // Note: HashSet order is not guaranteed, so we sort the string representation for a stable test
        var repr = set.Repr(); // e.g., "{1, 2, 3}"
        var possibleOutputs = new[]
        {
            "{1_i32, 2_i32}",
            "{2_i32, 1_i32}"
        };
        Assert.Contains(expected: repr, collection: possibleOutputs);
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var repr = set.Repr();
        Assert.Equal(expected: "SortedSet({1_i32, 2_i32, 3_i32})", actual: repr);
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue(item: "first");
        queue.Enqueue(item: "second");
        Assert.Equal(expected: "Queue([\"first\", \"second\"])", actual: queue.Repr());
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(item: 1);
        stack.Push(item: 2);
        Assert.Equal(expected: "Stack([2_i32, 1_i32])", actual: stack.Repr());
    }

    #if NET6_0_OR_GREATER
    [Fact]
    public void TestPriorityQueueRepr()
    {
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue(element: "second", priority: 2);
        pq.Enqueue(element: "first", priority: 1);
        pq.Enqueue(element: "third", priority: 3);

        var repr = pq.Repr();
        Assert.Contains(expectedSubstring: "\"first\" (priority: 1_i32)", actualString: repr);
        Assert.Contains(expectedSubstring: "\"second\" (priority: 2_i32)", actualString: repr);
        Assert.Contains(expectedSubstring: "\"third\" (priority: 3_i32)", actualString: repr);
    }
    #endif
    [Fact]
    public void TestTupleRepr()
    {
        Assert.Equal(expected: "(1_i32, \"hello\")", actual: (1, "hello").Repr());
    }
}