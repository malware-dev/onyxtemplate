namespace ConsoleApp1;

class TestTemplate : TestTemplateBase
{
    class Culr : ColorsItemBase
    {
        public override string GetKey() => "Keyyy";
    }
    
    protected override string GetNamespace() => "Numspac";

    protected override IEnumerable<ColorsItemBase> GetColors()
    {
        yield return new Culr();
        yield return new Culr();
        yield return new Culr();
    }
}
