
using ExcelDna.Integration;

public static class MyFunctions
{
    [ExcelFunction(Description = "My first .NET function")]
    public static string HelloDna(string name)
    {
        return "Hello " + name;
    }
}
