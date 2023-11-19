// See https://aka.ms/new-console-template for more information

using ConsoleApp1;

Console.WriteLine("----");
var test = new TestTemplateBase();
test.Colors = new TestTemplateBase.ColorsItem[]
{
    new TestTemplateBase.ColorsItem()
};
Console.Write(test);
Console.WriteLine("----");
