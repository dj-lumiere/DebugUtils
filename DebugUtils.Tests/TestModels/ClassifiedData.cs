namespace DebugUtils.Tests.TestModels;

public class ClassifiedData(string writer, string data, string password)
{
    public long Id = 5;
    public string Writer { get; set; } = writer;
    private string Data { get; set; } = data;
    private DateTime Date = DateTime.UnixEpoch;
    public int Age = 10;
    private Guid Key { get; set; } = new ("9a374b45-3771-4e91-b5e9-64bfa545efe9");
    public string Name { get; set; } = "Lumi";
    private string Password = password;
}