using System.Text.Json.Nodes;
using DebugUtils.Repr;
using DebugUtils.Tests.TestModels;

namespace DebugUtils.Tests;

// Test data structures from ReprTest.cs

public class GenericFormatterTests
{
    // Custom Types
    [Fact]
    public void TestCustomStructRepr_NoToString()
    {
        var point = new Point { X = 10, Y = 20 };
        Assert.Equal(expected: "Point(X: int(10), Y: int(20))", actual: point.Repr());
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        Assert.Equal(expected: "CustomStruct(Name: \"test\", Value: int(42))", actual: custom.Repr());
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        Assert.Equal(expected: "Person(Name: \"Alice\", Age: int(30))", actual: person.Repr());
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        Assert.Equal(expected: "NoToStringClass(Data: \"data\", Number: int(123))",
            actual: noToString.Repr());
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
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "double", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "200"
            },
            node2: tempSetting[propertyName: "value"]));

        var speedSetting =
            settingsArray.FirstOrDefault(predicate: s =>
                s![propertyName: "key"]![propertyName: "value"]!.ToString() ==
                "PrintSpeed (mm/s)");
        Assert.NotNull(@object: speedSetting);
        Assert.True(condition: JsonNode.DeepEquals(
            node1: new JsonObject
            {
                [propertyName: "type"] = "double", [propertyName: "kind"] = "struct",
                [propertyName: "value"] = "30"
            },
            node2: speedSetting[propertyName: "value"]));
    }

    [Fact]
    public void TestEnumRepr()
    {
        Assert.Equal(expected: "Colors.GREEN (int(1))", actual: Colors.GREEN.Repr());
    }

    [Fact]
    public void TestCircularReference()
    {
        var a = new List<object>();
        a.Add(item: a);
        var repr = a.Repr();
        // object hash code can be different.
        Assert.StartsWith(expectedStartString: "[<Circular Reference to List @",
            actualString: repr);
        Assert.EndsWith(expectedEndString: ">]", actualString: repr);
    }
}