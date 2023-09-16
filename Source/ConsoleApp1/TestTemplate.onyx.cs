namespace ConsoleApp1;

public class TestTemplate : TestTemplateBase
{
    // protected override string GetName()
    // {
    //     return "Jonesy";
    // }

    protected override string GetThings()
    {
        return "\"Hello\" +\n\"World\"";
    }
}