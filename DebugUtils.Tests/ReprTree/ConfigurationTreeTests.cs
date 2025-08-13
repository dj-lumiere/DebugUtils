using System.Text.Json.Nodes;
using DebugUtils.Repr;
using DebugUtils.Tests.TestModels;

namespace DebugUtils.Tests;

public class ConfigurationTreeTests
{
    [Fact]
    public void TestReadme()
    {
        var student = new Student
        {
            Name = "Alice",
            Age = 30,
            Hobbies = new List<string> { "reading", "coding" }
        };
        var actualJson = JsonNode.Parse(json: student.ReprTree()) ?? new JsonObject();

        Assert.Equal(expected: "Student", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!.AsObject();
        Assert.Equal(expected: "int", actual: ageNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "30", actual: ageNode[propertyName: "value"]
          ?.ToString());

        var hobbiesNode = actualJson[propertyName: "Hobbies"]!.AsObject();
        Assert.Equal(expected: "List", actual: hobbiesNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: hobbiesNode[propertyName: "count"]!.GetValue<int>());

        var hobbiesValue = hobbiesNode[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: "reading",
            actual: hobbiesValue[index: 0]![propertyName: "value"]!.GetValue<string>());
        Assert.Equal(expected: "coding",
            actual: hobbiesValue[index: 1]![propertyName: "value"]!.GetValue<string>());
    }

    [Fact]
    public void TestExample()
    {
        var person = new Person(name: "John", age: 30);
        var actualJson = JsonNode.Parse(json: person.ReprTree()) ?? new JsonObject();

        Assert.Equal(expected: "Person", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 4, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "John", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!.AsObject();
        Assert.Equal(expected: "int", actual: ageNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "30", actual: ageNode[propertyName: "value"]
          ?.ToString());
    }
    [Fact]
    public void TestReprConfig_MaxDepth_ReprTree()
    {
        var nestedList = new List<object> { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = new ReprConfig(MaxDepth: 1);
        var actualJson = JsonNode.Parse(json: nestedList.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "struct",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "kind"]
              ?.ToString());
        Assert.Equal(expected: "1",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "value"]
              ?.ToString());
        Assert.Equal(expected: "List",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "class",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "kind"]
              ?.ToString());
        Assert.Equal(expected: "true",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "maxDepthReached"]
              ?.ToString());
        Assert.Equal(expected: 1,
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "depth"]!
               .GetValue<int>());

        config = new ReprConfig(MaxDepth: 0);
        actualJson = JsonNode.Parse(json: nestedList.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Equal(expected: "true", actual: actualJson[propertyName: "maxDepthReached"]
          ?.ToString());
        Assert.Equal(expected: 0, actual: actualJson[propertyName: "depth"]!.GetValue<int>());
    }

    [Fact]
    public void TestReprConfig_MaxCollectionItems_ReprTree()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var config = new ReprConfig(MaxElementsPerCollection: 3);
        var actualJson = JsonNode.Parse(json: list.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: 4, actual: actualJson[propertyName: "value"]!.AsArray()
           .Count);
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 0]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 1]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "int",
            actual: actualJson[propertyName: "value"]![index: 2]![propertyName: "type"]
              ?.ToString());
        Assert.Equal(expected: "... (2 more items)",
            actual: actualJson[propertyName: "value"]![index: 3]
              ?.ToString());

        config = new ReprConfig(MaxElementsPerCollection: 0);
        actualJson = JsonNode.Parse(json: list.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "... (5 more items)",
            actual: actualJson[propertyName: "value"]![index: 0]
              ?.ToString());
    }

    [Fact]
    public void TestReprConfig_MaxStringLength_ReprTree()
    {
        var longString = "This is a very long string that should be truncated.";
        var config = new ReprConfig(MaxStringLength: 10);
        var actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "This is a ... (42 more letters)",
            actual: actualJson[propertyName: "value"]
              ?.ToString());

        config = new ReprConfig(MaxStringLength: 0);
        actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "... (52 more letters)", actual: actualJson[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_ShowNonPublicProperties_ReprTree()
    {
        var classified = new ClassifiedData(writer: "writer", data: "secret");
        var config = new ReprConfig(ShowNonPublicProperties: false);
        var actualJson = JsonNode.Parse(json: classified.ReprTree(config: config));
        Assert.NotNull(@object: actualJson);
        Assert.Equal(expected: "ClassifiedData", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var writerNode = actualJson[propertyName: "Writer"]!.AsObject();
        Assert.Equal(expected: "string", actual: writerNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: writerNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "writer", actual: writerNode[propertyName: "value"]
          ?.ToString());


        config = new ReprConfig(ShowNonPublicProperties: true);
        actualJson = JsonNode.Parse(json: classified.ReprTree(config: config));
        Assert.NotNull(@object: actualJson);
        Assert.Equal(expected: "ClassifiedData", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);


        writerNode = actualJson[propertyName: "Writer"]!.AsObject();
        Assert.Equal(expected: "string", actual: writerNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: writerNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "writer", actual: writerNode[propertyName: "value"]
          ?.ToString());

        var secretNode = actualJson[propertyName: "private_Data"];
        Assert.NotNull(@object: secretNode);
        Assert.Equal(expected: "string", actual: secretNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: secretNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "secret", actual: secretNode[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestReprTree_WithFloats()
    {
        var a = new { x = 3.14f, y = 2.71f };
        var actualJson = JsonNode.Parse(json: a.ReprTree())!;
        Assert.Equal(expected: "Anonymous", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);
        Assert.Equal(expected: "3.14",
            actual: actualJson[propertyName: "x"]?[propertyName: "value"]
              ?.ToString());
        Assert.Equal(expected: "2.71",
            actual: actualJson[propertyName: "y"]?[propertyName: "value"]
              ?.ToString());
    }
}