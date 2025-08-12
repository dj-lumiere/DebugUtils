using System.Text.Json.Nodes;
using DebugUtils.Repr;

namespace DebugUtils.Tests;

public class FunctionFormatterTreeTests
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
        await Task.Delay(millisecondsDelay: 1);
        return a;
    }
    
    [Fact]
    public void TestFunctionHierarchical()
    {
        var add5 = (int a) => a + 11;
        var a = Add;
        var b = Add2;
        var c = Add3<short>;
        var d = Add4;
        var e = Lambda;
        var nullJsonObject = new JsonObject
        {
            [propertyName: "type"] = "object", [propertyName: "kind"] = "class",
            [propertyName: "value"] = null
        };

        var actualJson = JsonNode.Parse(json: add5.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        var props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Lambda", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("internal"),
            node2: props[propertyName: "modifiers"]));
        var parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Single(collection: parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: a.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "int", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("public", "static"),
            node2: props[propertyName: "modifiers"]));
        parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Equal(expected: 2, actual: parameterArray.Count);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
        Assert.Equal(expected: "int", actual: parameterArray[index: 1]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "b", actual: parameterArray[index: 1]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 1]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 1]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: b.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add2", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "long", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("internal", "static"),
            node2: props[propertyName: "modifiers"]));
        parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Single(collection: parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: c.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add3", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "short", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "generic"),
            node2: props[propertyName: "modifiers"]));
        parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Single(collection: parameterArray);
        Assert.Equal(expected: "short", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: d.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Add4", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "void", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "static"),
            node2: props[propertyName: "modifiers"]));
        parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Equal(expected: 2, actual: parameterArray.Count);
        Assert.Equal(expected: "ref int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "in", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
        Assert.Equal(expected: "ref int", actual: parameterArray[index: 1]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "b", actual: parameterArray[index: 1]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "out", actual: parameterArray[index: 1]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 1]![propertyName: "defaultValue"]!));

        actualJson = JsonNode.Parse(json: e.ReprTree())!;
        Assert.Equal(expected: "Function", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        props = actualJson[propertyName: "properties"]!.AsObject();
        Assert.Equal(expected: "Lambda", actual: props[propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "Task<int>", actual: props[propertyName: "returnType"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: new JsonArray("private", "async"),
            node2: props[propertyName: "modifiers"]));
        parameterArray = props[propertyName: "parameters"]!.AsArray();
        Assert.Single(collection: parameterArray);
        Assert.Equal(expected: "int", actual: parameterArray[index: 0]![propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "a", actual: parameterArray[index: 0]![propertyName: "name"]
          ?.ToString());
        Assert.Equal(expected: "", actual: parameterArray[index: 0]![propertyName: "modifier"]
          ?.ToString());
        Assert.True(condition: JsonNode.DeepEquals(node1: nullJsonObject,
            node2: parameterArray[index: 0]![propertyName: "defaultValue"]!));
    }
}