namespace DebugUtils.Tests.TestModels;

public class Student
{
    public required string Name { get; set; }
    public int Age { get; set; }
    public required List<string> Hobbies { get; set; }
}