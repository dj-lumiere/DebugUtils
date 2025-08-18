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
        Assert.Equal(expected: "Point(X: 10_i32, Y: 20_i32)", actual: point.Repr());
    }

    [Fact]
    public void TestCustomStructRepr_WithToString()
    {
        var custom = new CustomStruct { Name = "test", Value = 42 };
        Assert.Equal(expected: "CustomStruct(Name: \"test\", Value: 42_i32)",
            actual: custom.Repr());
    }

    [Fact]
    public void TestClassRepr_WithToString()
    {
        var person = new Person(name: "Alice", age: 30);
        Assert.Equal(expected: "Person(Age: 30_i32, Name: \"Alice\")", actual: person.Repr());
    }

    [Fact]
    public void TestClassRepr_NoToString()
    {
        var noToString = new NoToStringClass(data: "data", number: 123);
        Assert.Equal(expected: "NoToStringClass(Data: \"data\", Number: 123_i32)",
            actual: noToString.Repr());
    }

    [Fact]
    public void TestRecordRepr()
    {
        var settings = new TestSettings(EquipmentName: "Printer",
            EquipmentSettings: new Dictionary<string, double>
                { [key: "Temp (C)"] = 200.0, [key: "PrintSpeed (mm/s)"] = 30.0 });
        var result = settings.Repr();
        Assert.Equal(
            expected:
            "TestSettings({ EquipmentName: \"Printer\", EquipmentSettings: {\"Temp (C)\": 200_f64, \"PrintSpeed (mm/s)\": 30_f64} })",
            actual: result);
    }

    [Fact]
    public void TestEnumRepr()
    {
        Assert.Equal(expected: "Colors.GREEN (1_i32)", actual: Colors.GREEN.Repr());
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