namespace DebugUtils.Tests.TestModels;

public class ClassifiedData(string writer, string data)
{
    public string Writer { get; set; } = writer;
    private string Data { get; set; } = data;
}