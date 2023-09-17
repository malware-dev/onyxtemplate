namespace ConsoleApp1;

class TestTemplate : TestTemplateBase
{
    class Animal : AnimalsItemBase
    {
        readonly string _name;
        readonly string _type;

        public Animal(string name, string type)
        {
            _name = name;
            _type = type;
        }

        public override string GetName() => _name;
        public override string GetType() => _type;
    }
    
    protected override IEnumerable<AnimalsItemBase> GetAnimals()
    {
        yield return new Animal("Dog", "Canidae");
        yield return new Animal("Cat", "Felidae");
        yield return new Animal("Tiger", "Felidae");
        yield return new Animal("Bear", "Felidae");
    }
}