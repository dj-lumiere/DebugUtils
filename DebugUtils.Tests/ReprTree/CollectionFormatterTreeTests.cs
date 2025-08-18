using System.Text.Json.Nodes;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class CollectionFormatterTreeTests
{
    [Fact]
    public void TestListRepr()
    {
        // Test with an empty list
        var actualJson = JsonNode.Parse(json: new List<int>().ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 0, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Empty(collection: actualJson[propertyName: "value"]!.AsArray());

        // Test with a list of integers
        actualJson = JsonNode.Parse(json: new List<int> { 1, 2, 3 }.ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.Equal(expected: "1_i32", actual: valueArray[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32", actual: valueArray[index: 1]!.GetValue<string>());
        Assert.Equal(expected: "3_i32", actual: valueArray[index: 2]!.GetValue<string>());

        // Test with a list of nullable strings
        actualJson = JsonNode.Parse(json: new List<string?> { "a", null, "c" }.ReprTree())!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);

        // Check first element: "a"
        var item1 = valueArray[index: 0]!.AsObject();
        Assert.Equal(expected: "string", actual: item1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item1[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: item1[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "a", actual: item1[propertyName: "value"]
          ?.ToString());

        // Check second element: null
        Assert.True(condition: JsonNode.DeepEquals(null, node2: valueArray[index: 1]));

        // Check third element: "c"
        var item3 = valueArray[index: 2]!.AsObject();
        Assert.Equal(expected: "string", actual: item3[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item3[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item3[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: item3[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "c", actual: item3[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestEnumerableRepr()
    {
        var actualJson = JsonNode.Parse(json: Enumerable.Range(start: 1, count: 3)
                                                        .ReprTree())!;
        Assert.Equal(expected: "RangeIterator", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.Equal(expected: "1_i32", actual: valueArray[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32", actual: valueArray[index: 1]!.GetValue<string>());
        Assert.Equal(expected: "3_i32", actual: valueArray[index: 2]!.GetValue<string>());
    }

    [Fact]
    public void TestNestedListRepr()
    {
        var nestedList = new List<List<int>> { new() { 1, 2 }, new() { 3, 4, 5 }, new() };
        var actualJson = JsonNode.Parse(json: nestedList.ReprTree())!;

        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: outerArray.Count);

        // Check first nested list: { 1, 2 }
        var nested1 = outerArray[index: 0]!.AsObject();
        Assert.Equal(expected: "List", actual: nested1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested1[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: nested1[propertyName: "count"]!.GetValue<int>());
        var nested1Value = nested1[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: nested1Value.Count);
        Assert.Equal(expected: "1_i32", actual: nested1Value[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32", actual: nested1Value[index: 1]!.GetValue<string>());

        // Check second nested list: { 3, 4, 5 }
        var nested2 = outerArray[index: 1]!.AsObject();
        Assert.Equal(expected: "List", actual: nested2[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested2[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested2[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: nested2[propertyName: "count"]!.GetValue<int>());
        var nested2Value = nested2[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: nested2Value.Count);
        Assert.Equal(expected: "3_i32", actual: nested2Value[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "4_i32", actual: nested2Value[index: 1]!.GetValue<string>());
        Assert.Equal(expected: "5_i32", actual: nested2Value[index: 2]!.GetValue<string>());

        // Check third nested list: { }
        var nested3 = outerArray[index: 2]!.AsObject();
        Assert.Equal(expected: "List", actual: nested3[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nested3[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nested3[propertyName: "hashCode"]);
        Assert.Equal(expected: 0, actual: nested3[propertyName: "count"]!.GetValue<int>());
        Assert.Empty(collection: nested3[propertyName: "value"]!.AsArray());
    }

    [Fact]
    public void TestArrayRepr()
    {
        var actualJson = JsonNode.Parse(json: Array.Empty<int>()
                                                   .ReprTree())!;
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(0),
            node2: actualJson[propertyName: "dimensions"]!));
        Assert.Empty(collection: actualJson[propertyName: "value"]!.AsArray());

        actualJson = JsonNode.Parse(json: new[] { 1, 2, 3 }.ReprTree())!;
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(3),
            node2: actualJson[propertyName: "dimensions"]!));
        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.Equal(expected: "1_i32", actual: valueArray[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32", actual: valueArray[index: 1]!.GetValue<string>());
        Assert.Equal(expected: "3_i32", actual: valueArray[index: 2]!.GetValue<string>());
    }

    [Fact]
    public void TestJaggedArrayRepr()
    {
        var jagged2D = new[] { new[] { 1, 2 }, new[] { 3 } };
        var actualJson = JsonNode.Parse(json: jagged2D.ReprTree())!;

        // Check outer jagged array properties
        Assert.Equal(expected: "JaggedArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 1, actual: actualJson[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: actualJson[propertyName: "dimensions"]!));
        Assert.Equal(expected: "1DArray", actual: actualJson[propertyName: "elementType"]
          ?.ToString());

        // Check the nested arrays structure
        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: outerArray.Count);

        // First inner array: int[] { 1, 2 }
        var innerArray1Json = outerArray[index: 0]!;
        Assert.Equal(expected: "1DArray", actual: innerArray1Json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: innerArray1Json[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: innerArray1Json[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2),
            node2: innerArray1Json[propertyName: "dimensions"]!));
        Assert.Equal(expected: "int", actual: innerArray1Json[propertyName: "elementType"]
          ?.ToString());

        var innerArray1Values = innerArray1Json[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray1Values.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "1_i32",
            node2: innerArray1Values[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "2_i32",
            node2: innerArray1Values[index: 1]));

        // Second inner array: int[] { 3 }
        var innerArray2Json = outerArray[index: 1]!;
        Assert.Equal(expected: "1DArray", actual: innerArray2Json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: innerArray2Json[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: innerArray2Json[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(1),
            node2: innerArray2Json[propertyName: "dimensions"]!));
        Assert.Equal(expected: "int", actual: innerArray2Json[propertyName: "elementType"]
          ?.ToString());

        var innerArray2Values = innerArray2Json[propertyName: "value"]!.AsArray();
        Assert.Single(collection: innerArray2Values);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "3_i32",
            node2: innerArray2Values[index: 0]));
    }

    [Fact]
    public void TestMultidimensionalArrayRepr()
    {
        var array2D = new[,] { { 1, 2 }, { 3, 4 } };
        var actualJson = JsonNode.Parse(json: array2D.ReprTree())!;

        Assert.Equal(expected: "2DArray", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "rank"]!.GetValue<int>());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray(2, 2),
            node2: actualJson[propertyName: "dimensions"]!));

        var outerArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: outerArray.Count);

        var innerArray1 = outerArray[index: 0]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray1.Count);
        Assert.Equal(expected: "1_i32", actual: innerArray1[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32", actual: innerArray1[index: 1]!.GetValue<string>());

        var innerArray2 = outerArray[index: 1]!.AsArray();
        Assert.Equal(expected: 2, actual: innerArray2.Count);
        Assert.Equal(expected: "3_i32", actual: innerArray2[index: 0]!.GetValue<string>());
        Assert.Equal(expected: "4_i32", actual: innerArray2[index: 1]!.GetValue<string>());
    }

    [Fact]
    public void TestHashSetRepr()
    {
        var set = new HashSet<int> { 1, 2 };
        var actualJson = JsonNode.Parse(json: set.ReprTree())!;

        Assert.Equal(expected: "HashSet", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Contains(collection: valueArray,
            filter: item => JsonNode.DeepEquals(node1: item, node2: "1_i32"));
        Assert.Contains(collection: valueArray,
            filter: item => JsonNode.DeepEquals(node1: item, node2: "2_i32"));
    }

    [Fact]
    public void TestSortedSetRepr()
    {
        var set = new SortedSet<int> { 3, 1, 2 };
        var actualJson = JsonNode.Parse(json: set.ReprTree())!;

        Assert.Equal(expected: "SortedSet", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "1_i32",
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "2_i32",
            node2: valueArray[index: 1]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "3_i32",
            node2: valueArray[index: 2]));
    }

    [Fact]
    public void TestQueueRepr()
    {
        var queue = new Queue<string>();
        queue.Enqueue(item: "first");
        queue.Enqueue(item: "second");
        var actualJson = JsonNode.Parse(json: queue.ReprTree())!;

        Assert.Equal(expected: "Queue", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);

        var item1 = valueArray[index: 0]!.AsObject();
        Assert.Equal(expected: "string", actual: item1[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item1[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item1[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: item1[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "first", actual: item1[propertyName: "value"]
          ?.ToString());

        var item2 = valueArray[index: 1]!.AsObject();
        Assert.Equal(expected: "string", actual: item2[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: item2[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: item2[propertyName: "hashCode"]);
        Assert.Equal(expected: 6, actual: item2[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "second", actual: item2[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestStackRepr()
    {
        var stack = new Stack<int>();
        stack.Push(item: 1);
        stack.Push(item: 2);
        var actualJson = JsonNode.Parse(json: stack.ReprTree())!;

        Assert.Equal(expected: "Stack", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: valueArray.Count);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "2_i32",
            node2: valueArray[index: 0]));
        Assert.True(condition: JsonNode.DeepEquals(
            node1: "1_i32",
            node2: valueArray[index: 1]));
    }

    [Fact]
    public void TestPriorityQueueRepr()
    {
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue(element: "second", priority: 2);
        pq.Enqueue(element: "first", priority: 1);
        pq.Enqueue(element: "third", priority: 3);

        var actualJson = JsonNode.Parse(json: pq.ReprTree())!;

        Assert.Equal(expected: "PriorityQueue", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: 3, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "elementType"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: actualJson[propertyName: "priorityType"]
          ?.ToString());

        var valueArray = actualJson[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 3, actual: valueArray.Count);

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "first" &&
            item[propertyName: "priority"]!.GetValue<string>() == "1_i32");

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "second" &&
            item[propertyName: "priority"]!.GetValue<string>() == "2_i32");

        Assert.Contains(collection: valueArray, filter: item =>
            item![propertyName: "element"]![propertyName: "value"]!.GetValue<string>() ==
            "third" &&
            item[propertyName: "priority"]!.GetValue<string>() == "3_i32");
    }
}