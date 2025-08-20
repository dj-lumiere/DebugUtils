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

        var ageNode = actualJson[propertyName: "Age"]!;
        Assert.Equal(expected: "30_i32", actual: ageNode.GetValue<string>());

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

        var ageNode = actualJson[propertyName: "Age"]!;
        Assert.Equal(expected: "30_i32", actual: ageNode.GetValue<string>());
    }
    [Fact]
    public void TestReprConfig_MaxDepth_ReprTree()
    {
        var nestedList = new List<object> { 1, new List<object> { 2, new List<object> { 3 } } };
        var config = ReprConfig.Configure()
                               .MaxDepth(1)
                               .Build();
        var actualJson = JsonNode.Parse(json: nestedList.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 2, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: "1_i32",
            actual: actualJson[propertyName: "value"]![index: 0]!.GetValue<string>());
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

        config = ReprConfig.Configure()
                           .MaxDepth(0)
                           .Build();
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
        var config = ReprConfig.Configure()
                               .MaxItems(3)
                               .Build();
        var actualJson = JsonNode.Parse(json: list.ReprTree(config: config))!;
        Assert.Equal(expected: "List", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 5, actual: actualJson[propertyName: "count"]!.GetValue<int>());
        Assert.Equal(expected: 4, actual: actualJson[propertyName: "value"]!.AsArray()
           .Count);
        Assert.Equal(expected: "1_i32",
            actual: actualJson[propertyName: "value"]![index: 0]!.GetValue<string>());
        Assert.Equal(expected: "2_i32",
            actual: actualJson[propertyName: "value"]![index: 1]!.GetValue<string>());
        Assert.Equal(expected: "3_i32",
            actual: actualJson[propertyName: "value"]![index: 2]!.GetValue<string>());
        Assert.Equal(expected: "... (2 more items)",
            actual: actualJson[propertyName: "value"]![index: 3]
              ?.ToString());

        config = ReprConfig.Configure()
                           .MaxItems(0)
                           .Build();
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
        var config = ReprConfig.Configure()
                               .MaxStringLength(10)
                               .Build();
        var actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "This is a ... (42 more letters)",
            actual: actualJson[propertyName: "value"]
              ?.ToString());

        config = ReprConfig.Configure()
                           .MaxStringLength(0)
                           .Build();
        actualJson = JsonNode.Parse(json: longString.ReprTree(config: config))!;
        Assert.Equal(expected: "string", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "... (52 more letters)", actual: actualJson[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_ShowNonPublicProperties_ReprTree()
    {
        var classified =
            new ClassifiedData(writer: "writer", dataValue: "secret", password: "REDACTED");
        var config = ReprConfig.Configure()
                               .ViewMode(MemberReprMode.PublicFieldAutoProperty)
                               .Build();
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


        config = ReprConfig.Configure()
                           .ViewMode(MemberReprMode.AllFieldAutoProperty)
                           .Build();
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

        var secretDataNode = actualJson[propertyName: "private_Data"];
        Assert.NotNull(@object: secretDataNode);
        Assert.Equal(expected: "string", actual: secretDataNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 6, actual: secretDataNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "secret", actual: secretDataNode[propertyName: "value"]
          ?.ToString());

        var secretPasswordNode = actualJson[propertyName: "private_Password"];
        Assert.NotNull(@object: secretPasswordNode);
        Assert.Equal(expected: "string", actual: secretPasswordNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 8,
            actual: secretPasswordNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "REDACTED", actual: secretPasswordNode[propertyName: "value"]
          ?.ToString());
    }

    [Fact]
    public void TestReprConfig_AllPublicMode_ReprTree()
    {
        var classified = new ClassifiedData(writer: "Lumi", dataValue: "Now Top Secret Accessing",
            password: "REDACTED");

        var config = new ReprConfig(ViewMode: MemberReprMode.AllPublic);
        var actualJson = JsonNode.Parse(classified.ReprTree(config))!;

        // Should include all public fields and properties
        Assert.Equal("ClassifiedData", actualJson["type"]
          ?.ToString());
        Assert.Equal("class", actualJson["kind"]
          ?.ToString());
        Assert.NotNull(actualJson["hashCode"]);

        // Public fields
        Assert.Equal("10_i32", actualJson["Age"]
          ?.ToString());
        Assert.Equal("5_i64", actualJson["Id"]
          ?.ToString());

        // Public auto-properties  
        var nameNode = actualJson["Name"]!.AsObject();
        Assert.Equal("string", nameNode["type"]
          ?.ToString());
        Assert.Equal("Lumi", nameNode["value"]
          ?.ToString());

        var writerNode = actualJson["Writer"]!.AsObject();
        Assert.Equal("string", writerNode["type"]
          ?.ToString());
        Assert.Equal("Lumi", writerNode["value"]
          ?.ToString());

        // Public computed property
        var realDateNode = actualJson["RealDate"]!.AsObject();
        Assert.Equal("DateTimeOffset", realDateNode["type"]
          ?.ToString());
        Assert.Equal("1970", realDateNode["year"]
          ?.ToString());

        // Should NOT include private members
        Assert.Null(actualJson["private_Date"]);
        Assert.Null(actualJson["private_Password"]);
        Assert.Null(actualJson["private_Data"]);
        Assert.Null(actualJson["private_Key"]);
    }

    [Fact]
    public void TestReprConfig_EverythingMode_ReprTree()
    {
        var classified = new ClassifiedData(writer: "Lumi", dataValue: "Now Top Secret Accessing",
            password: "REDACTED");

        var config = new ReprConfig(ViewMode: MemberReprMode.Everything);
        var actualJson = JsonNode.Parse(classified.ReprTree(config))!;

        // Should include all public members
        Assert.Equal("ClassifiedData", actualJson["type"]
          ?.ToString());
        Assert.Equal("10_i32", actualJson["Age"]
          ?.ToString());
        Assert.Equal("5_i64", actualJson["Id"]
          ?.ToString());

        // Should include private fields
        var dateNode = actualJson["private_Date"]!.AsObject();
        Assert.Equal("DateTime", dateNode["type"]
          ?.ToString());
        Assert.Equal("1970", dateNode["year"]
          ?.ToString());

        var passwordNode = actualJson["private_Password"]!.AsObject();
        Assert.Equal("string", passwordNode["type"]
          ?.ToString());
        Assert.Equal("REDACTED", passwordNode["value"]
          ?.ToString());

        var dataNode = actualJson["private_Data"]!.AsObject();
        Assert.Equal("string", dataNode["type"]
          ?.ToString());
        Assert.Equal("Now Top Secret Accessing", dataNode["value"]
          ?.ToString());

        var keyNode = actualJson["private_Key"]!.AsObject();
        Assert.Equal("Guid", keyNode["type"]
          ?.ToString());
        Assert.Equal("9a374b45-3771-4e91-b5e9-64bfa545efe9", keyNode["value"]
          ?.ToString());

        // Should include private computed properties
        Assert.NotNull(actualJson["private_DataChecksum"]);
        Assert.NotNull(actualJson["private_Hash"]);
        Assert.NotNull(actualJson["private_keyInt"]);
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
        Assert.Equal(expected: "3.14_f32",
            actual: actualJson[propertyName: "x"]!.GetValue<string>());
        Assert.Equal(expected: "2.71_f32",
            actual: actualJson[propertyName: "y"]!.GetValue<string>());
    }
}