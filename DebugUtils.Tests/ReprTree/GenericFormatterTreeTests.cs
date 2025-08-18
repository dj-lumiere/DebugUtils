using System.Text.Json.Nodes;
using DebugUtils.Repr;
using DebugUtils.Tests.TestModels;

namespace DebugUtils.Tests;

public class GenericFormatterTreeTests
{
    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        var actualJson = JsonNode.Parse(json: point.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Point",
            [propertyName: "kind"] = "struct",
            [propertyName: "X"] = "10_i32",
            [propertyName: "Y"] = "20_i32"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        var actualJson = JsonNode.Parse(json: custom.ReprTree())!;

        Assert.Equal(expected: "CustomStruct", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "struct", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.Null(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 4, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "test", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var valueNode = actualJson[propertyName: "Value"]!;
        Assert.Equal(expected: "42_i32", actual: valueNode.GetValue<string>());
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        var actualJson = JsonNode.Parse(json: person.ReprTree())!;

        Assert.Equal(expected: "Person", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var nameNode = actualJson[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJson[propertyName: "Age"]!;
        Assert.Equal(expected: "30_i32", actual: ageNode.GetValue<string>());
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        var actualJson = JsonNode.Parse(json: noToString.ReprTree())!;

        Assert.Equal(expected: "NoToStringClass", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var dataNode = actualJson[propertyName: "Data"]!.AsObject();
        Assert.Equal(expected: "string", actual: dataNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: dataNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: dataNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 4, actual: dataNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "data", actual: dataNode[propertyName: "value"]
          ?.ToString());

        var numberNode = actualJson[propertyName: "Number"]!;
        Assert.Equal(expected: "123_i32", actual: numberNode.GetValue<string>());
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings(EquipmentName: "Printer",
            EquipmentSettings: new Dictionary<string, double>
                { [key: "Temp (C)"] = 200.0, [key: "PrintSpeed (mm/s)"] = 30.0 });
        var actualJson = JsonNode.Parse(json: settings.ReprTree())!;

        Assert.Equal(expected: "TestSettings", actual: actualJson[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "record class", actual: actualJson[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJson[propertyName: "hashCode"]);

        var equipmentName = actualJson[propertyName: "EquipmentName"]!.AsObject();
        Assert.Equal(expected: "string", actual: equipmentName[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: equipmentName[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: equipmentName[propertyName: "hashCode"]);
        Assert.Equal(expected: 7, actual: equipmentName[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Printer", actual: equipmentName[propertyName: "value"]
          ?.ToString());

        var equipmentSettings = actualJson[propertyName: "EquipmentSettings"]!.AsObject();
        Assert.Equal(expected: "Dictionary", actual: equipmentSettings[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: equipmentSettings[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: equipmentSettings[propertyName: "hashCode"]);
        Assert.Equal(expected: 2,
            actual: equipmentSettings[propertyName: "count"]!.GetValue<int>());

        var settingsArray = equipmentSettings[propertyName: "value"]!.AsArray();
        Assert.Equal(expected: 2, actual: settingsArray.Count);

        // Since dictionary order isn't guaranteed, we check for presence of keys
        var tempSetting =
            settingsArray.FirstOrDefault(predicate: s =>
                s![propertyName: "key"]![propertyName: "value"]!.ToString() == "Temp (C)");
        Assert.NotNull(@object: tempSetting);
        Assert.Equal(expected: "200_f64",
            actual: tempSetting[propertyName: "value"]!.GetValue<string>());

        var speedSetting =
            settingsArray.FirstOrDefault(predicate: s =>
                s![propertyName: "key"]![propertyName: "value"]!.ToString() ==
                "PrintSpeed (mm/s)");
        Assert.NotNull(@object: speedSetting);
        Assert.Equal(expected: "30_f64", actual: speedSetting[propertyName: "value"]!.ToString());
    }


    [Fact]
    public void TestEnumRepr()
    {
        var actualJson = JsonNode.Parse(json: Colors.GREEN.ReprTree());
        var expectedJson = new JsonObject
        {
            [propertyName: "type"] = "Colors",
            [propertyName: "kind"] = "enum",
            [propertyName: "name"] = "GREEN",
            [propertyName: "value"] = "1_i32"
        };
        Assert.True(condition: JsonNode.DeepEquals(node1: actualJson, node2: expectedJson));
    }


    [Fact]
    public void TestObjectReprTree()
    {
        var data = new { Name = "Alice", Age = 30 };
        var actualJsonNode = JsonNode.Parse(json: data.ReprTree())!;

        Assert.Equal(expected: "Anonymous", actual: actualJsonNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: actualJsonNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: actualJsonNode[propertyName: "hashCode"]);

        var nameNode = actualJsonNode[propertyName: "Name"]!.AsObject();
        Assert.Equal(expected: "string", actual: nameNode[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "class", actual: nameNode[propertyName: "kind"]
          ?.ToString());
        Assert.NotNull(@object: nameNode[propertyName: "hashCode"]);
        Assert.Equal(expected: 5, actual: nameNode[propertyName: "length"]!.GetValue<int>());
        Assert.Equal(expected: "Alice", actual: nameNode[propertyName: "value"]
          ?.ToString());

        var ageNode = actualJsonNode[propertyName: "Age"]!;
        Assert.Equal(expected: "30_i32", actual: ageNode.GetValue<string>());
    }

    [Fact]
    public void TestCircularReprTree()
    {
        List<object> a = new();
        a.Add(item: a);
        var actualJsonString = a.ReprTree();

        // Parse the JSON to verify structure
        var json = JsonNode.Parse(json: actualJsonString)!;

        // Verify top-level structure
        Assert.Equal(expected: "List", actual: json[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: 1, actual: json[propertyName: "count"]!.GetValue<int>());

        // Verify circular reference structure
        var firstElement = json[propertyName: "value"]![index: 0]!;
        Assert.Equal(expected: "CircularReference", actual: firstElement[propertyName: "type"]
          ?.ToString());
        Assert.Equal(expected: "List",
            actual: firstElement[propertyName: "target"]![propertyName: "type"]
              ?.ToString());
        Assert.StartsWith(expectedStartString: "0x",
            actualString: firstElement[propertyName: "target"]![propertyName: "hashCode"]
              ?.ToString());
    }
}